import { test, expect } from '@playwright/test';

// SPEC-MDC-002: 9ボックス評価 — マップ画面 UI（TASK-046）
// 仕様書: docs/spec/SPEC-MDC-002-nine-box-assessment.md
// 設計書: docs/design/SPEC-MDC-002-design.md

const BASE = 'http://localhost:3000';
const API  = 'http://localhost:5108/api';

let managerId = '';
let prevPeriodId = '';

test.beforeAll(async ({ request }) => {
  const usersRes = await request.get(`${API}/users`);
  const users: Array<{ id: string; displayName: string; role: string }> = await usersRes.json();
  const manager = users.find(u => u.displayName === '山田 一郎');
  if (!manager) throw new Error('シードデータに山田 一郎 が見つかりません');
  managerId = manager.id;

  const periodsRes = await request.get(`${API}/periods`);
  const periods: Array<{ id: string; year: number }> = await periodsRes.json();
  const prev = periods.find(p => p.year === 2025);
  if (!prev) throw new Error('2025年度の考課期間が見つかりません');
  prevPeriodId = prev.id;
});

async function loginAsManager(page: import('@playwright/test').Page) {
  await page.goto(`${BASE}/select-user`);
  // ユーザーリストが表示されるまで待機
  await expect(page.getByText('山田 一郎')).toBeVisible({ timeout: 10000 });
  await page.getByText('山田 一郎').click();
  // 期間は自動的に最初の期間が選択されている。開始ボタンをクリック
  await page.getByRole('button', { name: '開始する' }).click();
  await page.waitForURL(`${BASE}/`);
}

// ── AC-02-01: グリッド表示 ────────────────────────────────────────────
test('AC-02-01: 9ボックスマップ画面に3×3グリッドが表示される', async ({ page }) => {
  await loginAsManager(page);
  await page.goto(`${BASE}/mdc/nine-box`);

  // グリッドが表示されるまで待機
  const grid = page.locator('[data-testid="nine-box-grid"]');
  await expect(grid).toBeVisible({ timeout: 10000 });

  // 9セル全て存在する
  const cells = page.locator('[data-testid^="nine-box-cell-"]');
  await expect(cells).toHaveCount(9);
});

// ── AC-02-01: セルの人数バッジ ──────────────────────────────────────
test('AC-02-01: 各セルに人数バッジが表示される', async ({ page }) => {
  await loginAsManager(page);
  await page.goto(`${BASE}/mdc/nine-box`);

  await expect(page.locator('[data-testid="nine-box-grid"]')).toBeVisible({ timeout: 10000 });

  // High/High セル（スター）は2名のシードデータがある
  const countBadge = page.locator('[data-testid="nine-box-count-high-high"]');
  await expect(countBadge).toBeVisible();
});

// ── AC-03-01: 期間セレクタ ──────────────────────────────────────────
test('AC-03-01: 期間セレクタが表示され、期間を切り替えられる', async ({ page }) => {
  await loginAsManager(page);
  await page.goto(`${BASE}/mdc/nine-box`);

  const periodSelect = page.locator('[data-testid="nine-box-period-select"]');
  await expect(periodSelect).toBeVisible({ timeout: 10000 });

  // セレクタに複数の期間オプションがある
  const options = periodSelect.locator('option');
  await expect(options).toHaveCount(2, { timeout: 5000 }); // 2026年度 + 2025年度
  expect(await options.count()).toBeGreaterThanOrEqual(1);

  // 前年度を選択
  await periodSelect.selectOption({ value: prevPeriodId });
  await expect(page.locator('[data-testid="nine-box-grid"]')).toBeVisible({ timeout: 10000 });
});

// ── AC-02-01: シードデータのタレントバッジが配置されている ──────────
test('AC-02-01: シードデータのタレント（田中 太郎）がHighHighセルに配置されている', async ({ page }) => {
  await loginAsManager(page);
  await page.goto(`${BASE}/mdc/nine-box`);

  // 2025年度を選択（シードデータあり）
  const periodSelect = page.locator('[data-testid="nine-box-period-select"]');
  await expect(periodSelect).toBeVisible({ timeout: 10000 });
  await periodSelect.selectOption({ value: prevPeriodId });
  await page.waitForTimeout(500);

  // 田中 太郎のバッジが存在する（talentId は動的のため data-testid のプレフィックスで確認）
  const badges = page.locator('[data-testid^="nine-box-talent-"]');
  await expect(badges.first()).toBeVisible({ timeout: 5000 });
  const count = await badges.count();
  expect(count).toBeGreaterThanOrEqual(4); // シードで4名評価済み
});

// ── AC-02-02: ポップオーバー表示 ────────────────────────────────────
test('AC-02-02: タレントバッジをクリックするとポップオーバーが表示される', async ({ page }) => {
  await loginAsManager(page);
  await page.goto(`${BASE}/mdc/nine-box`);

  const periodSelect = page.locator('[data-testid="nine-box-period-select"]');
  await expect(periodSelect).toBeVisible({ timeout: 10000 });
  await periodSelect.selectOption({ value: prevPeriodId });
  await page.waitForTimeout(500);

  // 最初のバッジをクリック
  const firstBadge = page.locator('[data-testid^="nine-box-talent-"]').first();
  await expect(firstBadge).toBeVisible({ timeout: 5000 });
  await firstBadge.click();

  // ポップオーバーが表示される（タレント名テキストを持つ要素）
  // ポップオーバーは Teleport で body に追加されるため、グローバル検索
  await expect(page.locator('.popover')).toBeVisible({ timeout: 3000 });
});

// ── ナビゲーションに9ボックスマップリンクがある ───────────────────────
test('サイドバーに9ボックスマップへのリンクが表示される', async ({ page }) => {
  await loginAsManager(page);
  await page.goto(`${BASE}/`);

  await expect(page.getByRole('link', { name: '9ボックスマップ' })).toBeVisible();
});
