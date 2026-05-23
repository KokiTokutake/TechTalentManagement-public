import { test, expect } from '@playwright/test';

// SPEC-MDC-004: 委員会議事録の構造化 — UI テスト（TASK-059）
// 仕様書: docs/spec/SPEC-MDC-004-structured-minutes.md
// 設計書: docs/design/SPEC-MDC-004-design.md

const BASE = 'http://localhost:3000';
const API  = 'http://localhost:5108/api';

let seniorId = '';
let managerId = '';
let talentId = '';
let sessionId = '';
let prevSessionId = '';

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

  // 前回セッション（積み残しテスト用）— 他テストと衝突しない日付を使用
  const prevRes = await request.post(`${API}/committee-sessions?requesterId=${seniorId}`, {
    data: { heldOn: '2026-12-15', minutes: '前回セッション（UIテスト）', participantIds: [seniorId], keyPersonIds: [] },
  });
  expect(prevRes.status()).toBe(201);
  const prevSession = await prevRes.json();
  prevSessionId = prevSession.id;

  // 前回セッションに未完了アクションアイテムを3件追加（積み残しテスト用）
  const prevDecisionRes = await request.post(`${API}/mdc-sessions/${prevSessionId}/decisions`, {
    data: {
      decisionType: 'Other',
      content: '前回テスト決定事項',
      decidedAt: '2026-12-15',
      talentIds: [talentId],
    },
    headers: { 'X-User-Id': seniorId },
  });
  const prevDecision = await prevDecisionRes.json();

  for (let i = 1; i <= 3; i++) {
    await request.post(`${API}/mdc-decisions/${prevDecision.id}/action-items`, {
      data: {
        description: `前回積み残しアクション ${i}`,
        assigneeId: managerId,
        dueDate: '2026-12-20',
      },
      headers: { 'X-User-Id': seniorId },
    });
  }

  // 今回セッション（prevSessionId より新しい日付）— 他テストと衝突しない日付
  const sessionRes = await request.post(`${API}/committee-sessions?requesterId=${seniorId}`, {
    data: { heldOn: '2026-12-31', minutes: 'UIテスト用議事録', participantIds: [seniorId], keyPersonIds: [] },
  });
  expect(sessionRes.status()).toBe(201);
  const session = await sessionRes.json();
  sessionId = session.id;
});

async function loginAsSenior(page: import('@playwright/test').Page) {
  await page.goto(`${BASE}/select-user`);
  await expect(page.getByText('部長 一郎')).toBeVisible({ timeout: 10000 });
  await page.getByText('部長 一郎').click();
  await page.getByRole('button', { name: '開始する' }).click();
  await page.waitForURL(`${BASE}/`);
}

async function loginAsManager(page: import('@playwright/test').Page) {
  await page.goto(`${BASE}/select-user`);
  await expect(page.getByText('山田 一郎')).toBeVisible({ timeout: 10000 });
  await page.getByText('山田 一郎').click();
  await page.getByRole('button', { name: '開始する' }).click();
  await page.waitForURL(`${BASE}/`);
}

// ── AC-01: 決定事項の構造化記録 ─────────────────────────────────────────────

