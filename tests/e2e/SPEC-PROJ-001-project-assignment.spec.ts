import { test, expect } from '@playwright/test';

// SPEC-PROJ-001: プロジェクト・アサインメント履歴管理 — UI E2E テスト（TASK-084）
// 仕様書: docs/spec/SPEC-PROJ-001-project-assignment.md
// 設計書: docs/design/SPEC-PROJ-001-design.md

const API = 'http://localhost:5108/api';

let talentId = '';
let managerId = '';

// ── セットアップ: タレント・マネージャーの ID を取得 ──────────────────────
test.beforeAll(async ({ request }) => {
  const usersRes = await request.get(`${API}/users`);
  const users: Array<{ id: string; displayName: string; role: string }> = await usersRes.json();

  const tanaka = users.find(u => u.displayName === '田中 太郎');
  if (!tanaka) throw new Error('シードデータに田中 太郎 が見つかりません');
  talentId = tanaka.id;

  const yamada = users.find(u => u.displayName === '山田 一郎');
  if (!yamada) throw new Error('シードデータに山田 一郎 が見つかりません');
  managerId = yamada.id;
});

// ── AC-01-01: プロジェクトの登録 ─────────────────────────────────────────

test('AC-01-01: プロジェクトを登録すると一覧に追加される', async ({ page, request }) => {
  // 事前にプロジェクト一覧をクリア（このテスト用データのみ）
  await page.goto(`/profile/projects?userId=${talentId}`);

  // When
  await page.click('[data-testid="project-add-button"]');
  await page.fill('[data-testid="project-name-input"]', '次世代バンキングシステム_UI_01');
  await page.fill('[data-testid="project-role-input"]', 'バックエンドテックリード');
  await page.fill('[data-testid="project-start-date-input"]', '2024-04-01');
  await page.click('[data-testid="project-save-button"]');

  // Then
  await expect(page.locator('[data-testid^="project-row-"]').filter({ hasText: '次世代バンキングシステム_UI_01' })).toBeVisible();

  // クリーンアップ
  const listRes = await request.get(`${API}/project-assignments?talentId=${talentId}`, { headers: { 'X-User-Id': talentId } });
  const list: Array<{ id: string; projectName: string }> = await listRes.json();
  const created = list.find(a => a.projectName === '次世代バンキングシステム_UI_01');
  if (created) await request.delete(`${API}/project-assignments/${created.id}`, { headers: { 'X-User-Id': talentId } });
});

// ── AC-01-02: プロジェクトの編集 ─────────────────────────────────────────

test('AC-01-02: プロジェクト名を変更すると一覧に反映される', async ({ page, request }) => {
  // Given: テスト用プロジェクトを事前登録
  const createRes = await request.post(`${API}/project-assignments`, {
    data: { projectName: '編集テスト_元の名前', role: 'エンジニア', startDate: '2024-01-01', endDate: null, description: null, skillItemIds: [] },
    headers: { 'X-User-Id': talentId },
  });
  expect(createRes.ok()).toBe(true);
  const { id } = await createRes.json();

  await page.goto(`/profile/projects?userId=${talentId}`);
  const row = page.locator(`[data-testid="project-row-${id}"]`);
  await expect(row).toBeVisible();

  // When
  await row.locator('[data-testid="project-edit-button"]').click();
  await page.fill('[data-testid="project-name-input"]', '編集テスト_変更後の名前');
  await page.click('[data-testid="project-save-button"]');

  // Then
  await expect(page.locator('[data-testid^="project-row-"]').filter({ hasText: '編集テスト_変更後の名前' })).toBeVisible();

  // クリーンアップ
  await request.delete(`${API}/project-assignments/${id}`, { headers: { 'X-User-Id': talentId } });
});

// ── AC-01-03: プロジェクトの削除 ─────────────────────────────────────────

test('AC-01-03: 削除ボタンを押して確認すると一覧から消える', async ({ page, request }) => {
  // Given: テスト用プロジェクトを事前登録
  const createRes = await request.post(`${API}/project-assignments`, {
    data: { projectName: '削除テストプロジェクト', role: 'エンジニア', startDate: '2024-01-01', endDate: null, description: null, skillItemIds: [] },
    headers: { 'X-User-Id': talentId },
  });
  expect(createRes.ok()).toBe(true);
  const { id } = await createRes.json();

  await page.goto(`/profile/projects?userId=${talentId}`);
  const row = page.locator(`[data-testid="project-row-${id}"]`);
  await expect(row).toBeVisible();

  // When
  await row.locator('[data-testid="project-delete-button"]').click();
  await page.click('[data-testid="project-delete-confirm-button"]');

  // Then
  await expect(page.locator(`[data-testid="project-row-${id}"]`)).not.toBeVisible();
});

// ── AC-02-01: 参画中プロジェクト ─────────────────────────────────────────

