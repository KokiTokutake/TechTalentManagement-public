import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { CareerPlan, IntegratedView, CareerPlanComment, OrgOverviewItem } from '~/types/careerPlan'

const mockFetch = vi.fn()
vi.stubGlobal('$fetch', mockFetch)
vi.stubGlobal('useRuntimeConfig', () => ({ public: { apiBaseUrl: 'http://localhost:5000' } }))

const makePlan = (overrides: Partial<CareerPlan> = {}): CareerPlan => ({
  id: 'plan-1',
  userId: 'user-1',
  periodYear: 2026,
  longTermGoal: '10年後にテックリードになる',
  mediumTermGoal: '3年でシステム設計力を高める',
  currentFocus: '設計スキル向上',
  createdAt: '2026-05-17T00:00:00Z',
  updatedAt: '2026-05-17T00:00:00Z',
  ...overrides,
})

const makeView = (overrides: Partial<IntegratedView> = {}): IntegratedView => ({
  will: makePlan(),
  can: [{ id: 's-1', skillItemId: 'si-1', skillItemName: 'C#', categoryName: '言語', level: 4 }],
  must: [{ id: 'g-1', category: 'Must', title: '今期目標', description: null, displayOrder: 1 }],
  ...overrides,
})

describe('useCareerPlanStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.resetModules()
  })

  it('fetchIntegratedView: APIから統合ビューを取得してintegratedViewに格納する', async () => {
    const { useCareerPlanStore } = await import('~/stores/careerPlan')
    const store = useCareerPlanStore()
    const view = makeView()
    mockFetch.mockResolvedValue(view)

    await store.fetchIntegratedView('user-1', 'user-1')

    expect(store.integratedView).toEqual(view)
    expect(store.loading).toBe(false)
    expect(store.error).toBeNull()
  })

  it('fetchIntegratedView: APIエラー時はerrorにメッセージを格納する', async () => {
    const { useCareerPlanStore } = await import('~/stores/careerPlan')
    const store = useCareerPlanStore()
    mockFetch.mockRejectedValue(new Error('network error'))

    await store.fetchIntegratedView('user-1', 'user-1')

    expect(store.integratedView).toBeNull()
    expect(store.error).toBeTruthy()
  })

  it('createPlan: プランを作成してintegratedViewを更新する', async () => {
    const { useCareerPlanStore } = await import('~/stores/careerPlan')
    const store = useCareerPlanStore()
    const newPlan = makePlan()
    mockFetch.mockResolvedValue(newPlan)

    const result = await store.createPlan('user-1', 'user-1', {
      longTermGoal: '10年後にテックリードになる',
      mediumTermGoal: '3年でシステム設計力を高める',
      currentFocus: '設計スキル向上',
    })

    expect(result).toEqual(newPlan)
    expect(store.integratedView?.will).toEqual(newPlan)
    expect(store.years).toContain(newPlan.periodYear)
  })

  it('updatePlan: プランを更新してintegratedViewを反映する', async () => {
    const { useCareerPlanStore } = await import('~/stores/careerPlan')
    const store = useCareerPlanStore()
    store.integratedView = makeView()
    const updatedPlan = makePlan({ longTermGoal: '更新後の目標', currentFocus: '更新後注力' })
    mockFetch.mockResolvedValue(updatedPlan)

    const result = await store.updatePlan('plan-1', 'user-1', {
      longTermGoal: '更新後の目標',
      mediumTermGoal: '3年でシステム設計力を高める',
      currentFocus: '更新後注力',
    })

    expect(result.longTermGoal).toBe('更新後の目標')
    expect(store.integratedView?.will?.longTermGoal).toBe('更新後の目標')
  })

  it('addComment: コメントを追加してcommentsに追記する', async () => {
    const { useCareerPlanStore } = await import('~/stores/careerPlan')
    const store = useCareerPlanStore()
    const comment: CareerPlanComment = {
      id: 'c-1',
      careerPlanId: 'plan-1',
      authorId: 'manager-1',
      authorName: '上長花子',
      content: 'よく書けています',
      createdAt: '2026-05-18T00:00:00Z',
    }
    mockFetch.mockResolvedValue(comment)

    await store.addComment('plan-1', 'manager-1', 'よく書けています')

    expect(store.comments).toHaveLength(1)
    expect(store.comments[0].content).toBe('よく書けています')
  })

  it('fetchOrgOverview: APIから組織俯瞰データを取得してorgOverviewに格納する', async () => {
    const { useCareerPlanStore } = await import('~/stores/careerPlan')
    const store = useCareerPlanStore()
    const items: OrgOverviewItem[] = [
      { userId: 'u-1', userName: 'タレント太郎', longTermGoal: '長期目標', currentFocus: '注力', periodYear: 2026 },
      { userId: 'u-2', userName: '他のタレント', longTermGoal: null, currentFocus: null, periodYear: null },
    ]
    mockFetch.mockResolvedValue(items)

    await store.fetchOrgOverview('senior-1')

    expect(store.orgOverview).toEqual(items)
    expect(store.loading).toBe(false)
    expect(store.error).toBeNull()
  })

  it('fetchOrgOverview: APIエラー時はerrorにメッセージを格納する', async () => {
    const { useCareerPlanStore } = await import('~/stores/careerPlan')
    const store = useCareerPlanStore()
    mockFetch.mockRejectedValue(new Error('forbidden'))

    await store.fetchOrgOverview('talent-1')

    expect(store.orgOverview).toEqual([])
    expect(store.error).toBeTruthy()
  })

  it('$reset: 全ての状態をリセットする', async () => {
    const { useCareerPlanStore } = await import('~/stores/careerPlan')
    const store = useCareerPlanStore()
    store.integratedView = makeView()
    store.years = [2026, 2025]
    store.orgOverview = [{ userId: 'u-1', userName: 'タレント', longTermGoal: null, currentFocus: null, periodYear: null }]

    store.$reset()

    expect(store.integratedView).toBeNull()
    expect(store.years).toEqual([])
    expect(store.orgOverview).toEqual([])
    expect(store.error).toBeNull()
  })
})
