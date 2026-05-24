import { test, expect } from '@playwright/test';

// SPEC-SKILL-002: スキルレベル定義拡充（ITSS準拠）・コンピテンシー評価 API テスト（TASK-071）
// 仕様書: docs/spec/SPEC-SKILL-002-skill-level-itss.md
// 設計書: docs/design/SPEC-SKILL-002-design.md
// 生成日: 2026-05-24（/test-write により自動生成）
//
// 対象 AC:
//   AC-01-03: 移行ツールによるスケール変換（preview + execute）
//   AC-03-01: コンピテンシー評価の入力（POST/GET /api/competency-evaluations）
//   AC-04-01: 自己評価と上長評価の比較表示（GET /api/skills/gap）
//   AC-04-02: 大きなギャップのアラート（threshold による requiresAttention）
//   AC-05-01（データ層）: コンピテンシーヒートマップデータ取得（GET /api/skill-map/competency）
//
// スキップ対象 AC（FE タスク対応）:
//   AC-01-01, AC-01-02, AC-02-01 → TASK-070 の API テストでカバー済み
//   AC-03-02                    → TASK-070 のコンピテンシーマスタ API でカバー済み
//   AC-05-01（UI）              → TASK-073（FE）で E2E テスト実施予定

const API = 'http://localhost:5108/api';

let adminId = '';
let managerId = '';
let talentId = '';
let periodId = '';
let competencyId = '';

test.beforeAll(async ({ request }) => {
  // ── ユーザー取得 ──────────────────────────────────────────────────────────
  const usersRes = await request.get(`${API}/users`);
  expect(usersRes.ok()).toBeTruthy();
  const users: Array<{ id: string; displayName: string; role: string }> = await usersRes.json();

  const admin = users.find(u => u.role === 'Admin');
  if (!admin) throw new Error('シードデータに Admin ユーザーが見つかりません');
  adminId = admin.id;

  const manager = users.find(u => u.displayName === '山田 一郎');
  if (!manager) throw new Error('シードデータに 山田 一郎 が見つかりません');
  managerId = manager.id;

  const talent = users.find(u => u.displayName === '田中 太郎');
  if (!talent) throw new Error('シードデータに 田中 太郎 が見つかりません');
  talentId = talent.id;

  // ── 考課期間取得 ──────────────────────────────────────────────────────────
  const periodsRes = await request.get(`${API}/periods`);
  const periods: Array<{ id: string; year: number }> = await periodsRes.json();
  const currPeriod = periods.sort((a, b) => b.year - a.year)[0];
  if (!currPeriod) throw new Error('考課期間が見つかりません');
  periodId = currPeriod.id;

  // ── コンピテンシーマスタ作成（テスト用）──────────────────────────────────
  const createRes = await request.post(`${API}/competency-masters?requesterId=${adminId}`, {
    data: {
      category: 'Common',
      name: `テスト用コンピテンシー_${Date.now()}`,
      description: 'E2Eテスト用',
      displayOrder: 99,
    },
  });
  if (createRes.status() === 201) {
    const body = await createRes.json();
    competencyId = body.id;
  } else {
    // 既存のアクティブなコンピテンシーを取得
    const listRes = await request.get(`${API}/competency-masters`);
    const list: Array<{ id: string }> = (await listRes.json()).competencies ?? [];
    if (list.length === 0) throw new Error('コンピテンシーマスタが存在しません');
    competencyId = list[0].id;
  }
});

// ── AC-03-01: コンピテンシー評価の入力 ────────────────────────────────────

