import { test, expect } from '@playwright/test';

// SPEC-MDC-005: 後継者計画 — サクセッションマップUI（TASK-079）
// 仕様書: docs/spec/SPEC-MDC-005-succession-plan.md
// 設計書: docs/design/SPEC-MDC-005-design.md
// 生成日: 2026-05-26（/test-write により自動生成）

const BASE = 'http://localhost:3000';
const API  = 'http://localhost:5108/api';

let seniorId = '';
let talentId = '';
let talent2Id = '';
let talent3Id = '';

test.beforeAll(async ({ request }) => {
  const usersRes = await request.get(`${API}/users`);
  const users: Array<{ id: string; displayName: string; role: string }> = await usersRes.json();

  const senior = users.find(u => u.displayName === '部長 一郎');
  if (!senior) throw new Error('シードデータに部長 一郎 が見つかりません');
  seniorId = senior.id;

  const tanaka = users.find(u => u.displayName === '田中 太郎');
  if (!tanaka) throw new Error('シードデータに田中 太郎 が見つかりません');
  talentId = tanaka.id;

  const sato = users.find(u => u.displayName === '佐藤 花子');
  if (!sato) throw new Error('シードデータに佐藤 花子 が見つかりません');
  talent2Id = sato.id;

  const talents = users.filter(u => u.role === 'Talent');
  const third = talents.find(u => u.id !== talentId && u.id !== talent2Id);
  if (!third) throw new Error('3人目のタレントが見つかりません');
  talent3Id = third.id;
});

async function loginAsSenior(page: import('@playwright/test').Page) {
  await page.goto(`${BASE}/select-user`);
  await expect(page.getByText('部長 一郎')).toBeVisible({ timeout: 10000 });
  await page.getByText('部長 一郎').click();
  await page.getByRole('button', { name: '開始する' }).click();
  await page.waitForURL(`${BASE}/`);
}

async function createPosition(
  request: import('@playwright/test').APIRequestContext,
  name: string,
  importance: string,
): Promise<string> {
  const res = await request.post(`${API}/critical-positions`, {
    headers: { 'X-User-Id': seniorId },
    data: { positionName: name, importance, currentHolderId: null },
  });
  const body = await res.json() as { id: string };
  return body.id;
}

async function addCandidate(
  request: import('@playwright/test').APIRequestContext,
  positionId: string,
  talentUserId: string,
  readiness: string,
): Promise<string> {
  const res = await request.post(`${API}/critical-positions/${positionId}/candidates`, {
    headers: { 'X-User-Id': seniorId },
    data: { talentId: talentUserId, readiness, developmentGap: null },
  });
  const body = await res.json() as { id: string };
  return body.id;
}

async function deactivatePosition(
  request: import('@playwright/test').APIRequestContext,
  positionId: string,
) {
  await request.put(`${API}/critical-positions/${positionId}`, {
    headers: { 'X-User-Id': seniorId },
    data: { positionName: 'cleanup', importance: 'Medium', currentHolderId: null, isActive: false },
  });
}

