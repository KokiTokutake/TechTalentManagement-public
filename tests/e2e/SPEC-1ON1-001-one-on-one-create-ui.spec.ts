import { test, expect } from '@playwright/test'

// SPEC-1ON1-001: 1on1面談記録機能
// 仕様書: docs/spec/SPEC-1ON1-001-one-on-one-meeting.md
// 対象タスク: TASK-048 [FE] 1on1記録作成・編集画面 UI

const BASE_URL = 'http://localhost:3000'
const API_BASE = 'http://localhost:5108'

// テストデータ（seederからの固定値）
const MANAGER = { id: '147469a5-e953-4afd-9ada-d67d9d271f13', displayName: '山田 一郎' }
const TALENT = { id: '7e4acb26-9a8d-4773-a572-bdcffb663166', displayName: '佐藤 花子' }

async function loginAsManager(page: import('@playwright/test').Page) {
  await page.goto(`${BASE_URL}/select-user`)
  await expect(page.getByText(MANAGER.displayName)).toBeVisible({ timeout: 10000 })
  await page.getByText(MANAGER.displayName).click()
  await page.getByRole('button', { name: '開始する' }).click()
  await page.waitForURL(`${BASE_URL}/`)
}

async function goToCreatePage(page: import('@playwright/test').Page) {
  await page.goto(`${BASE_URL}/one-on-one/create?talentId=${TALENT.id}&talentName=${encodeURIComponent(TALENT.displayName)}`)
}

test.describe('SPEC-1ON1-001 REQ-01: 1on1記録の作成', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsManager(page)
  })

  // AC-01-01: 基本情報の入力と保存
  test('実施日・形式・メモを入力して保存すると成功メッセージが表示される', async ({ page }) => {
    // Given
    await goToCreatePage(page)
    await expect(page.locator('h1')).toContainText('1on1 面談記録')
    await expect(page.getByText(TALENT.displayName)).toBeVisible()

    // When
    const today = new Date().toISOString().slice(0, 10)
    await page.locator('[data-testid="meeting-conducted-at-input"]').fill(today)
    await page.locator('[data-testid="meeting-format-select"]').selectOption('Online')
    await page.locator('[data-testid="meeting-will-topics-input"]').fill('テックリードを目指したい')
    await page.locator('[data-testid="meeting-can-topics-input"]').fill('TypeScriptは得意')
    await page.locator('[data-testid="meeting-must-topics-input"]').fill('コードレビューの質を上げる')

    await page.locator('[data-testid="meeting-save-button"]').click()

    // Then
    await expect(page.locator('[data-testid="meeting-saved-message"]')).toBeVisible({ timeout: 10000 })
    await expect(page.locator('[data-testid="meeting-saved-message"]')).toContainText('保存しました')
  })

  // AC-01-02: 必須項目バリデーション
  test('実施日を未入力で保存するとエラーが表示される', async ({ page }) => {
    // Given
    await goToCreatePage(page)

    // When: 実施日をJavaScriptでクリア（v-modelのリアクティビティを更新）
    await page.locator('[data-testid="meeting-conducted-at-input"]').evaluate(
      (el: HTMLInputElement) => {
        el.value = ''
        el.dispatchEvent(new Event('input', { bubbles: true }))
        el.dispatchEvent(new Event('change', { bubbles: true }))
      }
    )
    await page.locator('[data-testid="meeting-save-button"]').click()

    // Then: エラーメッセージが表示される
    await expect(page.locator('.field-error')).toBeVisible()
    await expect(page.locator('.field-error')).toContainText('実施日は必須')
    await expect(page.locator('[data-testid="meeting-saved-message"]')).not.toBeVisible()
  })
})

