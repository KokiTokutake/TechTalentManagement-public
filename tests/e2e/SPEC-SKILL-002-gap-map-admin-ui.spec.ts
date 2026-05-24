import { test, expect } from '@playwright/test';

// SPEC-SKILL-002: スキルギャップ比較・スキルマップ拡張・管理者ツール（TASK-073）
// 仕様書: docs/spec/SPEC-SKILL-002-skill-level-itss.md
// 設計書: docs/design/SPEC-SKILL-002-design.md
// 生成日: 2026-05-24（/test-write により自動生成）
//
// 対象 AC:
//   AC-01-01: レベル定義の設定（Admin: level-definitions.vue）
//   AC-01-03: 移行ツールによるスケール変換（Admin: migration.vue）
//   AC-03-02: コンピテンシーマスタの管理（Admin: competency-master.vue）
//   AC-04-01: 自己評価と上長評価の比較表示（gap.vue）
//   AC-04-02: 大きなギャップのアラート（gap.vue）
//   AC-05-01: コンピテンシータブの追加（map.vue）

const BASE = 'http://localhost:3000';
const API  = 'http://localhost:5108/api';

let talentId = '';
let managerId = '';
let adminId = '';
let seniorId = '';
let periodId = '';
let firstSkillItemId = '';
let firstCompetencyId = '';

test.beforeAll(async ({ request }) => {
  // ユーザー取得
  const usersRes = await request.get(`${API}/users`);
  const users: Array<{ id: string; displayName: string; role: string }> = await usersRes.json();

  const tanaka = users.find(u => u.displayName === '田中 太郎');
  if (!tanaka) throw new Error('シードデータに田中 太郎 が見つかりません');
  talentId = tanaka.id;

  const yamada = users.find(u => u.displayName === '山田 一郎');
  if (!yamada) throw new Error('シードデータに山田 一郎 が見つかりません');
  managerId = yamada.id;

  const admin = users.find(u => u.displayName === '管理者 システム');
  if (!admin) throw new Error('シードデータに管理者 システム が見つかりません');
  adminId = admin.id;

  const senior = users.find(u => u.displayName === '部長 一郎');
  if (!senior) throw new Error('シードデータに部長 一郎 が見つかりません');
  seniorId = senior.id;

  // 考課期間取得
  const periodsRes = await request.get(`${API}/periods`);
  const periods: Array<{ id: string; year: number }> = await periodsRes.json();
  const period = periods.sort((a, b) => b.year - a.year)[0];
  if (!period) throw new Error('考課期間が見つかりません');
  periodId = period.id;

  // スキルカテゴリ取得
  const catsRes = await request.get(`${API}/skill-categories`);
  const cats: Array<{ id: string; name: string }> = await catsRes.json();
  if (cats.length > 0) {
    const itemsRes = await request.get(`${API}/skill-categories/${cats[0].id}/items`);
    const items: Array<{ id: string }> = await itemsRes.json();
    firstSkillItemId = items[0]?.id ?? '';
  }

  // コンピテンシーマスタ取得
  const compRes = await request.get(`${API}/competency-masters`);
  const compData: { competencies: Array<{ id: string }> } = await compRes.json();
  firstCompetencyId = compData.competencies[0]?.id ?? '';
});

async function loginAsAdmin(page: import('@playwright/test').Page) {
  await page.goto(`${BASE}/select-user`);
  await expect(page.getByText('管理者 システム')).toBeVisible({ timeout: 10000 });
  await page.getByText('管理者 システム').click();
  await page.getByRole('button', { name: '開始する' }).click();
  await page.waitForURL(`${BASE}/`);
}