test.describe('AC-03-01: コンピテンシー評価の Upsert/取得', () => {
  test('AC-03-01-01: Talent が自己コンピテンシー評価を保存できる（200）', async ({ request }) => {
    const res = await request.post(`${API}/competency-evaluations?requesterId=${talentId}`, {
      data: {
        talentId,
        periodId,
        evaluatorType: 'Self',
        evaluations: [
          { competencyId, level: 3, comment: 'まずまず良好' },
        ],
      },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.savedCount).toBe(1);
  });

  test('AC-03-01-02: Manager が上長コンピテンシー評価を保存できる（200）', async ({ request }) => {
    const res = await request.post(`${API}/competency-evaluations?requesterId=${managerId}`, {
      data: {
        talentId,
        periodId,
        evaluatorType: 'Manager',
        evaluations: [
          { competencyId, level: 4, comment: '良好、継続して伸ばすこと' },
        ],
      },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.savedCount).toBe(1);
  });

  test('AC-03-01-03: 同じ評価を再送すると Upsert される（冪等）', async ({ request }) => {
    const res = await request.post(`${API}/competency-evaluations?requesterId=${talentId}`, {
      data: {
        talentId,
        periodId,
        evaluatorType: 'Self',
        evaluations: [
          { competencyId, level: 5, comment: '更新テスト' },
        ],
      },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.savedCount).toBe(1);
  });

  test('AC-03-01-04: 評価取得で Self/Manager 両方含む結果が返る（200）', async ({ request }) => {
    const res = await request.get(
      `${API}/competency-evaluations?requesterId=${managerId}&talentId=${talentId}&periodId=${periodId}`
    );
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.talentId).toBe(talentId);
    expect(body.periodId).toBe(periodId);
    expect(Array.isArray(body.evaluations)).toBe(true);

    const target = body.evaluations.find((e: { competencyId: string }) => e.competencyId === competencyId);
    expect(target).toBeDefined();
    // 自己評価（Upsert後のレベル5）
    expect(target.selfEvaluation).not.toBeNull();
    expect(target.selfEvaluation.level).toBe(5);
    // 上長評価（レベル4）
    expect(target.managerEvaluation).not.toBeNull();
    expect(target.managerEvaluation.level).toBe(4);
    // ギャップ = managerLevel - selfLevel = 4 - 5 = -1
    expect(target.gap).toBe(-1);
  });

  test('AC-03-01-05: レベル範囲外（0）は 400 を返す', async ({ request }) => {
    const res = await request.post(`${API}/competency-evaluations?requesterId=${talentId}`, {
      data: {
        talentId,
        periodId,
        evaluatorType: 'Self',
        evaluations: [{ competencyId, level: 0, comment: null }],
      },
    });
    expect(res.status()).toBe(400);
  });

  test('AC-03-01-06: Talent が他人の評価を取得しようとすると 403', async ({ request }) => {
    const otherTalent = await (async () => {
      const usersRes = await request.get(`${API}/users`);
      const users: Array<{ id: string; displayName: string; role: string }> = await usersRes.json();
      return users.find(u => u.role === 'Talent' && u.id !== talentId);
    })();
    if (!otherTalent) {
      test.skip();
      return;
    }
    const res = await request.get(
      `${API}/competency-evaluations?requesterId=${talentId}&talentId=${otherTalent.id}&periodId=${periodId}`
    );
    expect(res.status()).toBe(403);
  });
});

// ── AC-04-01 & AC-04-02: スキルギャップの可視化 ──────────────────────────

test.describe('AC-04-01 & AC-04-02: スキルギャップ API', () => {
  test('AC-04-01-01: コンピテンシーギャップが gap フィールドで返る（200）', async ({ request }) => {
    const res = await request.get(
      `${API}/skills/gap?requesterId=${managerId}&talentId=${talentId}&periodId=${periodId}&threshold=2`
    );
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.talentId).toBe(talentId);
    expect(body.periodId).toBe(periodId);
    expect(body.gapThreshold).toBe(2);
    expect(Array.isArray(body.skills)).toBe(true);
    expect(Array.isArray(body.competencies)).toBe(true);

    // コンピテンシー部分にギャップが含まれること（selfLevel=5, managerLevel=4 → gap=-1）
    const targetComp = body.competencies.find(
      (c: { competencyId: string }) => c.competencyId === competencyId
    );
    expect(targetComp).toBeDefined();
    expect(targetComp.selfLevel).toBe(5);
    expect(targetComp.managerLevel).toBe(4);
    expect(targetComp.gap).toBe(-1);
  });

  test('AC-04-02-01: ギャップが閾値以上の場合 requiresAttention = true', async ({ request }) => {
    // Manager が level 2 でさらに評価を追加 (selfLevel=5, managerLevel=2 → gap=-3)
    await request.post(`${API}/competency-evaluations?requesterId=${managerId}`, {
      data: {
        talentId,
        periodId,
        evaluatorType: 'Manager',
        evaluations: [{ competencyId, level: 2, comment: '要改善' }],
      },
    });

    const res = await request.get(
      `${API}/skills/gap?requesterId=${managerId}&talentId=${talentId}&periodId=${periodId}&threshold=2`
    );
    expect(res.status()).toBe(200);
    const body = await res.json();

    const targetComp = body.competencies.find(
      (c: { competencyId: string }) => c.competencyId === competencyId
    );
    expect(targetComp).toBeDefined();
    // |gap| = |5 - 2| = 3 >= threshold=2 → requiresAttention=true
    expect(Math.abs(targetComp.gap)).toBeGreaterThanOrEqual(2);
    expect(targetComp.requiresAttention).toBe(true);
  });

  test('AC-04-01-02: Talent が自分以外のギャップを見ようとすると 403', async ({ request }) => {
    const otherTalent = await (async () => {
      const usersRes = await request.get(`${API}/users`);
      const users: Array<{ id: string; displayName: string; role: string }> = await usersRes.json();
      return users.find(u => u.role === 'Talent' && u.id !== talentId);
    })();
    if (!otherTalent) {
      test.skip();
      return;
    }
    const res = await request.get(
      `${API}/skills/gap?requesterId=${talentId}&talentId=${otherTalent.id}&periodId=${periodId}&threshold=2`
    );
    expect(res.status()).toBe(403);
  });
});

// ── AC-05-01（データ層）: コンピテンシーヒートマップ取得 ──────────────────

test.describe('AC-05-01（データ層）: コンピテンシーヒートマップデータ取得', () => {
  test('AC-05-01-01: Senior がコンピテンシーヒートマップデータを取得できる（200）', async ({ request }) => {
    const usersRes = await request.get(`${API}/users`);
    const users: Array<{ id: string; displayName: string; role: string }> = await usersRes.json();
    const senior = users.find(u => u.role === 'Senior');
    if (!senior) {
      test.skip();
      return;
    }

    const res = await request.get(
      `${API}/skill-map/competency?requesterId=${senior.id}&periodId=${periodId}`
    );
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.periodId).toBe(periodId);
    expect(Array.isArray(body.categories)).toBe(true);
  });

  test('AC-05-01-02: Talent がヒートマップを閲覧しようとすると 403', async ({ request }) => {
    const res = await request.get(
      `${API}/skill-map/competency?requesterId=${talentId}&periodId=${periodId}`
    );
    expect(res.status()).toBe(403);
  });
});