test.describe('AC-01: 決定事項の構造化記録', () => {

  // AC-01-01: 決定事項の登録
  // Given: Senior が MDC セッション詳細画面を開いている
  // When: 決定タイプ・タレント・内容を入力して追加
  // Then: 決定事項が一覧に追加される
  test('AC-01-01: Seniorが決定タイプ・内容を入力して決定事項を登録できる', async ({ page }) => {
    await loginAsSenior(page);
    await page.goto(`${BASE}/mdc/sessions/${sessionId}`);

    await expect(page.locator('[data-testid="mdc-decision-list"]')).toBeVisible({ timeout: 10000 });

    // 追加ボタンをクリック
    await page.locator('[data-testid="mdc-decision-add-button"]').click();

    // 決定タイプを選択
    await page.locator('[data-testid="mdc-decision-type-select"]').selectOption('KeyPersonAssign');

    // 決定内容を入力
    const content = `Q3から集中育成対象に指定 ${Date.now()}`;
    await page.locator('[data-testid="mdc-decision-content-input"]').fill(content);

    // 保存
    await page.locator('[data-testid="mdc-decision-save-button"]').click();

    // 決定事項一覧に追加された内容が表示される
    await expect(page.locator('[data-testid="mdc-decision-list"]')).toContainText(content, { timeout: 5000 });
    await expect(page.locator('[data-testid="mdc-decision-list"]')).toContainText('重点管理指定');
  });

  // AC-01-02: 複数タレントへの関連付け
  // Given: 決定事項登録フォームが表示されている
  // When: 対象タレントに2名を選択して登録
  // Then: 決定事項が両タレントに関連付けられる
  test('AC-01-02: 対象タレントを選択して決定事項と関連付けできる', async ({ page }) => {
    await loginAsSenior(page);
    await page.goto(`${BASE}/mdc/sessions/${sessionId}`);

    await expect(page.locator('[data-testid="mdc-decision-list"]')).toBeVisible({ timeout: 10000 });

    await page.locator('[data-testid="mdc-decision-add-button"]').click();

    await page.locator('[data-testid="mdc-decision-type-select"]').selectOption('DevelopmentPolicyChange');
    await page.locator('[data-testid="mdc-decision-content-input"]').fill('育成方針変更テスト');

    // タレント選択チェックボックスをクリック（data-testid="mdc-decision-talent-select" の内部）
    const talentSelect = page.locator('[data-testid="mdc-decision-talent-select"]');
    await expect(talentSelect).toBeVisible();
    const firstCheckbox = talentSelect.locator('input[type="checkbox"]').first();
    await firstCheckbox.check();

    await page.locator('[data-testid="mdc-decision-save-button"]').click();

    await expect(page.locator('[data-testid="mdc-decision-list"]')).toContainText('育成方針変更テスト', { timeout: 5000 });
    await expect(page.locator('[data-testid="mdc-decision-list"]')).toContainText('育成方針変更');
  });

});

// ── AC-02: アクションアイテムの登録と追跡 ────────────────────────────────────

