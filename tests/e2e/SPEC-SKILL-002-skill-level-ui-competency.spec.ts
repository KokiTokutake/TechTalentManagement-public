import { test, expect } from '@playwright/test';

// SPEC-SKILL-002: スキルレベル参照 UI 拡張 + コンピテンシー評価ページ（TASK-072）
// 仕様書: docs/spec/SPEC-SKILL-002-skill-level-itss.md
// 設計書: docs/design/SPEC-SKILL-002-design.md
// 生成日: 2026-05-24（/test-write により自動生成）
//
// 対象 AC:
//   AC-01-02: 未設定スキルのフォールバック表示
//   AC-02-01: レベル定義の参照（ツールチップ）
//   AC-03-01: コンピテンシー評価の入力（保存・取得）

const BASE = 'http://localhost:3000';
const API  = 'http://localhost:5108/api';

let talentId = '';
let managerId = '';
let periodId = '';
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

  // 考課期間取得
  const periodsRes = await request.get(`${API}/periods`);
  const periods: Array<{ id: string; year: number }> = await periodsRes.json();
  const period = periods.sort((a, b) => b.year - a.year)[0];
  if (!period) throw new Error('考課期間が見つかりません');
  periodId = period.id;

  // コンピテンシーマスタ取得
  const compRes = await request.get(`${API}/competency-masters`);
  const compData: { competencies: Array<{ id: string }> } = await compRes.json();
  if (!compData.competencies.length) throw new Error('コンピテンシーマスタが見つかりません');
  firstCompetencyId = compData.competencies[0].id;
});

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

// ── AC-02-01: レベル定義の参照（ツールチップ） ───────────────────────────────

test.describe('AC-02-01: スキルレベル定義ツールチップ', () => {
  test('AC-02-01-01: 登録済みスキルの編集モードで ? アイコンが表示される', async ({ page }) => {
    // Given
    await loginAsTalent(page);
    await page.goto(`${BASE}/skills/my`);

    // 登録済みスキルが表示されるまで待機
    const editButton = page.getByRole('button', { name: '編集' }).first();
    await expect(editButton).toBeVisible({ timeout: 10000 });

    // When: 編集ボタンをクリックして編集モードに入る
    await editButton.click();

    // Then: ツールチップトリガーが表示される
    await expect(
      page.locator('[data-testid="skill-level-tooltip-trigger"]').first()
    ).toBeVisible({ timeout: 5000 });
  });

  test('AC-02-01-02: ? アイコンをクリックするとレベル定義ツールチップが表示される', async ({ page }) => {
    // Given
    await loginAsTalent(page);
    await page.goto(`${BASE}/skills/my`);

    // 登録済みスキルを編集モードにする
    const editButton = page.getByRole('button', { name: '編集' }).first();
    await expect(editButton).toBeVisible({ timeout: 10000 });
    await editButton.click();

    // When: ツールチップを開く
    await page.locator('[data-testid="skill-level-tooltip-trigger"]').first().click();

    // Then: ツールチップコンテンツが表示される
    await expect(
      page.locator('[data-testid="skill-level-tooltip-content"]')
    ).toBeVisible({ timeout: 5000 });

    // Level 1〜7 の行が表示されること
    const rows = page.locator('[data-testid="skill-level-tooltip-content"] .level-table tbody tr');
    await expect(rows).toHaveCount(7);
  });

  test('AC-01-02-01: ツールチップにレベル定義テキストが表示される（フォールバック含む）', async ({ page }) => {
    // Given
    await loginAsTalent(page);
    await page.goto(`${BASE}/skills/my`);

    // 登録済みスキルを編集モードにする
    const editButton = page.getByRole('button', { name: '編集' }).first();
    await expect(editButton).toBeVisible({ timeout: 10000 });
    await editButton.click();

    // When: ツールチップを開く
    await page.locator('[data-testid="skill-level-tooltip-trigger"]').first().click();
    await expect(
      page.locator('[data-testid="skill-level-tooltip-content"]')
    ).toBeVisible({ timeout: 5000 });

    // Then: レベル定義テキストが存在すること（フォールバックまたはカスタム）
    await expect(
      page.locator('[data-testid="skill-level-tooltip-content"] .def-text').first()
    ).toBeVisible();
  });

  test('AC-02-01-03: スキルレベルセレクトが 1〜7 の 7段階を持つ', async ({ page }) => {
    // Given
    await loginAsTalent(page);
    await page.goto(`${BASE}/skills/my`);

    // When
    const levelSelect = page.locator('[data-testid="skill-level-select"]').first();
    await expect(levelSelect).toBeVisible({ timeout: 10000 });
    const options = levelSelect.locator('option');

    // Then: 7段階の選択肢
    await expect(options).toHaveCount(7);
    await expect(options.nth(0)).toContainText('1');
    await expect(options.nth(6)).toContainText('7');
  });
});