// ── AC-01-03: 移行ツールによるスケール変換 ────────────────────────────────

test.describe('AC-01-03: 移行ツール（preview / execute）', () => {
  test('AC-01-03-01: Admin がプレビューを取得できる（200）', async ({ request }) => {
    const res = await request.get(`${API}/admin/skill-migration/preview?requesterId=${adminId}`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.conversionRule).toEqual({ '1': 1, '2': 2, '3': 3, '4': 5, '5': 7 });
    expect(typeof body.affectedCount).toBe('number');
    expect(Array.isArray(body.preview)).toBe(true);
  });

  test('AC-01-03-02: Admin 以外がプレビューを取得しようとすると 403', async ({ request }) => {
    const res = await request.get(`${API}/admin/skill-migration/preview?requesterId=${talentId}`);
    expect(res.status()).toBe(403);
  });

  // NOTE: migrate/execute は副作用が大きいため実行テストは統合テスト環境でのみ実施
  // AC-01-03-03（移行実行）は TASK-074 の統合テストで確認
  test.skip('AC-01-03-03: Admin が移行を実行できる（200）', async ({ request }) => {
    const res = await request.post(`${API}/admin/skill-migration/execute?requesterId=${adminId}`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.success).toBe(true);
    expect(typeof body.affectedCount).toBe('number');
    expect(body.migrationLogId).toBeTruthy();
  });

  test('AC-01-03-04: 二重実行は 409 を返す（移行ログ存在時）', async ({ request }) => {
    // 移行ログが存在する場合のみ検証（事前実行済みの環境向け）
    const previewRes = await request.get(`${API}/admin/skill-migration/preview?requesterId=${adminId}`);
    expect(previewRes.ok()).toBeTruthy();

    // ログが既にある場合は2回目の実行で 409 になることを確認
    const firstRes = await request.post(`${API}/admin/skill-migration/execute?requesterId=${adminId}`);
    if (firstRes.status() === 200) {
      // 初回実行成功 → 2回目で 409
      const secondRes = await request.post(`${API}/admin/skill-migration/execute?requesterId=${adminId}`);
      expect(secondRes.status()).toBe(409);
    } else {
      // 既に実行済みのため 409
      expect(firstRes.status()).toBe(409);
    }
  });
});
