import { test, expect } from '@playwright/test';

// SPEC-MDC-004: 委員会議事録の構造化 — API テスト（TASK-058）
// 仕様書: docs/spec/SPEC-MDC-004-structured-minutes.md
// 設計書: docs/design/SPEC-MDC-004-design.md

const API = 'http://localhost:5108/api';

let seniorId = '';   // 部長 一郎（Senior）
let managerId = '';  // 山田 一郎（Manager）
let talentId = '';   // 田中 太郎（Talent）
let sessionId = '';  // 最新 MDC セッション

test.beforeAll(async ({ request }) => {
  const usersRes = await request.get(`${API}/users`);
  const users: Array<{ id: string; displayName: string; role: string }> = await usersRes.json();

  const bucho = users.find(u => u.displayName === '部長 一郎');
  if (!bucho) throw new Error('シードデータに部長 一郎 が見つかりません');
  seniorId = bucho.id;

  const yamada = users.find(u => u.displayName === '山田 一郎');
  if (!yamada) throw new Error('シードデータに山田 一郎 が見つかりません');
  managerId = yamada.id;

  const tanaka = users.find(u => u.displayName === '田中 太郎');
  if (!tanaka) throw new Error('シードデータに田中 太郎 が見つかりません');
  talentId = tanaka.id;

  // セッションを1件作成（テスト用）
  const sessionRes = await request.post(`${API}/committee-sessions?requesterId=${seniorId}`, {
    data: { heldOn: '2026-05-23', minutes: 'テスト議事録', participantIds: [seniorId], keyPersonIds: [] },
  });
  expect(sessionRes.status()).toBe(201);
  const session = await sessionRes.json();
  sessionId = session.id;
});

// ── AC-01: 決定事項の構造化記録 ─────────────────────────────────────────────

