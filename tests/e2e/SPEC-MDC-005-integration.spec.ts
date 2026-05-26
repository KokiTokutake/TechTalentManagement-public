import { test, expect } from '@playwright/test';

// SPEC-MDC-005: 後継者計画 — 結合テスト・受入確認（TASK-080）
// 仕様書: docs/spec/SPEC-MDC-005-succession-plan.md
// 設計書: docs/design/SPEC-MDC-005-design.md
// 生成日: 2026-05-26（/test-write により自動生成）
//
// 対象AC: AC-01-01, AC-01-02, AC-02-01, AC-02-02, AC-02-03, AC-03-01, AC-04-01, AC-04-02

const BASE = 'http://localhost:3000';
const API  = 'http://localhost:5108/api';

let seniorId = '';
let talentId = '';   // 田中 太郎
let talent2Id = '';  // 佐藤 花子
let talent3Id = '';  // 3人目
let periodId = '';   // 現在の考課期間

test.beforeAll(async ({ request }) => {
  const [usersRes, periodsRes] = await Promise.all([
    request.get(`${API}/users`),
    request.get(`${API}/periods`),
  ]);

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

  const periods: Array<{ id: string; year: number; isCurrent?: boolean; currentPhase?: string }> = await periodsRes.json();
  const current = periods.find(p => p.year === 2026) ?? periods[0];
  if (!current) throw new Error('考課期間が見つかりません');
  periodId = current.id;
});

