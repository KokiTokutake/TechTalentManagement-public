import { test, expect } from '@playwright/test';

// SPEC-MDC-005: 後継者計画 — 重要ポジション管理UI（TASK-078）
// 仕様書: docs/spec/SPEC-MDC-005-succession-plan.md
// 設計書: docs/design/SPEC-MDC-005-design.md
// 生成日: 2026-05-26（/test-write により自動生成）

const BASE = 'http://localhost:3000';
const API  = 'http://localhost:5108/api';

let seniorId = '';   // 部長 一郎（Senior）
let talentId = '';   // 田中 太郎（Talent）
let talent2Id = '';  // 佐藤 花子（Talent）

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
});

async function loginAsSenior(page: import('@playwright/test').Page) {
  await page.goto(`${BASE}/select-user`);
  await expect(page.getByText('部長 一郎')).toBeVisible({ timeout: 10000 });
  await page.getByText('部長 一郎').click();
  await page.getByRole('button', { name: '開始する' }).click();
  await page.waitForURL(`${BASE}/`);
}

// テスト後に作成したポジションをクリーンアップするヘルパー
async function deletePosition(request: import('@playwright/test').APIRequestContext, positionId: string) {
  await request.put(`${API}/critical-positions/${positionId}`, {
    headers: { 'X-User-Id': seniorId },
    data: { positionName: 'cleanup', importance: 'Medium', currentHolderId: null, isActive: false },
  });
}

// ── AC-01-01: ポジションの登録 ───────────────────────────────────────────
test('AC-01-01: ポジション名・重要度を入力して保存するとポジションが一覧に追加される', async ({ page, request }) => {
  // Given
  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/succession/positions`);
  await expect(page.locator('[data-testid="position-save-button"]')).toBeVisible({ timeout: 10000 });

  // When
  await page.fill('[data-testid="position-name-input"]', 'UIテスト用ポジション-AC0101');
  await page.selectOption('[data-testid="position-importance-select"]', 'Critical');
  await page.click('[data-testid="position-save-button"]');

  // Then: ポジションが一覧に追加される
  await expect(page.getByText('UIテスト用ポジション-AC0101')).toBeVisible({ timeout: 8000 });

  // Cleanup
  const positionsRes = await request.get(`${API}/critical-positions?requesterId=${seniorId}`);
  const body = await positionsRes.json() as { positions: Array<{ id: string; positionName: string }> };
  const created = body.positions.find(p => p.positionName === 'UIテスト用ポジション-AC0101');
  if (created) await deletePosition(request, created.id);
});

// ── AC-01-02: ポジション一覧が重要度順に表示される ─────────────────────────
test('AC-01-02: ポジション一覧が重要度順（Critical → High → Medium）でソートされて表示される', async ({ page, request }) => {
  // Given: API で3件のポジションを作成（順序をバラバラに）
  const ids: string[] = [];
  for (const [name, importance] of [
    ['Medium-順序テスト', 'Medium'],
    ['Critical-順序テスト', 'Critical'],
    ['High-順序テスト', 'High'],
  ]) {
    const res = await request.post(`${API}/critical-positions`, {
      headers: { 'X-User-Id': seniorId },
      data: { positionName: name, importance, currentHolderId: null },
    });
    const body = await res.json() as { id: string };
    ids.push(body.id);
  }

  // When
  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/succession/positions`);
  await expect(page.getByText('Critical-順序テスト')).toBeVisible({ timeout: 10000 });

  // Then: Critical が High より上に表示される（行インデックスで確認）
  const rows = page.locator('.data-table tbody tr');
  const texts = await rows.allInnerTexts();
  const criticalIdx = texts.findIndex(t => t.includes('Critical-順序テスト'));
  const highIdx = texts.findIndex(t => t.includes('High-順序テスト'));
  const mediumIdx = texts.findIndex(t => t.includes('Medium-順序テスト'));

  expect(criticalIdx).toBeLessThan(highIdx);
  expect(highIdx).toBeLessThan(mediumIdx);

  // Cleanup
  for (const id of ids) await deletePosition(request, id);
});

// ── ポジション追加ボタン（FAB）─────────────────────────────────────────────
test('position-add-button: FABボタンがポジション管理ページに表示される', async ({ page }) => {
  // Given
  await loginAsSenior(page);

  // When
  await page.goto(`${BASE}/mdc/succession/positions`);

  // Then
  await expect(page.locator('[data-testid="position-add-button"]')).toBeVisible({ timeout: 10000 });
});