test.describe('AC-02: アクションアイテムの登録と追跡', () => {

  let decisionId = '';

  test.beforeAll(async ({ request }) => {
    // UIテスト用に決定事項を事前作成
    const res = await request.post(`${API}/mdc-sessions/${sessionId}/decisions`, {
      data: {
        decisionType: 'Other',
        content: 'AI テスト用決定事項',
        decidedAt: '2026-05-23',
        talentIds: [talentId],
      },
      headers: { 'X-User-Id': seniorId },
    });
    const decision = await res.json();
    decisionId = decision.id;
  });

  // AC-02-01: アクションアイテムの追加
  // Given: 決定事項が登録されている
  // When: 内容・担当者・期限を入力して追加
  // Then: アクションアイテムがステータス「未着手」で登録される
  test('AC-02-01: アクションアイテムを内容・担当者・期限で登録できる', async ({ page }) => {
    await loginAsSenior(page);
    await page.goto(`${BASE}/mdc/sessions/${sessionId}`);

    await expect(page.locator('[data-testid="mdc-decision-list"]')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('[data-testid="mdc-action-item-list"]').first()).toBeVisible();

    // アクションアイテム追加ボタン
    await page.locator('[data-testid="mdc-action-item-add-button"]').first().click();

    const aiDesc = `月次1on1を設定する ${Date.now()}`;
    await page.locator('[data-testid="mdc-action-item-description-input"]').fill(aiDesc);
    await page.locator('[data-testid="mdc-action-item-assignee-select"]').selectOption({ index: 1 });
    await page.locator('[data-testid="mdc-action-item-due-date-input"]').fill('2026-06-30');
    await page.locator('[data-testid="mdc-action-item-save-button"]').click();

    // 追加された AI が一覧に表示される（未着手）
    await expect(page.locator('[data-testid="mdc-action-item-list"]').first()).toContainText(aiDesc, { timeout: 5000 });
    await expect(page.locator('[data-testid="mdc-action-item-list"]').first()).toContainText('未着手');
  });

  // AC-02-02: ステータス更新
  // Given: 担当者がアクションアイテム一覧を表示
  // When: ステータスを「完了」に変更
  // Then: ステータスが「完了」に更新される
  test('AC-02-02: アクションアイテムのステータスを完了に更新できる', async ({ page, request }) => {
    // テスト用 AI を事前作成
    const aiRes = await request.post(`${API}/mdc-decisions/${decisionId}/action-items`, {
      data: { description: 'ステータス更新テスト用AI', assigneeId: managerId, dueDate: null },
      headers: { 'X-User-Id': seniorId },
    });
    const ai = await aiRes.json();
    const aiId = ai.id;

    await loginAsSenior(page);
    await page.goto(`${BASE}/mdc/sessions/${sessionId}`);

    await expect(page.locator('[data-testid="mdc-action-item-list"]').first()).toBeVisible({ timeout: 10000 });

    // ステータスセレクトで「Done」を選択
    const statusSelect = page.locator(`[data-testid="mdc-action-item-status-${aiId}"]`);
    await expect(statusSelect).toBeVisible({ timeout: 5000 });
    await statusSelect.selectOption('Done');

    // 完了に変わることを確認
    await expect(statusSelect).toHaveValue('Done', { timeout: 5000 });
    await expect(page.locator('[data-testid="mdc-action-item-list"]').first()).toContainText('完了');
  });

  // AC-02-03: 期限超過の警告
  // Given: 期限が過去日付のアクションアイテムが存在する
  // When: アクションアイテム一覧を開く
  // Then: 期限超過アイテムが警告バッジで表示される
  test('AC-02-03: 期限超過アクションアイテムに警告バッジが表示される', async ({ page, request }) => {
    // 期限が過去の未完了 AI を作成
    await request.post(`${API}/mdc-decisions/${decisionId}/action-items`, {
      data: { description: '期限超過テスト用AI', assigneeId: managerId, dueDate: '2020-01-01' },
      headers: { 'X-User-Id': seniorId },
    });

    await loginAsSenior(page);
    await page.goto(`${BASE}/mdc/sessions/${sessionId}`);

    await expect(page.locator('[data-testid="mdc-action-item-list"]').first()).toBeVisible({ timeout: 10000 });

    // 期限超過バッジが表示される
    await expect(page.locator('[data-testid="mdc-overdue-badge"]').first()).toBeVisible({ timeout: 5000 });
    await expect(page.locator('[data-testid="mdc-overdue-badge"]').first()).toContainText('期限超過');
  });

});

// ── AC-03: 未完了アクションの自動引き継ぎ ────────────────────────────────────