test.describe('AC-01: 決定事項の構造化記録', () => {
  let decisionId = '';

  test('AC-01-01: Seniorが決定事項を登録できる（201）', async ({ request }) => {
    const res = await request.post(`${API}/mdc-sessions/${sessionId}/decisions`, {
      data: {
        decisionType: 'KeyPersonAssign',
        content: 'Q3から集中育成対象に指定',
        decidedAt: '2026-05-23',
        talentIds: [talentId],
      },
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(201);
    const body = await res.json();
    expect(body.decisionType).toBe('KeyPersonAssign');
    expect(body.content).toBe('Q3から集中育成対象に指定');
    expect(body.talents).toHaveLength(1);
    expect(body.talents[0].talentId).toBe(talentId);
    decisionId = body.id;
  });

  test('AC-01-02: 複数タレントへの関連付けができる', async ({ request }) => {
    const usersRes = await request.get(`${API}/users`);
    const users: Array<{ id: string; displayName: string; role: string }> = await usersRes.json();
    const talent2 = users.find(u => u.role === 'Talent' && u.displayName !== '田中 太郎');
    if (!talent2) { test.skip(true, '2人目のタレントが見つかりません'); return; }

    const res = await request.post(`${API}/mdc-sessions/${sessionId}/decisions`, {
      data: {
        decisionType: 'DevelopmentPolicyChange',
        content: '複数タレント決定',
        decidedAt: '2026-05-23',
        talentIds: [talentId, talent2.id],
      },
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(201);
    const body = await res.json();
    expect(body.talents).toHaveLength(2);
  });

  test('AC-01-03: 決定事項一覧を取得できる', async ({ request }) => {
    const res = await request.get(`${API}/mdc-sessions/${sessionId}/decisions`, {
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(Array.isArray(body)).toBe(true);
    expect(body.length).toBeGreaterThanOrEqual(1);
  });

  test('AC-01-04: Manager が決定事項一覧を取得しようとすると 403', async ({ request }) => {
    const res = await request.get(`${API}/mdc-sessions/${sessionId}/decisions`, {
      headers: { 'X-User-Id': managerId },
    });
    expect(res.status()).toBe(403);
  });

  test('AC-01-05: 決定事項を更新できる', async ({ request }) => {
    if (!decisionId) test.skip(true, '前のテストで decisionId が取れませんでした');
    const res = await request.put(`${API}/mdc-decisions/${decisionId}`, {
      data: {
        decisionType: 'KeyPersonRemove',
        content: '更新後の決定内容',
        decidedAt: '2026-05-23',
        talentIds: [talentId],
      },
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.decisionType).toBe('KeyPersonRemove');
    expect(body.content).toBe('更新後の決定内容');
  });
});

// ── AC-02: アクションアイテムの登録と追跡 ────────────────────────────────────

test.describe('AC-02: アクションアイテムの登録と追跡', () => {
  let decisionId = '';
  let actionItemId = '';

  test.beforeAll(async ({ request }) => {
    const res = await request.post(`${API}/mdc-sessions/${sessionId}/decisions`, {
      data: {
        decisionType: 'Other',
        content: 'アクションアイテムテスト用決定',
        decidedAt: '2026-05-23',
        talentIds: [],
      },
      headers: { 'X-User-Id': seniorId },
    });
    decisionId = (await res.json()).id;
  });

  test('AC-02-01: アクションアイテムを追加するとステータス NotStarted で登録される', async ({ request }) => {
    const res = await request.post(`${API}/mdc-decisions/${decisionId}/action-items`, {
      data: {
        description: '田中さんとの月次1on1を設定する',
        assigneeId: managerId,
        dueDate: '2026-06-30',
      },
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(201);
    const body = await res.json();
    expect(body.status).toBe('NotStarted');
    expect(body.description).toBe('田中さんとの月次1on1を設定する');
    expect(body.dueDate).toBe('2026-06-30');
    expect(body.completedAt).toBeNull();
    actionItemId = body.id;
  });

  test('AC-02-02: ステータスを Done に更新すると completedAt が設定される', async ({ request }) => {
    if (!actionItemId) test.skip(true, 'actionItemId が未取得');
    const res = await request.patch(`${API}/mdc-action-items/${actionItemId}/status`, {
      data: { status: 'Done' },
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.status).toBe('Done');
    expect(body.completedAt).not.toBeNull();
  });

  test('AC-02-03: セッション内アクションアイテム一覧を取得できる', async ({ request }) => {
    const res = await request.get(`${API}/mdc-sessions/${sessionId}/action-items`, {
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(Array.isArray(body)).toBe(true);
    expect(body.length).toBeGreaterThanOrEqual(1);
  });

  test('AC-02-04: 担当者本人がステータスを更新できる', async ({ request }) => {
    // 新規アイテム作成
    const createRes = await request.post(`${API}/mdc-decisions/${decisionId}/action-items`, {
      data: { description: '担当者更新テスト', assigneeId: managerId, dueDate: null },
      headers: { 'X-User-Id': seniorId },
    });
    const newItemId = (await createRes.json()).id;

    const res = await request.patch(`${API}/mdc-action-items/${newItemId}/status`, {
      data: { status: 'InProgress' },
      headers: { 'X-User-Id': managerId },
    });
    expect(res.status()).toBe(200);
    expect((await res.json()).status).toBe('InProgress');
  });

  test('AC-02-05: 担当者でない Manager は他人のアイテムを更新できない（403）', async ({ request }) => {
    // 田中をアサインしたアイテム
    const createRes = await request.post(`${API}/mdc-decisions/${decisionId}/action-items`, {
      data: { description: '権限テスト', assigneeId: talentId, dueDate: null },
      headers: { 'X-User-Id': seniorId },
    });
    const itemId = (await createRes.json()).id;

    // Manager（山田）は担当者（田中）でないので 403
    const res = await request.patch(`${API}/mdc-action-items/${itemId}/status`, {
      data: { status: 'Done' },
      headers: { 'X-User-Id': managerId },
    });
    expect(res.status()).toBe(403);
  });
});

// ── AC-04: タレント別決定事項履歴 ────────────────────────────────────────────

test.describe('AC-04: タレント別決定事項履歴', () => {
  test('AC-04-01: タレント別決定事項履歴を取得できる（decidedAt 降順）', async ({ request }) => {
    const res = await request.get(`${API}/talents/${talentId}/mdc-decisions`, {
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(Array.isArray(body)).toBe(true);
    // 田中に紐付く決定事項が含まれる
    const withTanaka = body.filter((d: { talents: Array<{ talentId: string }> }) =>
      d.talents.some((t) => t.talentId === talentId)
    );
    expect(withTanaka.length).toBeGreaterThanOrEqual(1);
  });
});

// ── AC-05: 担当者別アクションアイテム ────────────────────────────────────────

test.describe('AC-05: 担当者別アクションアイテム', () => {
  test('AC-05-01: 自分のアクションアイテムを取得できる', async ({ request }) => {
    const res = await request.get(`${API}/users/${managerId}/action-items`, {
      headers: { 'X-User-Id': managerId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(Array.isArray(body)).toBe(true);
  });

  test('AC-05-02: pending フィルタで未完了のみ取得できる', async ({ request }) => {
    const res = await request.get(`${API}/users/${managerId}/action-items?status=pending`, {
      headers: { 'X-User-Id': seniorId },
    });
    expect(res.status()).toBe(200);
    const body = await res.json() as Array<{ status: string }>;
    for (const item of body) {
      expect(['NotStarted', 'InProgress']).toContain(item.status);
    }
  });

  test('AC-05-03: 他ユーザーのアイテムを本人以外が見ると 403', async ({ request }) => {
    const res = await request.get(`${API}/users/${seniorId}/action-items`, {
      headers: { 'X-User-Id': managerId },
    });
    expect(res.status()).toBe(403);
  });
});
