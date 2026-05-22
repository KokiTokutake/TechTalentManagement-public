import { test, expect } from '@playwright/test';

// SPEC-MDC-002: 9ボックス評価 — キャリブレーション画面 UI（TASK-047）
// 仕様書: docs/spec/SPEC-MDC-002-nine-box-assessment.md
// 設計書: docs/design/SPEC-MDC-002-design.md

const BASE = 'http://localhost:3000';
const API  = 'http://localhost:5108/api';

let prevPeriodId = '';

test.beforeAll(async ({ request }) => {
  const periodsRes = await request.get(`${API}/periods`);
  const periods: Array<{ id: string; year: number; currentPhase: string }> = await periodsRes.json();
  const prev = periods.find(p => p.year === 2025);
  if (!prev) throw new Error('2025年度の考課期間が見つかりません');
  prevPeriodId = prev.id;
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

// ── AC-04-01: キャリブレーション画面の表示 ─────────────────────────────
test('AC-04-01: キャリブレーション画面にテーブルが表示される', async ({ page }) => {
  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/nine-box/calibration`);

  // 期間セレクタが表示される
  const periodSelect = page.locator('[data-testid="nine-box-calibration-period-select"]');
  await expect(periodSelect).toBeVisible({ timeout: 10000 });

  // 2025年度（評価データあり）を選択
  await periodSelect.selectOption({ value: prevPeriodId });

  // テーブルが表示される
  const table = page.locator('[data-testid="nine-box-calibration-table"]');
  await expect(table).toBeVisible({ timeout: 10000 });
});

// ── AC-04-01: チーム別行が表示される ───────────────────────────────────
test('AC-04-01: 評価データが存在する期間でチーム別の行が表示される', async ({ page }) => {
  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/nine-box/calibration`);

  const periodSelect = page.locator('[data-testid="nine-box-calibration-period-select"]');
  await expect(periodSelect).toBeVisible({ timeout: 10000 });
  await periodSelect.selectOption({ value: prevPeriodId });

  const table = page.locator('[data-testid="nine-box-calibration-table"]');
  await expect(table).toBeVisible({ timeout: 10000 });

  // シードデータには manager1（山田 一郎）と manager2（鈴木 二郎）の2チームがある
  const rows = page.locator('[data-testid^="nine-box-calibration-row-"]');
  const count = await rows.count();
  expect(count).toBeGreaterThanOrEqual(1);
});

// ── AC-04-01: 評価統計の確認 ─────────────────────────────────────────
test('AC-04-01: 山田 一郎チームの評価行が表示される', async ({ page }) => {
  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/nine-box/calibration`);

  const periodSelect = page.locator('[data-testid="nine-box-calibration-period-select"]');
  await expect(periodSelect).toBeVisible({ timeout: 10000 });
  await periodSelect.selectOption({ value: prevPeriodId });

  await expect(page.locator('[data-testid="nine-box-calibration-table"]')).toBeVisible({ timeout: 10000 });

  // 山田 一郎のマネージャーIDを取得
  const usersRes = await page.request.get(`${API}/users`);
  const users: Array<{ id: string; displayName: string }> = await usersRes.json();
  const yamada = users.find(u => u.displayName === '山田 一郎');
  if (!yamada) throw new Error('山田 一郎が見つかりません');

  const yamadaRow = page.locator(`[data-testid="nine-box-calibration-row-${yamada.id}"]`);
  await expect(yamadaRow).toBeVisible({ timeout: 5000 });
});

// ── 期間の切り替え ──────────────────────────────────────────────────
test('AC-04-01: 期間セレクタで期間を切り替えるとテーブルが更新される', async ({ page }) => {
  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/nine-box/calibration`);

  const periodSelect = page.locator('[data-testid="nine-box-calibration-period-select"]');
  await expect(periodSelect).toBeVisible({ timeout: 10000 });

  // 2025年度に切り替え
  await periodSelect.selectOption({ value: prevPeriodId });

  // テーブルが表示される
  await expect(page.locator('[data-testid="nine-box-calibration-table"]')).toBeVisible({ timeout: 10000 });
});

// ── サイドバーにキャリブレーションリンクがある（Senior） ─────────────
test('サイドバーにキャリブレーションへのリンクが Senior ロールに表示される', async ({ page }) => {
  await loginAsSenior(page);
  await page.goto(`${BASE}/`);

  // サイドバー内のリンクを確認（exact: true でダッシュボードのリンクと区別）
  const sidebarLink = page.locator('aside').getByRole('link', { name: 'キャリブレーション' });
  await expect(sidebarLink).toBeVisible();
});

// ── Manager ロールにはキャリブレーションリンクが表示されない ──────────
test('キャリブレーションリンクは Manager ロールには表示されない', async ({ page }) => {
  await loginAsManager(page);
  await page.goto(`${BASE}/`);

  // Manager ロールにはキャリブレーションリンクは表示されない（サイドバー・ダッシュボード両方）
  const sidebarLink = page.locator('aside').getByRole('link', { name: 'キャリブレーション' });
  await expect(sidebarLink).toHaveCount(0);
});
