import { test, expect } from '@playwright/test';

// SPEC-MDC-003: タレントプロファイル — 画面 UI（TASK-055）
// 仕様書: docs/spec/mdc/SPEC-MDC-003-talent-profile.md
// 設計書: docs/design/SPEC-MDC-003-design.md

const BASE = 'http://localhost:3000';
const API  = 'http://localhost:5108/api';

let talentId = '';   // 田中 太郎（タレント）
let managerId = '';  // 山田 一郎（マネージャー）
let seniorId = '';   // 部長 一郎（Senior）

test.beforeAll(async ({ request }) => {
  const usersRes = await request.get(`${API}/users`);
  const users: Array<{ id: string; displayName: string; role: string }> = await usersRes.json();

  const tanaka = users.find(u => u.displayName === '田中 太郎');
  if (!tanaka) throw new Error('シードデータに田中 太郎 が見つかりません');
  talentId = tanaka.id;

  const yamada = users.find(u => u.displayName === '山田 一郎');
  if (!yamada) throw new Error('シードデータに山田 一郎 が見つかりません');
  managerId = yamada.id;

  const bucho = users.find(u => u.displayName === '部長 一郎');
  if (!bucho) throw new Error('シードデータに部長 一郎 が見つかりません');
  seniorId = bucho.id;
});

async function loginAsManager(page: import('@playwright/test').Page) {
  await page.goto(`${BASE}/select-user`);
  await expect(page.getByText('山田 一郎')).toBeVisible({ timeout: 10000 });
  await page.getByText('山田 一郎').click();
  await page.getByRole('button', { name: '開始する' }).click();
  await page.waitForURL(`${BASE}/`);
}

async function loginAsSenior(page: import('@playwright/test').Page) {
  await page.goto(`${BASE}/select-user`);
  await expect(page.getByText('部長 一郎')).toBeVisible({ timeout: 10000 });
  await page.getByText('部長 一郎').click();
  await page.getByRole('button', { name: '開始する' }).click();
  await page.waitForURL(`${BASE}/`);
}

// ── AC-01-01: プロファイル画面の全セクション表示 ────────────────────────────
test('AC-01-01: タレントプロファイル画面を開くと各セクションが表示される', async ({ page }) => {
  await loginAsManager(page);
  await page.goto(`${BASE}/mdc/talent-profile/${talentId}`);

  await expect(page.locator('[data-testid="talent-profile-page"]')).toBeVisible({ timeout: 10000 });
  await expect(page.locator('[data-testid="talent-profile-nine-box-section"]')).toBeVisible();
  await expect(page.locator('[data-testid="talent-profile-wcm-section"]')).toBeVisible();
  await expect(page.locator('[data-testid="talent-profile-skills-section"]')).toBeVisible();
  await expect(page.locator('[data-testid="talent-profile-meetings-section"]')).toBeVisible();
  await expect(page.locator('[data-testid="talent-profile-manager-comment-section"]')).toBeVisible();
});

// ── AC-01-02: 9ボックス未評価時の表示 ─────────────────────────────────────
test('AC-01-02: 9ボックスセクションには評価データまたは「未評価」メッセージが表示される', async ({ page, request }) => {
  // Senior は全タレントにアクセス可能
  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/talent-profile/${talentId}`);

  await expect(page.locator('[data-testid="talent-profile-page"]')).toBeVisible({ timeout: 10000 });

  // 9ボックスセクションが必ず表示される
  const nineBoxSection = page.locator('[data-testid="talent-profile-nine-box-section"]');
  await expect(nineBoxSection).toBeVisible();

  // nineBox が null なら empty、それ以外はバッジ — どちらかが表示される
  const hasEmpty = await page.locator('[data-testid="talent-profile-nine-box-empty"]').isVisible();
  const hasBadge = await page.locator('.cell-badge').isVisible();
  expect(hasEmpty || hasBadge).toBe(true);
});

// ── AC-02-01: 育成コメントの保存 ──────────────────────────────────────────
test('AC-02-01: 育成コメントを入力して保存すると内容が反映される', async ({ page }) => {
  await loginAsManager(page);
  await page.goto(`${BASE}/mdc/talent-profile/${talentId}`);

  await expect(page.locator('[data-testid="talent-profile-manager-comment-section"]')).toBeVisible({ timeout: 10000 });

  const challenges = `E2Eテスト課題 ${Date.now()}`;
  await page.locator('[data-testid="talent-profile-challenges-input"]').fill(challenges);
  await page.locator('[data-testid="talent-profile-policy-input"]').fill('育成方針テスト');
  await page.locator('[data-testid="talent-profile-expected-role-input"]').fill('期待役割テスト');

  await page.locator('[data-testid="talent-profile-comment-save-button"]').click();

  // 保存完了後にページをリロードして内容が永続化されているか確認
  await page.reload();
  await expect(page.locator('[data-testid="talent-profile-challenges-input"]')).toHaveValue(challenges, { timeout: 10000 });
});

// ── AC-03-01: 重点管理バッジの表示 ──────────────────────────────────────────
test('AC-03-01: isKeyPerson=true のタレントには重点管理バッジが表示される', async ({ page, request }) => {
  // まず重点管理フラグをONにする
  await request.patch(`${API}/talents/${talentId}/key-person-flag`, {
    data: { isActive: true, reason: 'UIテスト用' },
    headers: { 'X-User-Id': seniorId },
  });

  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/talent-profile/${talentId}`);

  await expect(page.locator('[data-testid="talent-profile-key-person-badge"]')).toBeVisible({ timeout: 10000 });

  // クリーンアップ
  await request.patch(`${API}/talents/${talentId}/key-person-flag`, {
    data: { isActive: false, reason: null },
    headers: { 'X-User-Id': seniorId },
  });
});

// ── AC-03-02: 重点管理フラグの切り替え ────────────────────────────────────
test('AC-03-02: Senior ユーザーが重点管理指定ボタンをクリックするとフラグが切り替わる', async ({ page, request }) => {
  // 事前にOFFにする
  await request.patch(`${API}/talents/${talentId}/key-person-flag`, {
    data: { isActive: false, reason: null },
    headers: { 'X-User-Id': seniorId },
  });

  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/talent-profile/${talentId}`);

  await expect(page.locator('[data-testid="talent-profile-key-person-toggle-button"]')).toBeVisible({ timeout: 10000 });

  // バッジが表示されていないことを確認
  await expect(page.locator('[data-testid="talent-profile-key-person-badge"]')).not.toBeVisible();

  // 重点管理指定ボタンをクリック
  await page.locator('[data-testid="talent-profile-key-person-toggle-button"]').click();

  // バッジが表示される
  await expect(page.locator('[data-testid="talent-profile-key-person-badge"]')).toBeVisible({ timeout: 5000 });

  // クリーンアップ
  await request.patch(`${API}/talents/${talentId}/key-person-flag`, {
    data: { isActive: false, reason: null },
    headers: { 'X-User-Id': seniorId },
  });
});

// ── AC-04-01: PDF出力ボタンの表示 ────────────────────────────────────────
test('AC-04-01: PDF出力ボタンが表示される', async ({ page }) => {
  await loginAsManager(page);
  await page.goto(`${BASE}/mdc/talent-profile/${talentId}`);

  await expect(page.locator('[data-testid="talent-profile-pdf-export-button"]')).toBeVisible({ timeout: 10000 });
});
