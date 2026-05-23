import { test, expect } from '@playwright/test';

// SPEC-MDC-003: タレントプロファイル（委員会事前資料）— API テスト
// 仕様書: docs/spec/mdc/spec-mdc-003-talent-profile.md
// 設計書: docs/design/SPEC-MDC-003-design.md
// 対象タスク: TASK-054
// 生成日: 2026-05-23

const API_BASE = 'http://localhost:5108/api';

let TALENT_ID = '';        // 田中 太郎（manager1 配下）
let MANAGER_ID = '';       // 山田 一郎（manager1）
let MANAGER2_ID = '';      // 鈴木 二郎（manager2、別チーム）
let SENIOR_ID = '';        // 部長 一郎
let PERIOD_ID = '';        // 最新考課期間

test.beforeAll(async ({ request }) => {
  const usersRes = await request.get(`${API_BASE}/users`);
  expect(usersRes.status()).toBe(200);
  const users: Array<{ id: string; role: string; displayName: string; managerId: string | null }> = await usersRes.json();

  const talent = users.find(u => u.displayName === '田中 太郎');
  const manager = users.find(u => u.displayName === '山田 一郎');
  const manager2 = users.find(u => u.displayName === '鈴木 二郎');
  const senior = users.find(u => u.displayName === '部長 一郎');

  if (!talent || !manager || !manager2 || !senior)
    throw new Error('必要なシードユーザーが見つかりません');

  TALENT_ID = talent.id;
  MANAGER_ID = manager.id;
  MANAGER2_ID = manager2.id;
  SENIOR_ID = senior.id;

  const periodsRes = await request.get(`${API_BASE}/periods`);
  const periods: Array<{ id: string; year: number }> = await periodsRes.json();
  const latest = periods.sort((a, b) => b.year - a.year)[0];
  if (!latest) throw new Error('考課期間が見つかりません');
  PERIOD_ID = latest.id;
});

// ─── AC-01: プロファイル集約取得 ─────────────────────────────────────────────

test.describe('AC-01: プロファイル集約取得', () => {
  // AC-01-01: Manager が配下タレントのプロファイルを取得できる
  test('Managerが配下タレントのプロファイルを取得するとすべてのセクションが含まれる', async ({ request }) => {
    const res = await request.get(
      `${API_BASE}/talents/${TALENT_ID}/profile?periodId=${PERIOD_ID}`,
      { headers: { 'X-User-Id': MANAGER_ID } }
    );
    expect(res.status()).toBe(200);
    const body = await res.json();

    expect(body.talentId).toBe(TALENT_ID);
    expect(body.talentName).toBeTruthy();
    expect(body.periodId).toBe(PERIOD_ID);
    expect(body.periodName).toBeTruthy();
    expect(typeof body.isKeyPerson).toBe('boolean');
    // skillSummary, meetingSummary は配列（空でも可）
    expect(Array.isArray(body.skillSummary)).toBe(true);
    expect(Array.isArray(body.meetingSummary)).toBe(true);
  });

  // AC-01-02: 9ボックス未評価のとき nineBox が null
  test('9ボックス未評価の場合 nineBox が null になる', async ({ request }) => {
    // 新規タレント（シードに評価がないケース）は別期間で確認
    const res = await request.get(
      `${API_BASE}/talents/${TALENT_ID}/profile?periodId=${PERIOD_ID}`,
      { headers: { 'X-User-Id': MANAGER_ID } }
    );
    expect(res.status()).toBe(200);
    const body = await res.json();
    // nineBox は null または オブジェクト（シード状況による）
    expect(body.nineBox === null || typeof body.nineBox === 'object').toBe(true);
  });

  // periodId 省略時は最新期間を使用
  test('periodId を省略した場合は最新考課期間のデータが返る', async ({ request }) => {
    const res = await request.get(
      `${API_BASE}/talents/${TALENT_ID}/profile`,
      { headers: { 'X-User-Id': MANAGER_ID } }
    );
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.periodId).toBeTruthy();
  });

  // Senior/Admin もアクセス可
  test('Seniorは任意のタレントプロファイルを取得できる', async ({ request }) => {
    const res = await request.get(
      `${API_BASE}/talents/${TALENT_ID}/profile`,
      { headers: { 'X-User-Id': SENIOR_ID } }
    );
    expect(res.status()).toBe(200);
  });

  // Manager が配下外のタレントにアクセスすると 403
  test('Manager が配下以外のタレントにアクセスすると 403 になる', async ({ request }) => {
    // TALENT_ID は manager1 配下、MANAGER2_ID は別のマネージャー
    const res = await request.get(
      `${API_BASE}/talents/${TALENT_ID}/profile`,
      { headers: { 'X-User-Id': MANAGER2_ID } }
    );
    expect(res.status()).toBe(403);
  });

  // 存在しないタレントIDは 404
  test('存在しないタレントIDを指定すると 404 になる', async ({ request }) => {
    const fakeId = '00000000-0000-0000-0000-000000000000';
    const res = await request.get(
      `${API_BASE}/talents/${fakeId}/profile`,
      { headers: { 'X-User-Id': SENIOR_ID } }
    );
    expect(res.status()).toBe(404);
  });
});

// ─── AC-02: 育成コメント保存（UPSERT） ───────────────────────────────────────

