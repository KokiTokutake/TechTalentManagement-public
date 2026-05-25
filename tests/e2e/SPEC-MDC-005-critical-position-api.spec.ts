import { test, expect } from '@playwright/test';

// SPEC-MDC-005: 後継者計画 — 重要ポジション CRUD API テスト（TASK-076）
// 仕様書: docs/spec/SPEC-MDC-005-succession-plan.md
// 設計書: docs/design/SPEC-MDC-005-design.md
// 生成日: 2026-05-25（/test-write により自動生成）

const API = 'http://localhost:5108/api';

let seniorId = '';  // 部長 一郎（Senior）
let talentId = '';  // 田中 太郎（Talent）
let managerId = ''; // 山田 一郎（Manager）

test.beforeAll(async ({ request }) => {
  const usersRes = await request.get(`${API}/users`);
  const users: Array<{ id: string; displayName: string; role: string }> = await usersRes.json();

  const senior = users.find(u => u.displayName === '部長 一郎');
  if (!senior) throw new Error('シードデータに部長 一郎 が見つかりません');
  seniorId = senior.id;

  const tanaka = users.find(u => u.displayName === '田中 太郎');
  if (!tanaka) throw new Error('シードデータに田中 太郎 が見つかりません');
  talentId = tanaka.id;

  const yamada = users.find(u => u.displayName === '山田 一郎');
  if (!yamada) throw new Error('シードデータに山田 一郎 が見つかりません');
  managerId = yamada.id;
});

// ── AC-01-01: ポジションの登録 ────────────────────────────────────────────────

test.describe('AC-01-01: ポジションの登録', () => {
  test('Seniorがポジションを登録できる（201）', async ({ request }) => {
    // Given: Senior ユーザー
    // When: ポジション名・現担当者・重要度を入力して POST
    const res = await request.post(`${API}/critical-positions`, {
      data: {
        positionName: 'バックエンドテックリード',
        currentHolderId: talentId,
        importance: 'Critical',
        requiredSkills: 'アーキテクチャ設計、チームリード経験',
      },
      headers: { 'X-User-Id': seniorId },
    });

    // Then: 201 Created が返り、ポジションが登録される
    expect(res.status()).toBe(201);
    const body = await res.json();
    expect(body.positionName).toBe('バックエンドテックリード');
    expect(body.importance).toBe('Critical');
    expect(body.id).toBeTruthy();
    expect(body.createdAt).toBeTruthy();
  });

  test('Manager がポジションを登録しようとすると 403', async ({ request }) => {
    const res = await request.post(`${API}/critical-positions`, {
      data: {
        positionName: '権限テスト用ポジション',
        currentHolderId: null,
        importance: 'High',
        requiredSkills: null,
      },
      headers: { 'X-User-Id': managerId },
    });
    expect(res.status()).toBe(403);
  });
});

// ── AC-01-02: ポジションの一覧表示（Importance 昇順ソート） ──────────────────

