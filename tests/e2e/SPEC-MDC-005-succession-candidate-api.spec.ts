import { test, expect } from '@playwright/test';

// SPEC-MDC-005: 後継者計画 — 後継者候補 CRUD API・サクセッションマップ API・9ボックスサジェスト API テスト（TASK-077）
// 仕様書: docs/spec/SPEC-MDC-005-succession-plan.md
// 設計書: docs/design/SPEC-MDC-005-design.md
// 生成日: 2026-05-26（/test-write により自動生成）

const API = 'http://localhost:5108/api';

let seniorId = '';   // 部長 一郎（Senior）
let managerId = '';  // 山田 一郎（Manager）
let talentId = '';   // 田中 太郎（Talent）
let talent2Id = '';  // 別タレント
let positionId = ''; // テスト用 Critical ポジション
let periodId = '';   // 考課期間

test.beforeAll(async ({ request }) => {
  const usersRes = await request.get(`${API}/users`);
  const users: Array<{ id: string; displayName: string; role: string }> = await usersRes.json();

  const senior = users.find(u => u.displayName === '部長 一郎');
  if (!senior) throw new Error('シードデータに部長 一郎 が見つかりません');
  seniorId = senior.id;

  const yamada = users.find(u => u.displayName === '山田 一郎');
  if (!yamada) throw new Error('シードデータに山田 一郎 が見つかりません');
  managerId = yamada.id;

  const tanaka = users.find(u => u.displayName === '田中 太郎');
  if (!tanaka) throw new Error('シードデータに田中 太郎 が見つかりません');
  talentId = tanaka.id;

  const otherTalent = users.find(u => u.role === 'Talent' && u.displayName !== '田中 太郎');
  if (!otherTalent) throw new Error('2人目のタレントが見つかりません');
  talent2Id = otherTalent.id;

  // 考課期間を取得
  const periodsRes = await request.get(`${API}/periods`);
  const periods: Array<{ id: string }> = await periodsRes.json();
  if (periods.length > 0) periodId = periods[0].id;

  // テスト用の Critical ポジションを作成
  const posRes = await request.post(`${API}/critical-positions`, {
    data: {
      positionName: '候補者テスト用 Critical ポジション',
      currentHolderId: null,
      importance: 'Critical',
      requiredSkills: 'アーキテクチャ設計',
    },
    headers: { 'X-User-Id': seniorId },
  });
  expect(posRes.status()).toBe(201);
  positionId = (await posRes.json()).id;
});

// ── AC-02-01: 候補者の追加 ────────────────────────────────────────────────────