async function loginAsSenior(page: import('@playwright/test').Page) {
  await page.goto(`${BASE}/select-user`);
  await expect(page.getByText('部長 一郎')).toBeVisible({ timeout: 10000 });
  await page.getByText('部長 一郎').click();
  await page.getByRole('button', { name: '開始する' }).click();
  await page.waitForURL(`${BASE}/`);
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

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 結合シナリオ1: REQ-01 重要ポジション管理（AC-01-01, AC-01-02）
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
test('結合AC-01: ポジション登録 → 重要度順ソート（AC-01-01 + AC-01-02）', async ({ page, request }) => {
  // Given: HighとMediumのポジションをAPIで事前登録
  const highRes = await request.post(`${API}/critical-positions`, {
    headers: { 'X-User-Id': seniorId },
    data: { positionName: '統合テスト-High', importance: 'High', currentHolderId: null },
  });
  const { id: highId } = await highRes.json() as { id: string };

  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/succession/positions`);
  await expect(page.locator('[data-testid="position-save-button"]')).toBeVisible({ timeout: 10000 });

  // AC-01-01: UIからCriticalポジションを追加
  await page.fill('[data-testid="position-name-input"]', '統合テスト-Critical');
  await page.selectOption('[data-testid="position-importance-select"]', 'Critical');
  await page.click('[data-testid="position-save-button"]');

  // Criticalポジションが一覧に追加される
  await expect(page.getByText('統合テスト-Critical')).toBeVisible({ timeout: 8000 });

  // AC-01-02: 重要度順でCriticalがHighより先に表示される
  const rows = page.locator('.data-table tbody tr');
  const texts = await rows.allInnerTexts();
  const critIdx = texts.findIndex(t => t.includes('統合テスト-Critical'));
  const highIdx = texts.findIndex(t => t.includes('統合テスト-High'));
  expect(critIdx).toBeGreaterThanOrEqual(0);
  expect(highIdx).toBeGreaterThanOrEqual(0);
  expect(critIdx).toBeLessThan(highIdx);

  // Cleanup
  const listRes = await request.get(`${API}/critical-positions?requesterId=${seniorId}`);
  const body = await listRes.json() as { positions: Array<{ id: string; positionName: string }> };
  const created = body.positions.find(p => p.positionName === '統合テスト-Critical');
  if (created) await deactivatePosition(request, created.id);
  await deactivatePosition(request, highId);
});

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 結合シナリオ2: REQ-02 候補者管理（AC-02-01, AC-02-02, AC-02-03）
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
test('結合AC-02: 候補者追加 → 複数登録 → Readiness更新（AC-02-01 + AC-02-02 + AC-02-03）', async ({ page, request }) => {
  // Given: テスト用ポジションを作成
  const posRes = await request.post(`${API}/critical-positions`, {
    headers: { 'X-User-Id': seniorId },
    data: { positionName: '統合テスト-候補者管理', importance: 'Critical', currentHolderId: null },
  });
  const { id: positionId } = await posRes.json() as { id: string };

  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/succession/positions/${positionId}`);
  await expect(page.locator('[data-testid="candidate-add-button"]')).toBeVisible({ timeout: 10000 });

  // AC-02-01: 1人目の候補者をUIから追加（田中 太郎、ReadySoon）
  await page.click('[data-testid="candidate-add-button"]');
  await expect(page.locator('[data-testid="candidate-talent-select"]')).toBeVisible();
  await page.selectOption('[data-testid="candidate-talent-select"]', { label: '田中 太郎' });
  await page.selectOption('[data-testid="candidate-readiness-select"]', 'ReadySoon');
  await page.click('[data-testid="candidate-confirm-button"]');
  await expect(page.getByText('田中 太郎')).toBeVisible({ timeout: 8000 });

  // AC-02-02: 2人目を追加（佐藤 花子、ReadyNow）— 上限制限なし
  await page.click('[data-testid="candidate-add-button"]');
  await expect(page.locator('[data-testid="candidate-talent-select"]')).toBeVisible();
  await page.selectOption('[data-testid="candidate-talent-select"]', { label: '佐藤 花子' });
  await page.selectOption('[data-testid="candidate-readiness-select"]', 'ReadyNow');
  await page.click('[data-testid="candidate-confirm-button"]');
  await expect(page.getByText('佐藤 花子')).toBeVisible({ timeout: 8000 });

  // 2名とも表示されること
  await expect(page.getByText('田中 太郎')).toBeVisible();
  await expect(page.getByText('佐藤 花子')).toBeVisible();

  // AC-02-03: 田中 太郎のReadinessをReadyNowに昇格
  const readinessSelects = page.locator('.readiness-select');
  // 田中 太郎のReadySoonセレクトを探して変更
  const tanakaCard = page.locator('.candidate-card').filter({ hasText: '田中 太郎' });
  const tanakaSelect = tanakaCard.locator('.readiness-select');
  await tanakaSelect.selectOption('ReadyNow');
  const saveBtn = tanakaCard.locator('[data-testid="candidate-readiness-save-button"]');
  await expect(saveBtn).toBeEnabled();
  await saveBtn.click();
  // 保存完了後、ボタンが無効化される
  await expect(saveBtn).toBeDisabled({ timeout: 8000 });

  // Cleanup
  await deactivatePosition(request, positionId);
});

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 結合シナリオ3: REQ-03 9ボックス連動サジェスト（AC-03-01）
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
test('結合AC-03: 9ボックスサジェストAPIが候補者推薦リストを返す（AC-03-01）', async ({ request }) => {
  // Given: テスト用ポジション作成
  const posRes = await request.post(`${API}/critical-positions`, {
    headers: { 'X-User-Id': seniorId },
    data: { positionName: '統合テスト-サジェスト', importance: 'High', currentHolderId: null },
  });
  const { id: positionId } = await posRes.json() as { id: string };

  // When: サジェストAPIを呼ぶ（High/Mid Potentialタレントのリスト取得）
  const suggestRes = await request.get(
    `${API}/succession-map/suggest?requesterId=${seniorId}&periodId=${periodId}&positionId=${positionId}`,
  );

  // Then: 200 OK でレスポンスが返る
  expect(suggestRes.ok()).toBeTruthy();
  const body = await suggestRes.json() as {
    suggestions: Array<{
      talentId: string
      talentName: string
      potentialLevel: string
      performanceLevel: string
      alreadyCandidate: boolean
    }>
  };
  expect(Array.isArray(body.suggestions)).toBeTruthy();

  // シードデータに9ボックス評価があれば、High/Mid Potentialのタレントが返る
  // alreadyCandidateが正しく設定されている
  for (const s of body.suggestions) {
    expect(['High', 'Mid']).toContain(s.potentialLevel);
    expect(typeof s.alreadyCandidate).toBe('boolean');
    expect(s.alreadyCandidate).toBe(false); // 新規ポジションなので全員false
  }

  // AC-03-01: 候補者として追加済みの場合alreadyCandidateがtrueになる
  if (body.suggestions.length > 0) {
    const firstSuggestion = body.suggestions[0];
    await request.post(`${API}/critical-positions/${positionId}/candidates`, {
      headers: { 'X-User-Id': seniorId },
      data: { talentId: firstSuggestion.talentId, readiness: 'ReadySoon', developmentGap: null },
    });

    const suggestRes2 = await request.get(
      `${API}/succession-map/suggest?requesterId=${seniorId}&periodId=${periodId}&positionId=${positionId}`,
    );
    const body2 = await suggestRes2.json() as typeof body;
    const updated = body2.suggestions.find(s => s.talentId === firstSuggestion.talentId);
    if (updated) {
      expect(updated.alreadyCandidate).toBe(true);
    }
  }

  // Cleanup
  await deactivatePosition(request, positionId);
});

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 結合シナリオ4: REQ-04 サクセッションマップ（AC-04-01, AC-04-02）
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
test('結合AC-04: サクセッションマップグリッド表示＋カバレッジアラート（AC-04-01 + AC-04-02）', async ({ page, request }) => {
  // Given: Critical ポジション（ReadyNowなし）+ High ポジション（ReadyNow + ReadySoon）
  const critRes = await request.post(`${API}/critical-positions`, {
    headers: { 'X-User-Id': seniorId },
    data: { positionName: '統合マップ-Critical', importance: 'Critical', currentHolderId: null },
  });
  const { id: critId } = await critRes.json() as { id: string };

  const highRes = await request.post(`${API}/critical-positions`, {
    headers: { 'X-User-Id': seniorId },
    data: { positionName: '統合マップ-High', importance: 'High', currentHolderId: null },
  });
  const { id: highId } = await highRes.json() as { id: string };

  // Critical: ReadySoonのみ（ReadyNowなし → hasReadyNowGap=true）
  await request.post(`${API}/critical-positions/${critId}/candidates`, {
    headers: { 'X-User-Id': seniorId },
    data: { talentId: talentId, readiness: 'ReadySoon', developmentGap: null },
  });

  // High: ReadyNow + ReadyMidTerm
  await request.post(`${API}/critical-positions/${highId}/candidates`, {
    headers: { 'X-User-Id': seniorId },
    data: { talentId: talent2Id, readiness: 'ReadyNow', developmentGap: null },
  });
  await request.post(`${API}/critical-positions/${highId}/candidates`, {
    headers: { 'X-User-Id': seniorId },
    data: { talentId: talent3Id, readiness: 'ReadyMidTerm', developmentGap: null },
  });

  // When: サクセッションマップ画面を開く
  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/succession`);

  // AC-04-01: マップグリッドが表示される
  await expect(page.locator('[data-testid="succession-map"]')).toBeVisible({ timeout: 10000 });

  // CriticalポジションのReadySoonセルに田中 太郎が表示される
  await expect(page.locator(`[data-testid="map-cell-${critId}-ReadySoon"]`).getByText('田中 太郎')).toBeVisible();

  // HighポジションのReadyNowセルに佐藤 花子が表示される
  await expect(page.locator(`[data-testid="map-cell-${highId}-ReadyNow"]`).getByText('佐藤 花子')).toBeVisible();

  // AC-04-02: Criticalポジション（ReadyNowなし）に「後継者未定」バッジが表示される
  await expect(page.locator(`[data-testid="map-gap-badge-${critId}"]`)).toBeVisible();
  await expect(page.locator(`[data-testid="map-gap-badge-${critId}"]`)).toContainText('後継者未定');

  // HighポジションにはReadyNow候補者がいるのでバッジなし
  await expect(page.locator(`[data-testid="map-gap-badge-${highId}"]`)).not.toBeVisible();

  // Cleanup
  await deactivatePosition(request, critId);
  await deactivatePosition(request, highId);
});

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 結合シナリオ5: フルシナリオ — ポジション登録からマップ確認まで
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
test('フルシナリオ: ポジション登録→候補者追加→マップ反映（全AC連携）', async ({ page, request }) => {
  // Step 1: ポジション管理ページでCriticalポジションを登録（AC-01-01）
  await loginAsSenior(page);
  await page.goto(`${BASE}/mdc/succession/positions`);
  await expect(page.locator('[data-testid="position-save-button"]')).toBeVisible({ timeout: 10000 });

  await page.fill('[data-testid="position-name-input"]', 'フルシナリオ-テックリード');
  await page.selectOption('[data-testid="position-importance-select"]', 'Critical');
  await page.click('[data-testid="position-save-button"]');
  await expect(page.getByText('フルシナリオ-テックリード')).toBeVisible({ timeout: 8000 });

  // ポジションIDを取得
  const listRes = await request.get(`${API}/critical-positions?requesterId=${seniorId}`);
  const listBody = await listRes.json() as { positions: Array<{ id: string; positionName: string }> };
  const newPos = listBody.positions.find(p => p.positionName === 'フルシナリオ-テックリード');
  if (!newPos) throw new Error('作成したポジションが見つかりません');
  const positionId = newPos.id;

  // Step 2: ポジション詳細で候補者を追加（AC-02-01）
  await page.goto(`${BASE}/mdc/succession/positions/${positionId}`);
  await expect(page.locator('[data-testid="candidate-add-button"]')).toBeVisible({ timeout: 10000 });

  await page.click('[data-testid="candidate-add-button"]');
  await expect(page.locator('[data-testid="candidate-talent-select"]')).toBeVisible();
  await page.selectOption('[data-testid="candidate-talent-select"]', { label: '田中 太郎' });
  await page.selectOption('[data-testid="candidate-readiness-select"]', 'ReadySoon');
  await page.click('[data-testid="candidate-confirm-button"]');
  await expect(page.getByText('田中 太郎')).toBeVisible({ timeout: 8000 });

  // Step 3: サクセッションマップで反映確認（AC-04-01, AC-04-02）
  await page.goto(`${BASE}/mdc/succession`);
  await expect(page.locator('[data-testid="succession-map"]')).toBeVisible({ timeout: 10000 });

  // ReadySoonに田中 太郎が表示される（AC-04-01）
  await expect(
    page.locator(`[data-testid="map-cell-${positionId}-ReadySoon"]`).getByText('田中 太郎'),
  ).toBeVisible({ timeout: 8000 });

  // ReadyNow候補がいないのでgapバッジが表示される（AC-04-02）
  await expect(page.locator(`[data-testid="map-gap-badge-${positionId}"]`)).toBeVisible();

  // Cleanup
  await deactivatePosition(request, positionId);
});