// ── AC-03-01: コンピテンシー評価ページ（Talent） ─────────────────────────────

test.describe('AC-03-01: コンピテンシー評価ページ（Talent）', () => {
  test('AC-03-01-01: タレントがコンピテンシー評価ページを開くとフォームが表示される', async ({ page }) => {
    // Given
    await loginAsTalent(page);

    // When
    await page.goto(`${BASE}/skills/competency`);

    // Then: 保存ボタンが表示されること
    await expect(
      page.locator('[data-testid="competency-eval-save-button"]')
    ).toBeVisible({ timeout: 10000 });
  });

  test('AC-03-01-02: コンピテンシーレベルセレクトが表示され選択できる', async ({ page }) => {
    // Given
    await loginAsTalent(page);
    await page.goto(`${BASE}/skills/competency`);
    await expect(
      page.locator('[data-testid="competency-eval-save-button"]')
    ).toBeVisible({ timeout: 10000 });

    // When: 最初のコンピテンシーレベルセレクトを操作
    const firstSelect = page.locator('[data-testid^="competency-level-select-"]').first();
    await expect(firstSelect).toBeVisible({ timeout: 5000 });
    await firstSelect.selectOption('3');

    // Then: 選択値が反映されている
    await expect(firstSelect).toHaveValue('3');
  });

  test('AC-03-01-03: 評価を保存すると保存完了メッセージが表示される', async ({ page }) => {
    // Given
    await loginAsTalent(page);
    await page.goto(`${BASE}/skills/competency`);
    await expect(
      page.locator('[data-testid="competency-eval-save-button"]')
    ).toBeVisible({ timeout: 10000 });

    // When: レベルを選択して保存
    const firstSelect = page.locator('[data-testid^="competency-level-select-"]').first();
    await firstSelect.selectOption('3');
    await page.locator('[data-testid="competency-eval-save-button"]').click();

    // Then
    await expect(page.getByText('保存しました')).toBeVisible({ timeout: 5000 });
  });

  test('AC-03-01-04: 保存後に再アクセスすると保存した評価が表示される', async ({ page, request }) => {
    // Given: APIで評価を事前保存
    await request.post(`${API}/competency-evaluations?requesterId=${talentId}`, {
      data: {
        talentId,
        periodId,
        evaluatorType: 'Self',
        evaluations: [{ competencyId: firstCompetencyId, level: 4, comment: 'UIテスト' }],
      },
    });

    // When
    await loginAsTalent(page);
    await page.goto(`${BASE}/skills/competency`);
    await expect(
      page.locator('[data-testid="competency-eval-save-button"]')
    ).toBeVisible({ timeout: 10000 });

    // Then: 保存した評価が反映されていること（最初のセレクトの値が4）
    const firstSelect = page.locator('[data-testid^="competency-level-select-"]').first();
    await expect(firstSelect).toHaveValue('4');
  });
});

// ── AC-03-01: コンピテンシー評価ページ（Manager タブ切替） ───────────────────────

test.describe('AC-03-01: コンピテンシー評価ページ（Manager）', () => {
  test('AC-03-01-05: Manager がコンピテンシー評価ページを開くと自己/上長タブが表示される', async ({ page }) => {
    // Given
    await loginAsManager(page);

    // When
    await page.goto(`${BASE}/skills/competency`);

    // Then: タブが表示されること
    await expect(
      page.locator('[data-testid="competency-tab"]')
    ).toBeVisible({ timeout: 10000 });
    await expect(page.getByText('上長評価（タレント選択）')).toBeVisible();
  });

  test('AC-03-01-06: Manager が上長評価タブでタレントを選択するとフォームが表示される', async ({ page }) => {
    // Given
    await loginAsManager(page);
    await page.goto(`${BASE}/skills/competency`);
    await expect(
      page.locator('[data-testid="competency-tab"]')
    ).toBeVisible({ timeout: 10000 });

    // When: 上長評価タブに切り替え
    await page.getByText('上長評価（タレント選択）').click();

    // タレント選択セレクトが表示される
    const talentSelect = page.locator('select').first();
    await expect(talentSelect).toBeVisible({ timeout: 3000 });

    // タレントを選択（最初のオプション）
    await talentSelect.selectOption({ index: 1 });

    // Then: フォームが表示される
    await expect(
      page.locator('[data-testid="competency-eval-save-button"]')
    ).toBeVisible({ timeout: 5000 });
  });
});

// ── ナビゲーション確認 ────────────────────────────────────────────────────────

test.describe('ナビゲーション', () => {
  test('ホームページにコンピテンシー評価へのリンクが表示される', async ({ page }) => {
    // Given
    await loginAsTalent(page);

    // Then
    await expect(page.getByText('コンピテンシー評価')).toBeVisible({ timeout: 10000 });
  });
});
