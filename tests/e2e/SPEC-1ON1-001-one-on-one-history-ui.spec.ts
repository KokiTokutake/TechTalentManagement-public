import { test, expect } from '@playwright/test'

// SPEC-1ON1-001: 1on1面談記録機能
// 仕様書: docs/spec/SPEC-1ON1-001-one-on-one-meeting.md
// 対象タスク: TASK-049 [FE] 面談履歴一覧 UI＋前回引き継ぎサイドパネル

const BASE_URL = 'http://localhost:3000'
const API_BASE = 'http://localhost:5108/api'

const MANAGER = { id: '147469a5-e953-4afd-9ada-d67d9d271f13', displayName: '山田 一郎' }
const TALENT  = { id: '7e4acb26-9a8d-4773-a572-bdcffb663166', displayName: '佐藤 花子' }

async function loginAsManager(page: import('@playwright/test').Page) {
  await page.goto(`${BASE_URL}/select-user`)
  await expect(page.getByText(MANAGER.displayName)).toBeVisible({ timeout: 10000 })
  await page.getByText(MANAGER.displayName).click()
  await page.getByRole('button', { name: '開始する' }).click()
  await page.waitForURL(`${BASE_URL}/`)
}

test.describe('SPEC-1ON1-001 REQ-03: 面談履歴一覧', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsManager(page)
  })

  // AC-03-01: 一覧表示
  test('タレントの面談記録が一覧表示される（新しい順）', async ({ page, request }) => {
    // Given: 面談記録が存在する（既存データ使用）
    const listRes = await request.get(
      `${API_BASE}/one-on-ones?talentId=${TALENT.id}&page=1&limit=10`,
      { headers: { 'X-User-Id': MANAGER.id } },
    )
    const listData = await listRes.json()
    expect(listData.totalCount).toBeGreaterThan(0)

    // When: 面談履歴ページを開く
    await page.goto(`${BASE_URL}/one-on-one?talentId=${TALENT.id}&talentName=${encodeURIComponent(TALENT.displayName)}`)

    // Then: タレント名が表示されている
    await expect(page.locator('h1')).toContainText('1on1 面談記録')
    await expect(page.getByText(`${TALENT.displayName} の面談履歴`)).toBeVisible()

    // 面談記録が表示されている
    const rows = page.locator('[data-testid^="meeting-list-row-"]')
    await expect(rows.first()).toBeVisible({ timeout: 5000 })
    const count = await rows.count()
    expect(count).toBeGreaterThan(0)
  })

  test('面談一覧に実施日・形式・メモ抜粋・宿題数が表示される', async ({ page, request }) => {
    // Given: 最新の面談記録を取得
    const listRes = await request.get(
      `${API_BASE}/one-on-ones?talentId=${TALENT.id}&page=1&limit=1`,
      { headers: { 'X-User-Id': MANAGER.id } },
    )
    const listData = await listRes.json()
    const meeting = listData.items[0]

    // When
    await page.goto(`${BASE_URL}/one-on-one?talentId=${TALENT.id}&talentName=${encodeURIComponent(TALENT.displayName)}`)
    await expect(page.locator('[data-testid^="meeting-list-row-"]').first()).toBeVisible({ timeout: 5000 })

    // Then: 最新面談行に実施日が表示されている
    const firstRow = page.locator(`[data-testid="meeting-list-row-${meeting.id}"]`)
    await expect(firstRow).toBeVisible()
    await expect(firstRow).toContainText(meeting.conductedAt.replace(/-/g, '/'))
  })

  test('「新規面談を記録」ボタンが表示され、作成ページへ遷移する', async ({ page }) => {
    // Given / When
    await page.goto(`${BASE_URL}/one-on-one?talentId=${TALENT.id}&talentName=${encodeURIComponent(TALENT.displayName)}`)
    await expect(page.locator('[data-testid^="meeting-list-row-"]').first()).toBeVisible({ timeout: 5000 })

    // Then
    const newBtn = page.locator('[data-testid="meeting-new-button"]')
    await expect(newBtn).toBeVisible()

    await newBtn.click()
    await expect(page).toHaveURL(new RegExp(`/one-on-one/create.*talentId=${TALENT.id}`))
  })

  test('面談行の「詳細」リンクをクリックすると詳細ページに遷移する', async ({ page, request }) => {
    // Given: 最新面談IDを取得
    const listRes = await request.get(
      `${API_BASE}/one-on-ones?talentId=${TALENT.id}&page=1&limit=1`,
      { headers: { 'X-User-Id': MANAGER.id } },
    )
    const meeting = (await listRes.json()).items[0]

    // When
    await page.goto(`${BASE_URL}/one-on-one?talentId=${TALENT.id}&talentName=${encodeURIComponent(TALENT.displayName)}`)
    await expect(page.locator(`[data-testid="meeting-list-row-${meeting.id}"]`)).toBeVisible({ timeout: 5000 })

    await page.locator(`[data-testid="meeting-list-row-${meeting.id}"]`).getByRole('link', { name: '詳細' }).click()

    // Then
    await expect(page).toHaveURL(`${BASE_URL}/one-on-one/${meeting.id}`)
    await expect(page.locator('h1')).toContainText('面談記録')
  })
})