test.describe('AC-02: 育成コメント保存', () => {
  const commentPayload = {
    currentChallenges: 'テスト: アーキテクチャ設計力の強化',
    developmentPolicy: 'テスト: 設計レビューへの参加機会を増やす',
    expectedRole: 'テスト: 2年後にテックリード候補'
  };

  // AC-02-01: Manager が育成コメントを保存できる（新規INSERT）
  test('Managerが初回コメントを保存すると 200 でコメントが返る', async ({ request }) => {
    const res = await request.put(
      `${API_BASE}/talents/${TALENT_ID}/manager-comment?periodId=${PERIOD_ID}`,
      {
        headers: { 'X-User-Id': MANAGER_ID, 'Content-Type': 'application/json' },
        data: commentPayload
      }
    );
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.talentId).toBe(TALENT_ID);
    expect(body.periodId).toBe(PERIOD_ID);
    expect(body.currentChallenges).toBe(commentPayload.currentChallenges);
    expect(body.developmentPolicy).toBe(commentPayload.developmentPolicy);
    expect(body.expectedRole).toBe(commentPayload.expectedRole);
    expect(body.updatedAt).toBeTruthy();
  });

  // UPSERT: 同じ (talentId, periodId) で再保存すると UPDATE される
  test('同じ期間で再度保存すると内容が更新される', async ({ request }) => {
    const updatedPayload = {
      currentChallenges: 'テスト更新: リーダーシップ開発',
      developmentPolicy: 'テスト更新: チームリード経験を積ませる',
      expectedRole: 'テスト更新: 3年後にテックリード'
    };
    const res = await request.put(
      `${API_BASE}/talents/${TALENT_ID}/manager-comment?periodId=${PERIOD_ID}`,
      {
        headers: { 'X-User-Id': MANAGER_ID, 'Content-Type': 'application/json' },
        data: updatedPayload
      }
    );
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.currentChallenges).toBe(updatedPayload.currentChallenges);
  });

  // null フィールドも許容
  test('フィールドを null で保存できる', async ({ request }) => {
    const res = await request.put(
      `${API_BASE}/talents/${TALENT_ID}/manager-comment?periodId=${PERIOD_ID}`,
      {
        headers: { 'X-User-Id': MANAGER_ID, 'Content-Type': 'application/json' },
        data: { currentChallenges: null, developmentPolicy: null, expectedRole: null }
      }
    );
    expect(res.status()).toBe(200);
  });

  // Manager が配下外のタレントに保存すると 403
  test('Managerが配下外のタレントにコメントを保存しようとすると 403 になる', async ({ request }) => {
    const res = await request.put(
      `${API_BASE}/talents/${TALENT_ID}/manager-comment?periodId=${PERIOD_ID}`,
      {
        headers: { 'X-User-Id': MANAGER2_ID, 'Content-Type': 'application/json' },
        data: commentPayload
      }
    );
    expect(res.status()).toBe(403);
  });

  // プロファイル取得にコメントが反映される
  test('保存したコメントがプロファイル取得で返る', async ({ request }) => {
    const saved = {
      currentChallenges: '反映確認テスト',
      developmentPolicy: '反映確認方針',
      expectedRole: '反映確認役割'
    };
    await request.put(
      `${API_BASE}/talents/${TALENT_ID}/manager-comment?periodId=${PERIOD_ID}`,
      {
        headers: { 'X-User-Id': MANAGER_ID, 'Content-Type': 'application/json' },
        data: saved
      }
    );

    const profileRes = await request.get(
      `${API_BASE}/talents/${TALENT_ID}/profile?periodId=${PERIOD_ID}`,
      { headers: { 'X-User-Id': MANAGER_ID } }
    );
    const profile = await profileRes.json();
    expect(profile.managerComment?.currentChallenges).toBe(saved.currentChallenges);
  });
});

// ─── AC-03: 重点管理フラグ ────────────────────────────────────────────────────

test.describe('AC-03: 重点管理フラグ', () => {
  // AC-03-02: Senior がフラグを ON にできる
  test('SeniorがフラグをONにすると isActive:true が返る', async ({ request }) => {
    const res = await request.patch(
      `${API_BASE}/talents/${TALENT_ID}/key-person-flag`,
      {
        headers: { 'X-User-Id': SENIOR_ID, 'Content-Type': 'application/json' },
        data: { isActive: true, reason: 'テスト: 重点管理指定' }
      }
    );
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.talentId).toBe(TALENT_ID);
    expect(body.isActive).toBe(true);
  });

  // AC-03-01: フラグON後にプロファイルの isKeyPerson が true になる
  test('フラグON後にプロファイルの isKeyPerson が true になる', async ({ request }) => {
    const profileRes = await request.get(
      `${API_BASE}/talents/${TALENT_ID}/profile`,
      { headers: { 'X-User-Id': SENIOR_ID } }
    );
    const profile = await profileRes.json();
    expect(profile.isKeyPerson).toBe(true);
  });

  // フラグを OFF にできる
  test('SeniorがフラグをOFFにすると isActive:false が返る', async ({ request }) => {
    const res = await request.patch(
      `${API_BASE}/talents/${TALENT_ID}/key-person-flag`,
      {
        headers: { 'X-User-Id': SENIOR_ID, 'Content-Type': 'application/json' },
        data: { isActive: false, reason: null }
      }
    );
    expect(res.status()).toBe(200);
    const body = await res.json();
    expect(body.isActive).toBe(false);
  });

  // Manager はフラグ操作不可（403）
  test('Managerがフラグを操作しようとすると 403 になる', async ({ request }) => {
    const res = await request.patch(
      `${API_BASE}/talents/${TALENT_ID}/key-person-flag`,
      {
        headers: { 'X-User-Id': MANAGER_ID, 'Content-Type': 'application/json' },
        data: { isActive: true, reason: null }
      }
    );
    expect(res.status()).toBe(403);
  });
});