// ── AC-04-01: マップの表示 ────────────────────────────────────────────────
test('AC-04-01: サクセッションマップ画面を開くとポジション×Readinessのグリッドが表示される', async ({ page, request }) => {
  // Given: 3件の重要ポジションと各候補者を作成
  const ids: string[] = [];
  const pos1 = await createPosition(request, 'マップテスト-BE', 'Critical');
  const pos2 = await createPosition(request, 'マップテスト-FE', 'High');
  const pos3 = await createPosition(request, 'マップテスト-PM', 'Medium');
  ids.push(pos1, pos2, pos3);

  await addCandidate(request, pos1, talentId, 'ReadyNow');
  await addCandidate(request, pos2, talent2Id, 'ReadySoon');
  await addCandidate(request, pos3, talent3Id, 'ReadyMidTerm');

  // When: サクセッションマップ画面を開く
  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/succession`);

  // Then: マップコンテナが表示される
  await expect(page.locator('[data-testid="succession-map"]')).toBeVisible({ timeout: 10000 });

  // ReadyNowセル・ReadySoonセル・ReadyMidTermセルが存在する
  await expect(page.locator(`[data-testid="map-cell-${pos1}-ReadyNow"]`)).toBeVisible();
  await expect(page.locator(`[data-testid="map-cell-${pos2}-ReadySoon"]`)).toBeVisible();
  await expect(page.locator(`[data-testid="map-cell-${pos3}-ReadyMidTerm"]`)).toBeVisible();

  // 候補者名がセル内に表示される
  await expect(page.locator(`[data-testid="map-cell-${pos1}-ReadyNow"]`).getByText('田中 太郎')).toBeVisible();
  await expect(page.locator(`[data-testid="map-cell-${pos2}-ReadySoon"]`).getByText('佐藤 花子')).toBeVisible();

  // ポジション名がヘッダーに表示される
  await expect(page.getByText('マップテスト-BE')).toBeVisible();
  await expect(page.getByText('マップテスト-FE')).toBeVisible();
  await expect(page.getByText('マップテスト-PM')).toBeVisible();

  // Cleanup
  for (const id of ids) await deactivatePosition(request, id);
});

// ── AC-04-02: カバレッジのアラート（hasReadyNowGap） ─────────────────────
test('AC-04-02: Critical ポジションに Ready Now 候補者がいない場合、後継者未定バッジが表示される', async ({ page, request }) => {
  // Given: CriticalポジションにReadyNow候補がいない状態
  const posId = await createPosition(request, 'AC0402テスト-Critical', 'Critical');

  // ReadySoon候補者のみ追加（ReadyNowなし）
  await addCandidate(request, posId, talentId, 'ReadySoon');

  // When: サクセッションマップ画面を開く
  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/succession`);
  await expect(page.locator('[data-testid="succession-map"]')).toBeVisible({ timeout: 10000 });

  // Then: 後継者未定バッジが表示される
  await expect(page.locator(`[data-testid="map-gap-badge-${posId}"]`)).toBeVisible();
  await expect(page.locator(`[data-testid="map-gap-badge-${posId}"]`)).toContainText('後継者未定');

  // ReadyNowセルにはバッジのみで候補者バッジなし
  const readyNowCell = page.locator(`[data-testid="map-cell-${posId}-ReadyNow"]`);
  await expect(readyNowCell).toBeVisible();
  await expect(readyNowCell.locator('.candidate-chip')).toHaveCount(0);

  // Cleanup
  await deactivatePosition(request, posId);
});

// ── AC-04-02 補足: Ready Now 候補がいる Critical ポジションにはバッジなし ────
test('AC-04-02: Critical ポジションに Ready Now 候補者がいる場合、後継者未定バッジが表示されない', async ({ page, request }) => {
  // Given: CriticalポジションにReadyNow候補あり
  const posId = await createPosition(request, 'AC0402OK-Critical', 'Critical');
  await addCandidate(request, posId, talentId, 'ReadyNow');

  // When
  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/succession`);
  await expect(page.locator('[data-testid="succession-map"]')).toBeVisible({ timeout: 10000 });

  // Then: バッジが表示されない
  await expect(page.locator(`[data-testid="map-gap-badge-${posId}"]`)).not.toBeVisible();

  // Cleanup
  await deactivatePosition(request, posId);
});

// ── マップ全体コンテナの表示確認 ─────────────────────────────────────────
test('succession-map: ポジションが存在する場合にマップコンテナが表示される', async ({ page, request }) => {
  // Given
  const posId = await createPosition(request, 'マップ表示確認テスト', 'High');

  // When
  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/succession`);

  // Then
  await expect(page.locator('[data-testid="succession-map"]')).toBeVisible({ timeout: 10000 });

  // Cleanup
  await deactivatePosition(request, posId);
});

// ── ポジション管理リンク ──────────────────────────────────────────────────
test('ポジション管理へのリンクがサクセッションマップページに表示される', async ({ page }) => {
  // Given / When
  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/succession`);

  // Then
  await expect(page.getByRole('link', { name: 'ポジション管理へ' })).toBeVisible({ timeout: 10000 });
});