test.describe('SPEC-1ON1-001 REQ-02: 宿題管理', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsManager(page)
  })

  // AC-02-01: 宿題の登録
  test('宿題を入力して「宿題を追加」をクリックすると一覧に追加される', async ({ page }) => {
    // Given
    await goToCreatePage(page)

    // When
    await page.locator('[data-testid="meeting-action-item-input"]').fill('Playwright の学習をする')
    await page.locator('[data-testid="meeting-action-item-add-button"]').click()

    // Then
    await expect(page.getByText('Playwright の学習をする')).toBeVisible()

    // 2件目も追加できる
    await page.locator('[data-testid="meeting-action-item-input"]').fill('PR のセルフレビューを徹底する')
    await page.locator('[data-testid="meeting-action-item-add-button"]').click()
    await expect(page.getByText('PR のセルフレビューを徹底する')).toBeVisible()
  })

  test('Enterキーで宿題を追加できる', async ({ page }) => {
    // Given
    await goToCreatePage(page)

    // When
    await page.locator('[data-testid="meeting-action-item-input"]').fill('Enterで追加テスト')
    await page.locator('[data-testid="meeting-action-item-input"]').press('Enter')

    // Then
    await expect(page.getByText('Enterで追加テスト')).toBeVisible()
  })

  test('宿題の削除ボタンで宿題を削除できる', async ({ page }) => {
    // Given
    await goToCreatePage(page)
    await page.locator('[data-testid="meeting-action-item-input"]').fill('削除予定のタスク')
    await page.locator('[data-testid="meeting-action-item-add-button"]').click()
    await expect(page.getByText('削除予定のタスク')).toBeVisible()

    // When
    await page.locator('.action-item-remove').click()

    // Then
    await expect(page.getByText('削除予定のタスク')).not.toBeVisible()
  })
})

test.describe('SPEC-1ON1-001 REQ-05: 面談テンプレート', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsManager(page)
  })

  // AC-05-01: テンプレートを選択してプレースホルダーがセットされる
  test('「Will-Can-Must振り返り」テンプレートを選択するとプレースホルダーが変わる', async ({ page }) => {
    // Given
    await goToCreatePage(page)

    // テンプレートバーが表示されていることを確認
    await expect(page.locator('.template-bar')).toBeVisible({ timeout: 5000 })

    // デフォルトのプレースホルダーを確認
    const willTextarea = page.locator('[data-testid="meeting-will-topics-input"]')
    const defaultPlaceholder = await willTextarea.getAttribute('placeholder')
    expect(defaultPlaceholder).toContain('Will')

    // When: テンプレートを選択
    await page.locator('.template-btn', { hasText: 'Will-Can-Must振り返り' }).click()

    // Then: テンプレートが選択状態になる
    await expect(page.locator('.template-btn--active')).toContainText('Will-Can-Must振り返り')
  })

  test('テンプレートを選択してクリアすると元に戻る', async ({ page }) => {
    // Given
    await goToCreatePage(page)
    await expect(page.locator('.template-bar')).toBeVisible({ timeout: 5000 })

    // When: テンプレートを選択してからクリア
    await page.locator('.template-btn', { hasText: 'Will-Can-Must振り返り' }).click()
    await expect(page.locator('.template-btn--active')).toBeVisible()

    await page.locator('.template-btn--clear').click()

    // Then: 選択状態が解除される
    await expect(page.locator('.template-btn--active')).not.toBeVisible()
    await expect(page.locator('.template-btn--clear')).not.toBeVisible()
  })
})

test.describe('SPEC-1ON1-001 ナビゲーション', () => {
  test('ManagerがログインするとサイドバーにX1on1面談リンクが表示される', async ({ page }) => {
    // Given / When
    await loginAsManager(page)

    // Then
    await expect(page.locator('aside').getByRole('link', { name: '1on1面談' })).toBeVisible()
  })

  test('サイドバーの1on1面談リンクをクリックするとページに遷移する', async ({ page }) => {
    // Given
    await loginAsManager(page)

    // When
    await page.locator('aside').getByRole('link', { name: '1on1面談' }).click()

    // Then
    await expect(page).toHaveURL(/\/one-on-one/)
  })

  test('戻るリンクでone-on-one一覧に戻れる', async ({ page }) => {
    // Given
    await loginAsManager(page)
    await goToCreatePage(page)

    // When
    await page.getByRole('link', { name: '← 戻る' }).click()

    // Then
    await expect(page).toHaveURL(/\/one-on-one/)
  })
})