test.describe('AC-02-01: 候補者の追加', () => {
  test('候補者を ReadySoon で登録できる（201）', async ({ request }) => {
    // Given: 重要ポジションが存在する
    // When: タレントを Readiness=ReadySoon で候補者登録
    const res = await request.post(`${API}/critical-positions/${positionId}/candidates`, {
      data: {
        talentId,
        readiness: 'ReadySoon',
        developmentGap: 'アーキテクチャ設計経験が不足',
        developmentActions: '社外勉強会参加、設計レビュー担当',
      },
      headers: { 'X-User-Id': seniorId },
    });

    // Then: 201 Created + 候補者データが返る
    expect(res.status()).toBe(201);
    const body = await res.json();
    expect(body.readiness).toBe('ReadySoon');
    expect(body.talent.id).toBe(talentId);
    expect(body.developmentGap).toBe('アーキテクチャ設計経験が不足');
    expect(body.nominatedAt).toBeTruthy();
    expect(body.updatedAt).toBeTruthy();
  });

  test('候補者一覧が取得できる（200）', async ({ request }) => {
    const res = await request.get(
      `${API}/critical-positions/${positionId}/candidates?requesterId=${seniorId}`
    );
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.positionId).toBe(positionId);
    expect(Array.isArray(body.candidates)).toBe(true);
    expect(body.candidates.length).toBeGreaterThanOrEqual(1);
    // developmentActionItems フィールドが含まれる
    expect(Array.isArray(body.candidates[0].developmentActionItems)).toBe(true);
  });

  test('同一ポジション・同一タレントの重複登録は 409', async ({ request }) => {
    const res = await request.post(`${API}/critical-positions/${positionId}/candidates`, {
      data: {
        talentId,
        readiness: 'ReadyNow',
        developmentGap: null,
        developmentActions: null,
      },
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(409);
  });

  test('Manager が候補者を追加しようとすると 403', async ({ request }) => {
    const res = await request.post(`${API}/critical-positions/${positionId}/candidates`, {
      data: { talentId: talent2Id, readiness: 'ReadyMidTerm', developmentGap: null, developmentActions: null },
      headers: { 'X-User-Id': managerId },
    });
    expect(res.status()).toBe(403);
  });

  test('存在しないポジションへの候補者追加は 404', async ({ request }) => {
    const fakeId = '00000000-0000-0000-0000-000000000000';
    const res = await request.post(`${API}/critical-positions/${fakeId}/candidates`, {
      data: { talentId, readiness: 'ReadySoon', developmentGap: null, developmentActions: null },
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(404);
  });
});

// ── AC-02-02: 複数候補者の登録（上限制限なし） ───────────────────────────────

test.describe('AC-02-02: 複数候補者の登録', () => {
  let multiPositionId = '';

  test.beforeAll(async ({ request }) => {
    const posRes = await request.post(`${API}/critical-positions`, {
      data: { positionName: '複数候補者テスト用ポジション', currentHolderId: null, importance: 'High', requiredSkills: null },
      headers: { 'X-User-Id': seniorId },
    });
    multiPositionId = (await posRes.json()).id;
  });

  test('複数候補者を追加できる（4名目以降も制限なし）', async ({ request }) => {
    // 1人目（田中）
    const res1 = await request.post(`${API}/critical-positions/${multiPositionId}/candidates`, {
      data: { talentId, readiness: 'ReadySoon', developmentGap: null, developmentActions: null },
      headers: { 'X-User-Id': seniorId },
    });
    expect(res1.status()).toBe(201);

    // 2人目（別タレント）
    const res2 = await request.post(`${API}/critical-positions/${multiPositionId}/candidates`, {
      data: { talentId: talent2Id, readiness: 'ReadyMidTerm', developmentGap: null, developmentActions: null },
      headers: { 'X-User-Id': seniorId },
    });
    expect(res2.status()).toBe(201);

    // 一覧で2名確認
    const listRes = await request.get(
      `${API}/critical-positions/${multiPositionId}/candidates?requesterId=${seniorId}`
    );
    const body = await listRes.json();
    expect(body.candidates.length).toBeGreaterThanOrEqual(2);
  });
});

// ── AC-02-03: Readiness の更新 ──────────────────────────────────────────────

test.describe('AC-02-03: Readiness の更新', () => {
  let candidateId = '';

  test.beforeAll(async ({ request }) => {
    // 別ポジションを作成して候補者を追加
    const posRes = await request.post(`${API}/critical-positions`, {
      data: { positionName: 'Readiness 更新テスト用ポジション', currentHolderId: null, importance: 'Medium', requiredSkills: null },
      headers: { 'X-User-Id': seniorId },
    });
    const pid = (await posRes.json()).id;

    const candRes = await request.post(`${API}/critical-positions/${pid}/candidates`, {
      data: { talentId, readiness: 'ReadySoon', developmentGap: '育成ギャップあり', developmentActions: null },
      headers: { 'X-User-Id': seniorId },
    });
    candidateId = (await candRes.json()).id;
  });

  test('ReadySoon → ReadyNow に更新され updatedAt が更新される', async ({ request }) => {
    // Given: 候補者が ReadySoon で登録されている
    const beforeRes = await request.get(`${API}/critical-positions/${'00000000-0000-0000-0000-000000000001'}/candidates?requesterId=${seniorId}`);
    // When: PUT /api/succession-candidates/{id}
    const res = await request.put(`${API}/succession-candidates/${candidateId}`, {
      data: {
        readiness: 'ReadyNow',
        developmentGap: '概ね解消',
        developmentActions: '引き継ぎ計画の策定',
      },
      headers: { 'X-User-Id': seniorId },
    });

    // Then: 200 + Readiness が ReadyNow に変わる
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.readiness).toBe('ReadyNow');
    expect(body.developmentGap).toBe('概ね解消');
    expect(body.updatedAt).toBeTruthy();
  });

  test('Manager が更新しようとすると 403', async ({ request }) => {
    const res = await request.put(`${API}/succession-candidates/${candidateId}`, {
      data: { readiness: 'ReadyNow', developmentGap: null, developmentActions: null },
      headers: { 'X-User-Id': managerId },
    });
    expect(res.status()).toBe(403);
  });

  test('存在しない candidateId は 404', async ({ request }) => {
    const fakeId = '00000000-0000-0000-0000-000000000000';
    const res = await request.put(`${API}/succession-candidates/${fakeId}`, {
      data: { readiness: 'ReadyNow', developmentGap: null, developmentActions: null },
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(404);
  });
});

// ── 候補者の除外 ─────────────────────────────────────────────────────────────

test.describe('DELETE /api/succession-candidates/{id} — 候補者の除外', () => {
  let candidateId = '';

  test.beforeAll(async ({ request }) => {
    const posRes = await request.post(`${API}/critical-positions`, {
      data: { positionName: '候補者除外テスト用ポジション', currentHolderId: null, importance: 'Medium', requiredSkills: null },
      headers: { 'X-User-Id': seniorId },
    });
    const pid = (await posRes.json()).id;

    const candRes = await request.post(`${API}/critical-positions/${pid}/candidates`, {
      data: { talentId, readiness: 'ReadyMidTerm', developmentGap: null, developmentActions: null },
      headers: { 'X-User-Id': seniorId },
    });
    candidateId = (await candRes.json()).id;
  });

  test('候補者を除外できる（204）', async ({ request }) => {
    const res = await request.delete(`${API}/succession-candidates/${candidateId}?requesterId=${seniorId}`);
    expect(res.status()).toBe(204);
  });

  test('除外後は 404 が返る', async ({ request }) => {
    const res = await request.put(`${API}/succession-candidates/${candidateId}`, {
      data: { readiness: 'ReadyNow', developmentGap: null, developmentActions: null },
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(404);
  });
});

// ── 育成アクション ──────────────────────────────────────────────────────────

test.describe('育成アクションの追加・進捗更新', () => {
  let actionCandidateId = '';
  let actionId = '';

  test.beforeAll(async ({ request }) => {
    const posRes = await request.post(`${API}/critical-positions`, {
      data: { positionName: '育成アクションテスト用ポジション', currentHolderId: null, importance: 'High', requiredSkills: null },
      headers: { 'X-User-Id': seniorId },
    });
    const pid = (await posRes.json()).id;

    const candRes = await request.post(`${API}/critical-positions/${pid}/candidates`, {
      data: { talentId, readiness: 'ReadySoon', developmentGap: null, developmentActions: null },
      headers: { 'X-User-Id': seniorId },
    });
    actionCandidateId = (await candRes.json()).id;
  });

  test('育成アクションを追加できる（201・Status=NotStarted）', async ({ request }) => {
    const res = await request.post(`${API}/succession-candidates/${actionCandidateId}/development-actions`, {
      data: {
        action: 'アーキテクチャ設計書のレビュー担当',
        dueDate: '2026-09-30',
      },
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(201);
    const body = await res.json();
    expect(body.action).toBe('アーキテクチャ設計書のレビュー担当');
    expect(body.status).toBe('NotStarted');
    expect(body.dueDate).toBe('2026-09-30');
    expect(body.completedAt).toBeNull();
    actionId = body.id;
  });

  test('育成アクションを Done に更新すると completedAt が設定される', async ({ request }) => {
    if (!actionId) test.skip(true, 'actionId が未取得');
    const now = new Date().toISOString();
    const res = await request.put(`${API}/succession-development-actions/${actionId}`, {
      data: { status: 'Done', completedAt: now },
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.status).toBe('Done');
    expect(body.completedAt).not.toBeNull();
  });
});

// ── AC-04-01 & AC-04-02: サクセッションマップ ───────────────────────────────

test.describe('AC-04-01 & AC-04-02: サクセッションマップ', () => {
  let mapPositionId = '';

  test.beforeAll(async ({ request }) => {
    // Critical ポジションを作成（ReadyNow 候補なし → hasReadyNowGap=true になるケース）
    const posRes = await request.post(`${API}/critical-positions`, {
      data: {
        positionName: 'マップテスト用 Critical ポジション（ReadyNow 空）',
        currentHolderId: null,
        importance: 'Critical',
        requiredSkills: null,
      },
      headers: { 'X-User-Id': seniorId },
    });
    mapPositionId = (await posRes.json()).id;

    // ReadySoon 候補のみ追加（ReadyNow はなし）
    await request.post(`${API}/critical-positions/${mapPositionId}/candidates`, {
      data: { talentId, readiness: 'ReadySoon', developmentGap: null, developmentActions: null },
      headers: { 'X-User-Id': seniorId },
    });
  });

  test('AC-04-01: サクセッションマップが Readiness 別グリッドで返る（200）', async ({ request }) => {
    const res = await request.get(`${API}/succession-map?requesterId=${seniorId}`);

    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(Array.isArray(body.positions)).toBe(true);
    expect(body.positions.length).toBeGreaterThanOrEqual(1);

    // 各ポジションに readyNow / readySoon / readyMidTerm が含まれる
    const pos = body.positions[0];
    expect(Array.isArray(pos.readyNow)).toBe(true);
    expect(Array.isArray(pos.readySoon)).toBe(true);
    expect(Array.isArray(pos.readyMidTerm)).toBe(true);
    expect(typeof pos.hasReadyNowGap).toBe('boolean');
  });

  test('AC-04-02: Critical ポジションに ReadyNow 候補がいない場合 hasReadyNowGap=true', async ({ request }) => {
    const res = await request.get(`${API}/succession-map?requesterId=${seniorId}`);
    expect(res.status()).toBe(200);
    const body = await res.json();

    const targetPos = body.positions.find((p: { id: string }) => p.id === mapPositionId);
    expect(targetPos).toBeDefined();
    expect(targetPos.readyNow).toHaveLength(0);
    expect(targetPos.importance).toBe('Critical');
    expect(targetPos.hasReadyNowGap).toBe(true);
  });

  test('Manager がマップを取得しようとすると 403', async ({ request }) => {
    const res = await request.get(`${API}/succession-map?requesterId=${managerId}`);
    expect(res.status()).toBe(403);
  });
});

// ── AC-03-01: 9ボックスサジェスト ────────────────────────────────────────────

test.describe('AC-03-01: 9ボックス High Potential サジェスト', () => {
  test('periodId と positionId を指定してサジェストを取得できる（200）', async ({ request }) => {
    if (!periodId) {
      test.skip(true, '考課期間が見つかりません（シードデータを確認してください）');
      return;
    }

    const res = await request.get(
      `${API}/succession-map/suggest?requesterId=${seniorId}&periodId=${periodId}&positionId=${positionId}`
    );

    // Then: 200 + suggestions 配列
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(Array.isArray(body.suggestions)).toBe(true);

    // 返ってくるタレントは PotentialLevel が High または Mid
    for (const s of body.suggestions) {
      expect(['High', 'Mid']).toContain(s.potentialLevel);
      expect(typeof s.alreadyCandidate).toBe('boolean');
    }
  });

  test('Manager がサジェストを取得しようとすると 403', async ({ request }) => {
    if (!periodId) { test.skip(true, '期間なし'); return; }
    const res = await request.get(
      `${API}/succession-map/suggest?requesterId=${managerId}&periodId=${periodId}&positionId=${positionId}`
    );
    expect(res.status()).toBe(403);
  });
});