test('AC-02-01: 終了日なしで登録すると「参画中」ラベルが表示される', async ({ page, request }) => {
  // Given
  await page.goto(`/profile/projects?userId=${talentId}`);
  await page.click('[data-testid="project-add-button"]');

  // When: 終了日を入力しない
  await page.fill('[data-testid="project-name-input"]', '参画中プロジェクト_UI_02');
  await page.fill('[data-testid="project-role-input"]', 'フロントエンドエンジニア');
  await page.fill('[data-testid="project-start-date-input"]', '2025-01-01');
  await page.click('[data-testid="project-save-button"]');

  // Then
  const row = page.locator('[data-testid^="project-row-"]').filter({ hasText: '参画中プロジェクト_UI_02' });
  await expect(row).toBeVisible();
  await expect(row.locator('[data-testid="project-active-badge"]')).toBeVisible();

  // クリーンアップ
  const listRes = await request.get(`${API}/project-assignments?talentId=${talentId}`, { headers: { 'X-User-Id': talentId } });
  const list: Array<{ id: string; projectName: string }> = await listRes.json();
  const created = list.find(a => a.projectName === '参画中プロジェクト_UI_02');
  if (created) await request.delete(`${API}/project-assignments/${created.id}`, { headers: { 'X-User-Id': talentId } });
});

// ── AC-02-02: 終了済みプロジェクト ───────────────────────────────────────

test('AC-02-02: 開始日・終了日を両方入力すると期間が表示され「参画中」バッジは出ない', async ({ page, request }) => {
  // Given
  await page.goto(`/profile/projects?userId=${talentId}`);
  await page.click('[data-testid="project-add-button"]');

  // When
  await page.fill('[data-testid="project-name-input"]', '過去プロジェクト_UI_02');
  await page.fill('[data-testid="project-role-input"]', 'バックエンドエンジニア');
  await page.fill('[data-testid="project-start-date-input"]', '2023-01-01');
  await page.fill('[data-testid="project-end-date-input"]', '2023-12-31');
  await page.click('[data-testid="project-save-button"]');

  // Then
  const row = page.locator('[data-testid^="project-row-"]').filter({ hasText: '過去プロジェクト_UI_02' });
  await expect(row).toBeVisible();
  await expect(row).toContainText('2023');
  await expect(row.locator('[data-testid="project-active-badge"]')).not.toBeVisible();

  // クリーンアップ
  const listRes = await request.get(`${API}/project-assignments?talentId=${talentId}`, { headers: { 'X-User-Id': talentId } });
  const list: Array<{ id: string; projectName: string }> = await listRes.json();
  const created = list.find(a => a.projectName === '過去プロジェクト_UI_02');
  if (created) await request.delete(`${API}/project-assignments/${created.id}`, { headers: { 'X-User-Id': talentId } });
});

// ── AC-05-01: 表示順（参画中が先頭） ──────────────────────────────────────

test('AC-05-01: 参画中プロジェクトが一覧の先頭に表示される', async ({ page, request }) => {
  // Given: 参画中と終了済みを登録
  const activeRes = await request.post(`${API}/project-assignments`, {
    data: { projectName: '順序テスト_参画中_UI', role: 'エンジニア', startDate: '2025-06-01', endDate: null, description: null, skillItemIds: [] },
    headers: { 'X-User-Id': talentId },
  });
  const { id: activeId } = await activeRes.json();

  const closedRes = await request.post(`${API}/project-assignments`, {
    data: { projectName: '順序テスト_終了済み_UI', role: 'エンジニア', startDate: '2024-01-01', endDate: '2024-12-31', description: null, skillItemIds: [] },
    headers: { 'X-User-Id': talentId },
  });
  const { id: closedId } = await closedRes.json();

  // When
  await page.goto(`/profile/projects?userId=${talentId}`);

  // Then: 参画中バッジが先頭行にある
  const firstRow = page.locator('[data-testid^="project-row-"]').first();
  await expect(firstRow.locator('[data-testid="project-active-badge"]')).toBeVisible();

  // クリーンアップ
  await request.delete(`${API}/project-assignments/${activeId}`, { headers: { 'X-User-Id': talentId } });
  await request.delete(`${API}/project-assignments/${closedId}`, { headers: { 'X-User-Id': talentId } });
});

// ── AC-08-01: 上長コメント ────────────────────────────────────────────────

test('AC-08-01: 上長がタレントプロファイルからコメントを保存するとプロジェクト行に表示される', async ({ page, request }) => {
  // Given: タレントのプロジェクトを事前登録
  const createRes = await request.post(`${API}/project-assignments`, {
    data: { projectName: 'コメントテスト用プロジェクト_UI', role: 'テックリード', startDate: '2024-04-01', endDate: null, description: null, skillItemIds: [] },
    headers: { 'X-User-Id': talentId },
  });
  expect(createRes.ok()).toBe(true);
  const { id } = await createRes.json();

  // When: タレントプロファイルページでコメント入力
  await page.goto(`/mdc/talent-profile/${talentId}?managerId=${managerId}`);
  const row = page.locator(`[data-testid="project-row-${id}"]`);
  await expect(row).toBeVisible();

  await row.locator('[data-testid="project-manager-comment-input"]').fill('アーキテクチャ設計において主体的にリードし高い貢献を発揮した');
  await row.locator('[data-testid="project-manager-comment-save-button"]').click();

  // Then: コメントが表示される
  await expect(row).toContainText('アーキテクチャ設計において主体的にリードし高い貢献を発揮した');

  // クリーンアップ
  await request.delete(`${API}/project-assignments/${id}`, { headers: { 'X-User-Id': talentId } });
});