test.describe('SPEC-1ON1-001 面談記録詳細', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsManager(page)
  })

  test('面談記録の詳細情報が表示される', async ({ page, request }) => {
    // Given: 詳細を取得
    const listRes = await request.get(
      `${API_BASE}/one-on-ones?talentId=${TALENT.id}&page=1&limit=1`,
      { headers: { 'X-User-Id': MANAGER.id } },
    )
    const meeting = (await listRes.json()).items[0]

    // When
    await page.goto(`${BASE_URL}/one-on-one/${meeting.id}`)

    // Then
    await expect(page.locator('h1')).toContainText('面談記録')
    await expect(page.getByText(TALENT.displayName)).toBeVisible()
    // .meta-value.mono に絞って strict mode violation を回避
    await expect(page.locator('.meta-value.mono')).toContainText(meeting.conductedAt.replace(/-/g, '/'))
  })

  test('宿題のチェックボックスでステータスを切り替えられる', async ({ page, request }) => {
    // Given: 宿題付き面談を作成
    const createRes = await request.post(`${API_BASE}/one-on-ones`, {
      headers: { 'X-User-Id': MANAGER.id, 'Content-Type': 'application/json' },
      data: {
        talentId: TALENT.id,
        conductedAt: '2026-01-10',
        format: 'Online',
        shareWithMdc: false,
      },
    })
    const meeting = await createRes.json()
    const itemRes = await request.post(`${API_BASE}/one-on-ones/${meeting.id}/action-items`, {
      headers: { 'X-User-Id': MANAGER.id, 'Content-Type': 'application/json' },
      data: { description: 'E2Eテスト用宿題' },
    })
    const actionItem = await itemRes.json()

    // When: 詳細ページを開く
    await page.goto(`${BASE_URL}/one-on-one/${meeting.id}`)
    const checkbox = page.locator(`[data-testid="meeting-action-item-check-${actionItem.id}"]`)
    await expect(checkbox).toBeVisible({ timeout: 5000 })
    expect(await checkbox.isChecked()).toBe(false)

    // チェックを入れる
    await checkbox.click()

    // Then: チェック済みになる
    await expect(checkbox).toBeChecked({ timeout: 5000 })
  })
})

test.describe('SPEC-1ON1-001 REQ-04: 前回記録サイドパネル', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsManager(page)
  })

  // AC-04-01: 前回記録のサイドパネル表示
  test('過去の面談記録があると新規作成画面にサイドパネルが表示される', async ({ page }) => {
    // Given: 佐藤 花子には既存の面談記録がある（シードデータ）
    // When: 新規作成画面を開く
    await page.goto(
      `${BASE_URL}/one-on-one/create?talentId=${TALENT.id}&talentName=${encodeURIComponent(TALENT.displayName)}`,
    )

    // Then: サイドパネルが表示される
    await expect(page.locator('[data-testid="previous-meeting-panel"]')).toBeVisible({ timeout: 8000 })
    await expect(page.locator('[data-testid="previous-meeting-date"]')).toBeVisible()
  })

  test('サイドパネルに前回の実施日が表示される', async ({ page, request }) => {
    // Given: 前回面談を取得
    const prevRes = await request.get(
      `${API_BASE}/talents/${TALENT.id}/previous-meeting`,
      { headers: { 'X-User-Id': MANAGER.id } },
    )
    const prev = await prevRes.json()
    const expectedDate = prev.conductedAt.replace(/-/g, '/')

    // When
    await page.goto(
      `${BASE_URL}/one-on-one/create?talentId=${TALENT.id}&talentName=${encodeURIComponent(TALENT.displayName)}`,
    )
    await expect(page.locator('[data-testid="previous-meeting-panel"]')).toBeVisible({ timeout: 8000 })

    // Then: 実施日が表示されている
    await expect(page.locator('[data-testid="previous-meeting-date"]')).toContainText(expectedDate)
  })

  test('前回の未完了宿題があるとサイドパネルに表示される', async ({ page, request }) => {
    // Given: 最新面談（= previous-meeting APIが返す面談）に宿題を追加する
    const prevRes = await request.get(
      `${API_BASE}/talents/${TALENT.id}/previous-meeting`,
      { headers: { 'X-User-Id': MANAGER.id } },
    )
    const prevMeeting = await prevRes.json()
    await request.post(`${API_BASE}/one-on-ones/${prevMeeting.id}/action-items`, {
      headers: { 'X-User-Id': MANAGER.id, 'Content-Type': 'application/json' },
      data: { description: 'サイドパネル表示テスト用宿題' },
    })

    // When: 新規作成画面を開く
    await page.goto(
      `${BASE_URL}/one-on-one/create?talentId=${TALENT.id}&talentName=${encodeURIComponent(TALENT.displayName)}`,
    )
    await expect(page.locator('[data-testid="previous-meeting-panel"]')).toBeVisible({ timeout: 8000 })

    // Then: 未完了宿題セクションが表示される
    await expect(page.locator('[data-testid="previous-meeting-pending-items"]')).toBeVisible()
  })
})

test.describe('SPEC-1ON1-001 ナビゲーション（履歴）', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsManager(page)
  })

  test('サイドバーの1on1面談リンクからタレント選択画面に遷移する', async ({ page }) => {
    // Given / When
    await page.locator('aside').getByRole('link', { name: '1on1面談' }).click()

    // Then
    await expect(page).toHaveURL(`${BASE_URL}/one-on-one`)
    await expect(page.locator('h1')).toContainText('1on1 面談記録')
  })

  test('タレント選択画面からタレントをクリックすると履歴一覧に遷移する', async ({ page }) => {
    // Given
    await page.goto(`${BASE_URL}/one-on-one`)
    await expect(page.locator('h1')).toContainText('1on1 面談記録')

    // タレントリンクが表示されるまで待機
    await expect(page.locator(`[data-testid="one-on-one-talent-link-${TALENT.id}"]`)).toBeVisible({ timeout: 8000 })

    // When
    await page.locator(`[data-testid="one-on-one-talent-link-${TALENT.id}"]`).click()

    // Then
    await expect(page).toHaveURL(new RegExp(`talentId=${TALENT.id}`))
    await expect(page.getByText(`${TALENT.displayName} の面談履歴`)).toBeVisible()
  })
})