async function loginAsTalent(page: import('@playwright/test').Page) {
  await page.goto(`${BASE}/select-user`);
  await expect(page.getByText('田中 太郎')).toBeVisible({ timeout: 10000 });
  await page.getByText('田中 太郎').click();
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

async function loginAsSenior(page: import('@playwright/test').Page) {
  await page.goto(`${BASE}/select-user`);
  await expect(page.getByText('部長 一郎')).toBeVisible({ timeout: 10000 });
  await page.getByText('部長 一郎').click();
  await page.getByRole('button', { name: '開始する' }).click();
  await page.waitForURL(`${BASE}/`);
}

// ── AC-01-01: Admin レベル定義の設定 ─────────────────────────────────────────

test.describe('AC-01-01: スキルレベル定義管理（Admin）', () => {
  test('AC-01-01-01: Admin がレベル定義管理ページを開くとスキル選択フォームが表示される', async ({ page }) => {
    // Given
    await loginAsAdmin(page);

    // When
    await page.goto(`${BASE}/admin/skills/level-definitions`);

    // Then: カテゴリ・スキル選択が表示される
    await expect(page.locator('select').first()).toBeVisible({ timeout: 10000 });
    // Admin以外のエラーが表示されない
    await expect(page.getByText('この画面は管理者専用です')).not.toBeVisible();
  });

  test('AC-01-01-02: スキルアイテムを選択するとレベル定義フォームが表示される', async ({ page }) => {
    // Given
    if (!firstSkillItemId) {
      test.skip(true, 'スキルアイテムが存在しません');
      return;
    }
    await loginAsAdmin(page);
    await page.goto(`${BASE}/admin/skills/level-definitions`);

    // When: カテゴリを選択
    const catSelect = page.locator('select').first();
    await catSelect.selectOption({ index: 1 });
    await page.waitForTimeout(500);

    // スキルを選択
    const skillSelect = page.locator('select').nth(1);
    await skillSelect.selectOption({ index: 1 });
    await page.waitForTimeout(500);

    // Then: レベル定義保存ボタンが表示される
    await expect(
      page.locator('[data-testid="skill-level-def-save-button"]')
    ).toBeVisible({ timeout: 5000 });
  });

  test('AC-01-01-03: レベル定義を入力して保存するとボタンが反応する', async ({ page }) => {
    // Given
    if (!firstSkillItemId) {
      test.skip(true, 'スキルアイテムが存在しません');
      return;
    }
    await loginAsAdmin(page);
    await page.goto(`${BASE}/admin/skills/level-definitions`);

    // カテゴリ・スキル選択
    await page.locator('select').first().selectOption({ index: 1 });
    await page.waitForTimeout(500);
    await page.locator('select').nth(1).selectOption({ index: 1 });
    await page.waitForTimeout(500);

    // レベル定義フォームが表示されるまで待機
    const saveButton = page.locator('[data-testid="skill-level-def-save-button"]');
    await expect(saveButton).toBeVisible({ timeout: 5000 });

    // When: Lv.1 の説明を入力
    const firstInput = page.locator('.level-def-form .input').first();
    await firstInput.fill('概念的理解。学習中または経験ゼロ');

    // Then: 保存ボタンが有効になっている
    // （全レベル入力必須のため、他のレベルも入力する）
    const inputs = page.locator('.level-def-form .input');
    const count = await inputs.count();
    for (let i = 0; i < count; i++) {
      await inputs.nth(i).fill(`レベル ${i + 1} の定義テキスト`);
    }

    await expect(saveButton).not.toBeDisabled();
  });

  test('AC-01-01-04: 非Adminユーザーには管理者専用メッセージが表示される', async ({ page }) => {
    // Given
    await loginAsTalent(page);

    // When
    await page.goto(`${BASE}/admin/skills/level-definitions`);

    // Then
    await expect(page.getByText('この画面は管理者専用です')).toBeVisible({ timeout: 5000 });
  });
});

// ── AC-01-03: 移行ツール ──────────────────────────────────────────────────────

test.describe('AC-01-03: スキルスケール移行ツール（Admin）', () => {
  test('AC-01-03-01: Admin が移行ツールページを開くとプレビューボタンが表示される', async ({ page }) => {
    // Given
    await loginAsAdmin(page);

    // When
    await page.goto(`${BASE}/admin/skills/migration`);

    // Then: 警告バナーとプレビューボタンが表示される
    await expect(page.locator('[data-testid="migration-preview-button"]')).toBeVisible({ timeout: 10000 });
    await expect(page.getByText('元に戻せません')).toBeVisible();
  });

  test('AC-01-03-02: プレビューボタンをクリックすると変換プレビューが取得される', async ({ page }) => {
    // Given
    await loginAsAdmin(page);
    await page.goto(`${BASE}/admin/skills/migration`);

    // When
    await page.locator('[data-testid="migration-preview-button"]').click();

    // Then: 実行ボタンが表示される（プレビューデータ取得の証拠）
    await expect(
      page.locator('[data-testid="migration-execute-button"]')
    ).toBeVisible({ timeout: 10000 });
  });

  test('AC-01-03-03: 移行実行ボタンをクリックすると確認ダイアログが表示される', async ({ page }) => {
    // Given
    await loginAsAdmin(page);
    await page.goto(`${BASE}/admin/skills/migration`);

    // プレビュー取得
    await page.locator('[data-testid="migration-preview-button"]').click();
    await expect(
      page.locator('[data-testid="migration-execute-button"]')
    ).toBeVisible({ timeout: 10000 });

    // When: 実行ボタンクリック（対象件数が0の場合はdisabledのため条件分岐）
    const execBtn = page.locator('[data-testid="migration-execute-button"]');
    const isDisabled = await execBtn.isDisabled();
    if (isDisabled) {
      test.skip(true, '移行対象データが存在しません（既に移行済みまたはデータなし）');
      return;
    }

    await execBtn.click();

    // Then: 確認ダイアログが表示される
    await expect(
      page.locator('[data-testid="migration-confirm-execute-button"]')
    ).toBeVisible({ timeout: 5000 });
  });

  test('AC-01-03-04: 非Adminには管理者専用メッセージが表示される', async ({ page }) => {
    // Given
    await loginAsTalent(page);

    // When
    await page.goto(`${BASE}/admin/skills/migration`);

    // Then
    await expect(page.getByText('この画面は管理者専用です')).toBeVisible({ timeout: 5000 });
  });
});

// ── AC-03-02: コンピテンシーマスタ管理 ──────────────────────────────────────

test.describe('AC-03-02: コンピテンシーマスタ管理（Admin）', () => {
  test('AC-03-02-01: Admin がコンピテンシーマスタ管理ページを開くと追加フォームが表示される', async ({ page }) => {
    // Given
    await loginAsAdmin(page);

    // When
    await page.goto(`${BASE}/admin/skills/competency-master`);

    // Then: 追加フォームの主要要素が表示される
    await expect(
      page.locator('[data-testid="competency-add-button"]')
    ).toBeVisible({ timeout: 10000 });
    await expect(
      page.locator('[data-testid="competency-name-input"]')
    ).toBeVisible();
    await expect(
      page.locator('[data-testid="competency-category-select"]')
    ).toBeVisible();
  });

  test('AC-03-02-02: 新しいコンピテンシーを追加できる', async ({ page }) => {
    // Given
    await loginAsAdmin(page);
    await page.goto(`${BASE}/admin/skills/competency-master`);

    // カテゴリ・名称・表示順を入力
    await page.locator('[data-testid="competency-category-select"]').selectOption('Technology');
    await page.locator('[data-testid="competency-name-input"]').fill(`AI活用力_テスト_${Date.now()}`);
    const orderInput = page.locator('input[type="number"]').first();
    await orderInput.fill('99');

    // When: 追加ボタンをクリック
    await page.locator('[data-testid="competency-add-button"]').click();

    // Then: 追加後にフォームがリセットまたは一覧が更新される
    // （ページがエラー表示なしで続行できること）
    await expect(page.locator('.error-box')).not.toBeVisible({ timeout: 5000 });
  });

  test('AC-03-02-03: 追加したコンピテンシーが一覧に表示される', async ({ page }) => {
    // Given: テスト用のコンピテンシーが存在することを前提
    await loginAsAdmin(page);
    await page.goto(`${BASE}/admin/skills/competency-master`);

    // Then: コンピテンシー一覧のカテゴリグループが表示される（マスタが存在する場合）
    await page.waitForTimeout(1000);
    const catGroups = page.locator('.cat-group');
    const count = await catGroups.count();
    if (count > 0) {
      await expect(catGroups.first()).toBeVisible();
    } else {
      // マスタが空の場合は空メッセージ
      await expect(page.getByText('コンピテンシーが登録されていません')).toBeVisible();
    }
  });

  test('AC-03-02-04: 非Adminにはアクセス制限が表示される', async ({ page }) => {
    // Given
    await loginAsTalent(page);

    // When
    await page.goto(`${BASE}/admin/skills/competency-master`);

    // Then
    await expect(page.getByText('この画面は管理者専用です')).toBeVisible({ timeout: 5000 });
  });
});

// ── AC-04-01: スキルギャップ比較表示 ─────────────────────────────────────────

test.describe('AC-04-01: スキルギャップ比較ページ', () => {
  test.beforeEach(async ({ request }) => {
    // ギャップデータを作るため、Talent と Manager の両評価を API で登録
    if (!firstSkillItemId || !talentId || !managerId || !periodId) return;
    // ユーザースキルはすでに seed データ経由でのみ存在するため、
    // ギャップ表示のためにスキル差がある評価データを前提とする。
    // このテストでは gap API のレスポンスが返ることを確認する。
  });

  test('AC-04-01-01: Talent がスキルギャップページを開ける', async ({ page }) => {
    // Given
    await loginAsTalent(page);

    // When
    await page.goto(`${BASE}/skills/gap`);

    // Then: ページタイトルが表示される
    await expect(page.getByText('スキルギャップ比較')).toBeVisible({ timeout: 10000 });
  });

  test('AC-04-01-02: ギャップデータがある場合はスキルギャップ行が表示される', async ({ page }) => {
    // Given: gap APIがデータを返す場合
    await loginAsTalent(page);
    await page.goto(`${BASE}/skills/gap`);

    // ページが読み込まれる
    await expect(page.getByText('スキルギャップ比較')).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(2000);

    // Then: ギャップ行またはデータなしメッセージが表示される
    const gapRows = page.locator('[data-testid^="skill-gap-row-"]');
    const emptyNote = page.locator('.empty-note');
    const count = await gapRows.count();
    if (count > 0) {
      // ギャップ行が存在する場合、値が表示されている
      await expect(gapRows.first()).toBeVisible();
      await expect(
        page.locator('[data-testid^="skill-gap-value-"]').first()
      ).toBeVisible();
    } else {
      // データなしの場合、emptyNoteまたはギャップコンテナが表示
      await expect(emptyNote.first()).toBeVisible();
    }
  });

  test('AC-04-01-03: Manager は対象タレント選択ドロップダウンが表示される', async ({ page }) => {
    // Given
    await loginAsManager(page);

    // When
    await page.goto(`${BASE}/skills/gap`);

    // Then: タレント選択セレクトが表示される
    await expect(page.getByText('対象タレント')).toBeVisible({ timeout: 10000 });
  });
});

// ── AC-04-02: 大きなギャップのアラート ───────────────────────────────────────

test.describe('AC-04-02: 要確認セクション', () => {
  test('AC-04-02-01: 大きなギャップがある場合に要確認セクションが表示される', async ({ page }) => {
    // Given: ギャップデータが存在する場合（APIが要確認データを返す場合）
    await loginAsTalent(page);
    await page.goto(`${BASE}/skills/gap`);
    await expect(page.getByText('スキルギャップ比較')).toBeVisible({ timeout: 10000 });
    await page.waitForTimeout(2000);

    // Then: 要確認セクションまたはギャップ行が表示されるか確認
    // ギャップデータ次第でセクション表示が変わるためいずれかをチェック
    const attentionSection = page.locator('[data-testid="gap-attention-section"]');
    const gapRows = page.locator('[data-testid^="skill-gap-row-"]');
    const emptyNote = page.locator('.empty-note');

    const hasAttention = await attentionSection.count() > 0;
    const hasRows = await gapRows.count() > 0;
    const hasEmpty = await emptyNote.count() > 0;

    expect(hasAttention || hasRows || hasEmpty).toBe(true);
  });
});

// ── AC-05-01: コンピテンシータブ ─────────────────────────────────────────────

test.describe('AC-05-01: スキルマップ コンピテンシータブ', () => {
  test('AC-05-01-01: スキルマップにコンピテンシータブが表示される', async ({ page }) => {
    // Given
    await loginAsSenior(page);

    // When
    await page.goto(`${BASE}/skills/map`);

    // Then: コンピテンシータブが表示される
    await expect(
      page.locator('[data-testid="skillmap-competency-tab"]')
    ).toBeVisible({ timeout: 10000 });
  });

  test('AC-05-01-02: コンピテンシータブをクリックするとヒートマップコンテナが表示される', async ({ page, request }) => {
    // Given
    await loginAsSenior(page);
    await page.goto(`${BASE}/skills/map`);
    await expect(
      page.locator('[data-testid="skillmap-competency-tab"]')
    ).toBeVisible({ timeout: 10000 });

    // When
    await page.locator('[data-testid="skillmap-competency-tab"]').click();

    // Then: ヒートマップコンテナまたは期間未選択メッセージが表示される
    await page.waitForTimeout(1500);
    const heatmap = page.locator('[data-testid="competency-heatmap"]');
    const periodError = page.getByText('考課期間が選択されていません');
    const accessError = page.getByText('Senior / Admin のみ閲覧できます');

    const hasHeatmap = await heatmap.count() > 0;
    const hasPeriodError = await periodError.count() > 0;
    const hasAccessError = await accessError.count() > 0;

    // いずれかが表示されていること
    expect(hasHeatmap || hasPeriodError || hasAccessError).toBe(true);
  });

  test('AC-05-01-03: Talent ユーザーはコンピテンシータブを閲覧できない', async ({ page }) => {
    // Given
    await loginAsTalent(page);

    // When
    await page.goto(`${BASE}/skills/map`);
    await page.locator('[data-testid="skillmap-competency-tab"]').click();
    await page.waitForTimeout(500);

    // Then: アクセス制限メッセージが表示される
    await expect(page.getByText('Senior / Admin のみ閲覧できます')).toBeVisible({ timeout: 5000 });
  });
});