test.describe('AC-01-02: ポジションの一覧表示', () => {
  test.beforeAll(async ({ request }) => {
    // テスト用に複数ポジションを登録
    const positions = [
      { positionName: 'テスト用 Medium ポジション', importance: 'Medium', currentHolderId: null, requiredSkills: null },
      { positionName: 'テスト用 High ポジション', importance: 'High', currentHolderId: null, requiredSkills: null },
      { positionName: 'テスト用 Critical ポジション', importance: 'Critical', currentHolderId: null, requiredSkills: null },
    ];
    for (const p of positions) {
      await request.post(`${API}/critical-positions`, {
        data: p,
        headers: { 'X-User-Id': seniorId },
      });
    }
  });

  test('一覧が Importance 昇順（Critical→High→Medium）でソートされる（200）', async ({ request }) => {
    // Given: 複数ポジションが登録されている
    // When: GET /api/critical-positions を実行
    const res = await request.get(`${API}/critical-positions?requesterId=${seniorId}`);

    // Then: Importance 昇順でソートされる（Critical=1 < High=2 < Medium=3）
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(Array.isArray(body.positions)).toBe(true);
    expect(body.positions.length).toBeGreaterThanOrEqual(3);

    const importanceOrder: Record<string, number> = { Critical: 1, High: 2, Medium: 3 };
    for (let i = 0; i < body.positions.length - 1; i++) {
      const current = importanceOrder[body.positions[i].importance];
      const next = importanceOrder[body.positions[i + 1].importance];
      expect(current).toBeLessThanOrEqual(next);
    }
  });

  test('candidateCount と hasReadyNowCandidate フィールドが含まれる', async ({ request }) => {
    const res = await request.get(`${API}/critical-positions?requesterId=${seniorId}`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    const position = body.positions[0];
    expect(typeof position.candidateCount).toBe('number');
    expect(typeof position.hasReadyNowCandidate).toBe('boolean');
  });

  test('Manager が一覧取得しようとすると 403', async ({ request }) => {
    const res = await request.get(`${API}/critical-positions?requesterId=${managerId}`);
    expect(res.status()).toBe(403);
  });

  test('includeInactive=false（デフォルト）では isActive=true のみ返る', async ({ request }) => {
    const res = await request.get(`${API}/critical-positions?requesterId=${seniorId}`);
    expect(res.status()).toBe(200);
    const body = await res.json();
    for (const p of body.positions) {
      expect(p.isActive).toBe(true);
    }
  });
});

// ── ポジションの更新 ──────────────────────────────────────────────────────────

test.describe('PUT /api/critical-positions/{id} — ポジション更新', () => {
  let positionId = '';

  test.beforeAll(async ({ request }) => {
    const res = await request.post(`${API}/critical-positions`, {
      data: {
        positionName: '更新テスト用ポジション',
        currentHolderId: null,
        importance: 'High',
        requiredSkills: null,
      },
      headers: { 'X-User-Id': seniorId },
    });
    positionId = (await res.json()).id;
  });

  test('ポジション情報を更新できる（200）', async ({ request }) => {
    const res = await request.put(`${API}/critical-positions/${positionId}`, {
      data: {
        positionName: '更新後のポジション名',
        currentHolderId: talentId,
        importance: 'Critical',
        requiredSkills: '更新後の要求スキル',
        isActive: true,
      },
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.positionName).toBe('更新後のポジション名');
    expect(body.importance).toBe('Critical');
  });

  test('isActive=false に更新するとアーカイブされる', async ({ request }) => {
    const res = await request.put(`${API}/critical-positions/${positionId}`, {
      data: {
        positionName: '更新後のポジション名',
        currentHolderId: null,
        importance: 'Critical',
        requiredSkills: null,
        isActive: false,
      },
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.isActive).toBe(false);
  });

  test('存在しない ID を更新しようとすると 404', async ({ request }) => {
    const fakeId = '00000000-0000-0000-0000-000000000000';
    const res = await request.put(`${API}/critical-positions/${fakeId}`, {
      data: {
        positionName: 'テスト',
        currentHolderId: null,
        importance: 'Medium',
        requiredSkills: null,
        isActive: true,
      },
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(404);
  });
});

// ── タレント別候補ポジション一覧 ──────────────────────────────────────────────

test.describe('GET /api/talents/{talentId}/succession-candidacies', () => {
  test('Senior がタレントの候補ポジション一覧を取得できる（200）', async ({ request }) => {
    // Given: Senior ユーザーが requesterId
    // When: GET /api/talents/{talentId}/succession-candidacies
    const res = await request.get(
      `${API}/talents/${talentId}/succession-candidacies?requesterId=${seniorId}`
    );

    // Then: 200 + candidacies 配列
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(Array.isArray(body.candidacies)).toBe(true);
  });

  test('Manager がタレントの候補ポジション一覧を取得しようとすると 403', async ({ request }) => {
    const res = await request.get(
      `${API}/talents/${talentId}/succession-candidacies?requesterId=${managerId}`
    );
    expect(res.status()).toBe(403);
  });
});