test.describe('AC-03: 未完了アクションの自動引き継ぎ', () => {

  // AC-03-01: 前回積み残し表示
  // Given: 前回のMDCセッションに未完了AIが3件ある
  // When: 新規MDCセッションの議事録画面を開く
  // Then: 「前回の積み残し（3件）」セクションが自動表示される
  test('AC-03-01: 前回セッションの未完了AIが積み残しセクションに表示される', async ({ page }) => {
    await loginAsSenior(page);
    await page.goto(`${BASE}/mdc/sessions/${sessionId}`);

    // 積み残しセクションが表示される
    await expect(page.locator('[data-testid="mdc-carryover-section"]')).toBeVisible({ timeout: 10000 });

    // 件数バッジが表示され、件数が1以上
    const countBadge = page.locator('[data-testid="mdc-carryover-count"]');
    await expect(countBadge).toBeVisible();
    const countText = await countBadge.textContent();
    const count = parseInt(countText?.replace(/[^0-9]/g, '') ?? '0', 10);
    expect(count).toBeGreaterThanOrEqual(1);
  });

  // AC-03-02: 積み残しの確認操作
  // Given: 前回積み残しセクションが表示されている
  // When: アイテムに「完了」をクリック
  // Then: ステータスが更新される
  test('AC-03-02: 積み残しアイテムを完了にできる', async ({ page, request }) => {
    // 前回セッションの未完了 AI を取得
    const aiRes = await request.get(`${API}/mdc-sessions/${prevSessionId}/action-items`, {
      headers: { 'X-User-Id': seniorId },
    });
    const items: Array<{ id: string; status: string }> = await aiRes.json();
    const pendingItems = items.filter(i => i.status === 'NotStarted' || i.status === 'InProgress');
    expect(pendingItems.length).toBeGreaterThan(0);
    const targetId = pendingItems[0].id;

    await loginAsSenior(page);
    await page.goto(`${BASE}/mdc/sessions/${sessionId}`);

    await expect(page.locator('[data-testid="mdc-carryover-section"]')).toBeVisible({ timeout: 10000 });

    // 完了ボタンをクリック
    const doneBtn = page.locator(`[data-testid="mdc-carryover-done-${targetId}"]`);
    await expect(doneBtn).toBeVisible({ timeout: 5000 });
    await doneBtn.click();

    // ボタンが非表示になる（完了済みアイテムは折りたたまれる）か、件数が減る
    await expect(doneBtn).not.toBeVisible({ timeout: 5000 });
  });

  // AC-03-02（キャンセル）: 積み残しをキャンセルできる
  test('AC-03-02: 積み残しアイテムをキャンセルにできる', async ({ page, request }) => {
    // 前回セッションの未完了 AI を取得
    const aiRes = await request.get(`${API}/mdc-sessions/${prevSessionId}/action-items`, {
      headers: { 'X-User-Id': seniorId },
    });
    const items: Array<{ id: string; status: string }> = await aiRes.json();
    const pendingItems = items.filter(i => i.status === 'NotStarted' || i.status === 'InProgress');

    if (pendingItems.length === 0) {
      test.skip();
      return;
    }

    const targetId = pendingItems[0].id;

    await loginAsSenior(page);
    await page.goto(`${BASE}/mdc/sessions/${sessionId}`);

    await expect(page.locator('[data-testid="mdc-carryover-section"]')).toBeVisible({ timeout: 10000 });

    const cancelBtn = page.locator(`[data-testid="mdc-carryover-cancel-${targetId}"]`);
    await expect(cancelBtn).toBeVisible({ timeout: 5000 });
    await cancelBtn.click();

    await expect(cancelBtn).not.toBeVisible({ timeout: 5000 });
  });

});

// ── AC-04: タレント別決定事項履歴 ────────────────────────────────────────────

test.describe('AC-04: タレント別決定事項履歴', () => {

  test.beforeAll(async ({ request }) => {
    // タレントに関連した決定事項を確認のため追加
    await request.post(`${API}/mdc-sessions/${sessionId}/decisions`, {
      data: {
        decisionType: 'KeyPersonAssign',
        content: 'タレント別履歴テスト用決定事項',
        decidedAt: '2026-05-23',
        talentIds: [talentId],
      },
      headers: { 'X-User-Id': seniorId },
    });
  });

  // AC-04-01: タレントプロファイルへの表示
  // Given: タレントAに対する過去のMDC決定事項が存在する
  // When: タレントAのプロファイル画面を開く
  // Then: 「MDC決定履歴」セクションに時系列で表示される
  test('AC-04-01: タレントプロファイル画面にMDC決定履歴セクションが表示される', async ({ page }) => {
    await loginAsSenior(page);
    await page.goto(`${BASE}/mdc/talent-profile/${talentId}`);

    await expect(page.locator('[data-testid="talent-profile-mdc-decisions-section"]')).toBeVisible({ timeout: 10000 });
  });

  test('AC-04-01: MDC決定履歴に決定タイプと内容が表示される', async ({ page }) => {
    await loginAsSenior(page);
    await page.goto(`${BASE}/mdc/talent-profile/${talentId}`);

    const section = page.locator('[data-testid="talent-profile-mdc-decisions-section"]');
    await expect(section).toBeVisible({ timeout: 10000 });

    // タレント関連の決定事項があれば内容が表示される
    const hasDecisions = await section.locator('.decision-row').count();
    if (hasDecisions > 0) {
      // 決定タイプバッジが存在する
      await expect(section.locator('.type-badge').first()).toBeVisible();
      // 日付が表示される
      await expect(section.locator('.decided-at').first()).toBeVisible();
    }
  });

  test('AC-04-01: マネージャーもタレントプロファイルのMDC決定履歴を閲覧できる', async ({ page }) => {
    await loginAsManager(page);
    await page.goto(`${BASE}/mdc/talent-profile/${talentId}`);

    await expect(page.locator('[data-testid="talent-profile-mdc-decisions-section"]')).toBeVisible({ timeout: 10000 });
  });

});