// ── AC-02-01: 候補者の追加 ───────────────────────────────────────────────
test('AC-02-01: 候補者追加ダイアログでタレントを選択しReadySoonで登録するとReady Soonグループに表示される', async ({ page, request }) => {
  // Given: テスト用ポジションを作成
  const posRes = await request.post(`${API}/critical-positions`, {
    headers: { 'X-User-Id': seniorId },
    data: { positionName: 'AC0201テスト用ポジション', importance: 'High', currentHolderId: null },
  });
  const { id: positionId } = await posRes.json() as { id: string };

  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/succession/positions/${positionId}`);
  await expect(page.locator('[data-testid="candidate-add-button"]')).toBeVisible({ timeout: 10000 });

  // When: 候補者追加ダイアログを開く
  await page.click('[data-testid="candidate-add-button"]');
  await expect(page.locator('[data-testid="candidate-talent-select"]')).toBeVisible();

  // タレントを選択
  await page.selectOption('[data-testid="candidate-talent-select"]', { label: '田中 太郎' });
  await page.selectOption('[data-testid="candidate-readiness-select"]', 'ReadySoon');
  await page.click('[data-testid="candidate-confirm-button"]');

  // Then: Ready Soonグループに田中 太郎が表示される
  await expect(page.getByText('田中 太郎')).toBeVisible({ timeout: 8000 });

  // Cleanup
  await deletePosition(request, positionId);
});

// ── AC-02-02: 上限なし（複数候補者） ─────────────────────────────────────
test('AC-02-02: 既存候補者がいるポジションにさらに候補者を追加できる（上限制限なし）', async ({ page, request }) => {
  // Given: ポジション作成 + 1名目の候補者をAPIで追加
  const posRes = await request.post(`${API}/critical-positions`, {
    headers: { 'X-User-Id': seniorId },
    data: { positionName: 'AC0202テスト用ポジション', importance: 'High', currentHolderId: null },
  });
  const { id: positionId } = await posRes.json() as { id: string };

  await request.post(`${API}/critical-positions/${positionId}/candidates`, {
    headers: { 'X-User-Id': seniorId },
    data: { talentId, readiness: 'ReadySoon', developmentGap: null },
  });

  // When: 詳細画面を開いて2名目を追加
  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/succession/positions/${positionId}`);
  await expect(page.getByText('田中 太郎')).toBeVisible({ timeout: 10000 });

  await page.click('[data-testid="candidate-add-button"]');
  await expect(page.locator('[data-testid="candidate-talent-select"]')).toBeVisible();
  await page.selectOption('[data-testid="candidate-talent-select"]', { label: '佐藤 花子' });
  await page.selectOption('[data-testid="candidate-readiness-select"]', 'ReadyNow');
  await page.click('[data-testid="candidate-confirm-button"]');

  // Then: 2名とも表示される
  await expect(page.getByText('田中 太郎')).toBeVisible({ timeout: 8000 });
  await expect(page.getByText('佐藤 花子')).toBeVisible({ timeout: 8000 });

  // Cleanup
  await deletePosition(request, positionId);
});

// ── AC-02-03: Readiness更新 ─────────────────────────────────────────────
test('AC-02-03: Ready SoonからReady Nowに保存するとReadinessが更新される', async ({ page, request }) => {
  // Given: ポジション + 候補者をAPIで作成
  const posRes = await request.post(`${API}/critical-positions`, {
    headers: { 'X-User-Id': seniorId },
    data: { positionName: 'AC0203テスト用ポジション', importance: 'Critical', currentHolderId: null },
  });
  const { id: positionId } = await posRes.json() as { id: string };

  await request.post(`${API}/critical-positions/${positionId}/candidates`, {
    headers: { 'X-User-Id': seniorId },
    data: { talentId, readiness: 'ReadySoon', developmentGap: null },
  });

  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/succession/positions/${positionId}`);
  await expect(page.getByText('田中 太郎')).toBeVisible({ timeout: 10000 });

  // When: ReadinessセレクトをReady Nowに変更して保存
  const readinessSelect = page.locator('.readiness-select').first();
  await readinessSelect.selectOption('ReadyNow');
  await page.click('[data-testid="candidate-readiness-save-button"]');

  // Then: 保存ボタンが無効化される（保存完了）
  await expect(page.locator('[data-testid="candidate-readiness-save-button"]')).toBeDisabled({ timeout: 8000 });

  // Cleanup
  await deletePosition(request, positionId);
});

// ── 候補者追加ボタンの表示 ────────────────────────────────────────────────
test('candidate-add-button: ポジション詳細画面に候補者追加ボタンが表示される', async ({ page, request }) => {
  // Given: テスト用ポジション作成
  const posRes = await request.post(`${API}/critical-positions`, {
    headers: { 'X-User-Id': seniorId },
    data: { positionName: 'UIボタン確認テスト用', importance: 'Medium', currentHolderId: null },
  });
  const { id: positionId } = await posRes.json() as { id: string };

  // When
  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/succession/positions/${positionId}`);

  // Then
  await expect(page.locator('[data-testid="candidate-add-button"]')).toBeVisible({ timeout: 10000 });
  await expect(page.locator('[data-testid="candidate-add-button"]')).toBeEnabled();

  // Cleanup
  await deletePosition(request, positionId);
});

// ── サジェストボタンの表示 ────────────────────────────────────────────────
test('candidate-suggest-button: 候補者追加ダイアログに推薦候補ボタンが表示される', async ({ page, request }) => {
  // Given
  const posRes = await request.post(`${API}/critical-positions`, {
    headers: { 'X-User-Id': seniorId },
    data: { positionName: 'サジェストボタンテスト用', importance: 'High', currentHolderId: null },
  });
  const { id: positionId } = await posRes.json() as { id: string };

  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/succession/positions/${positionId}`);
  await expect(page.locator('[data-testid="candidate-add-button"]')).toBeVisible({ timeout: 10000 });

  // When: ダイアログを開く
  await page.click('[data-testid="candidate-add-button"]');

  // Then: 推薦候補ボタンが表示される
  await expect(page.locator('[data-testid="candidate-suggest-button"]')).toBeVisible();

  // Cleanup
  await deletePosition(request, positionId);
});
