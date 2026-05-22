import { test, expect } from '@playwright/test'

// SPEC-1ON1-001: 1on1面談記録機能
// 仕様書: docs/spec/SPEC-1ON1-001-one-on-one-meeting.md
// 対象タスク: TASK-050 [FE] 委員会連携フラグ・MDCサマリー反映 UI

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

test.describe('SPEC-1ON1-001 REQ-06: 委員会連携', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsManager(page)
  })

  // AC-06-01: 共有フラグの設定
  test('新規作成画面に「委員会に共有する」トグルが表示される', async ({ page }) => {
    // When
    await page.goto(
      `${BASE_URL}/one-on-one/create?talentId=${TALENT.id}&talentName=${encodeURIComponent(TALENT.displayName)}`,
    )

    // Then
    const toggle = page.locator('[data-testid="meeting-share-mdc-toggle"]')
    await expect(toggle).toBeVisible({ timeout: 5000 })
    expect(await toggle.isChecked()).toBe(false)
  })

  test('「委員会に共有する」トグルをオンにして保存すると shareWithMdc=true で記録される', async ({ page, request }) => {
    // Given
    await page.goto(
      `${BASE_URL}/one-on-one/create?talentId=${TALENT.id}&talentName=${encodeURIComponent(TALENT.displayName)}`,
    )
    await expect(page.locator('[data-testid="meeting-conducted-at-input"]')).toBeVisible({ timeout: 5000 })

    // When: 実施日を入力してトグルをオン
    await page.locator('[data-testid="meeting-conducted-at-input"]').fill('2026-06-01')
    await page.locator('[data-testid="meeting-share-mdc-toggle"]').check()
    await expect(page.locator('[data-testid="meeting-share-mdc-toggle"]')).toBeChecked()

    await page.locator('[data-testid="meeting-save-button"]').click()
    await expect(page.locator('[data-testid="meeting-saved-message"]')).toBeVisible({ timeout: 5000 })

    // Then: APIで確認 — shareWithMdc=true になっている
    const listRes = await request.get(
      `${API_BASE}/one-on-ones?talentId=${TALENT.id}&page=1&limit=1`,
      { headers: { 'X-User-Id': MANAGER.id } },
    )
    const latest = (await listRes.json()).items[0]
    expect(latest.shareWithMdc).toBe(true)
  })

  test('キャリアプロファイルページに面談サマリーセクションが表示される', async ({ page, request }) => {
    // Given: shareWithMdc=true の面談が存在する（上のテストで作成済み or シードデータ）
    // APIで確認してから画面に移動
    const summaryRes = await request.get(
      `${API_BASE}/talents/${TALENT.id}/one-on-one-summary`,
      { headers: { 'X-User-Id': MANAGER.id } },
    )
    const summary = await summaryRes.json()

    // When
    await page.goto(`${BASE_URL}/career/${TALENT.id}`)
    await expect(page.locator('[data-testid="meeting-summary-section"]')).toBeVisible({ timeout: 8000 })

    // Then: 最終面談日が表示される
    if (summary.lastMeetingAt) {
      const expectedDate = summary.lastMeetingAt.replace(/-/g, '/')
      await expect(page.locator('[data-testid="meeting-summary-last-date"]')).toContainText(expectedDate)
    }
  })

  test('shareWithMdc=true の面談はキャリアプロファイルの面談サマリーに表示される', async ({ page, request }) => {
    // Given: shareWithMdc=true の面談を作成
    const createRes = await request.post(`${API_BASE}/one-on-ones`, {
      headers: { 'X-User-Id': MANAGER.id, 'Content-Type': 'application/json' },
      data: {
        talentId: TALENT.id,
        conductedAt: '2026-06-10',
        format: 'Online',
        shareWithMdc: true,
        willTopics: 'MDCサマリーテスト用Will',
      },
    })
    const meeting = await createRes.json()

    // When
    await page.goto(`${BASE_URL}/career/${TALENT.id}`)
    await expect(page.locator('[data-testid="meeting-summary-section"]')).toBeVisible({ timeout: 8000 })

    // Then: 作成した面談が共有リストに表示される
    await expect(page.locator('[data-testid="meeting-summary-shared-list"]')).toBeVisible()
    await expect(page.locator(`[data-testid="meeting-summary-item-${meeting.id}"]`)).toBeVisible()
  })
})
