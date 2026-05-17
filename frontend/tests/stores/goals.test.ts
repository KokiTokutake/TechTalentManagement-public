import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { Goal } from '~/types/goal'

const mockFetchGoals = vi.fn()
const mockCreateGoal = vi.fn()
const mockUpdateGoal = vi.fn()
const mockDeleteGoal = vi.fn()

vi.mock('~/composables/useGoalsApi', () => ({
  useGoalsApi: () => ({
    fetchGoals: mockFetchGoals,
    createGoal: mockCreateGoal,
    updateGoal: mockUpdateGoal,
    deleteGoal: mockDeleteGoal,
  }),
}))

// Nuxt auto-imports のスタブ
vi.stubGlobal('useGoalsApi', () => ({
  fetchGoals: mockFetchGoals,
  createGoal: mockCreateGoal,
  updateGoal: mockUpdateGoal,
  deleteGoal: mockDeleteGoal,
}))

const makeGoal = (overrides: Partial<Goal> = {}): Goal => ({
  id: 'goal-1',
  userId: 'user-1',
  evaluationPeriodId: 'period-1',
  category: 'Will',
  title: 'テスト目標',
  description: null,
  displayOrder: 1,
  createdAt: '2026-05-17T00:00:00Z',
  updatedAt: '2026-05-17T00:00:00Z',
  ...overrides,
})

describe('useGoalsStore', () => {
  beforeEach(async () => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    // store を遅延インポートして毎回フレッシュな状態で
    vi.resetModules()
  })

  it('load: APIから目標一覧を取得してgoalsに格納する', async () => {
    const { useGoalsStore } = await import('~/stores/goals')
    const store = useGoalsStore()
    const goals = [makeGoal(), makeGoal({ id: 'goal-2', category: 'Must', title: 'Must目標' })]
    mockFetchGoals.mockResolvedValue(goals)

    await store.load('period-1', 'user-1')

    expect(store.goals).toEqual(goals)
    expect(store.loading).toBe(false)
    expect(store.error).toBeNull()
  })

  it('load: APIエラー時はerrorにメッセージを格納する', async () => {
    const { useGoalsStore } = await import('~/stores/goals')
    const store = useGoalsStore()
    mockFetchGoals.mockRejectedValue(new Error('network error'))

    await store.load('period-1', 'user-1')

    expect(store.goals).toEqual([])
    expect(store.error).toBeTruthy()
  })

  it('byCategory: カテゴリでフィルタリングして表示順にソートする', async () => {
    const { useGoalsStore } = await import('~/stores/goals')
    const store = useGoalsStore()
    const goals = [
      makeGoal({ id: 'w2', category: 'Will', displayOrder: 2 }),
      makeGoal({ id: 'm1', category: 'Must', displayOrder: 1 }),
      makeGoal({ id: 'w1', category: 'Will', displayOrder: 1 }),
    ]
    mockFetchGoals.mockResolvedValue(goals)
    await store.load('period-1', 'user-1')

    const willGoals = store.byCategory('Will').value
    expect(willGoals).toHaveLength(2)
    expect(willGoals[0].id).toBe('w1')
    expect(willGoals[1].id).toBe('w2')
  })

  it('add: 新しい目標を作成してgoalsに追加する', async () => {
    const { useGoalsStore } = await import('~/stores/goals')
    const store = useGoalsStore()
    mockFetchGoals.mockResolvedValue([])
    await store.load('period-1', 'user-1')

    const newGoal = makeGoal({ id: 'new-1', category: 'Can', title: '新しい目標' })
    mockCreateGoal.mockResolvedValue(newGoal)

    await store.add('period-1', 'user-1', {
      category: 'Can',
      title: '新しい目標',
      description: null,
      displayOrder: 1,
    })

    expect(store.goals).toContainEqual(newGoal)
  })

  it('update: 既存の目標を更新する', async () => {
    const { useGoalsStore } = await import('~/stores/goals')
    const store = useGoalsStore()
    const original = makeGoal()
    mockFetchGoals.mockResolvedValue([original])
    await store.load('period-1', 'user-1')

    const updated = makeGoal({ title: '更新後タイトル' })
    mockUpdateGoal.mockResolvedValue(updated)

    await store.update('period-1', 'user-1', 'goal-1', {
      title: '更新後タイトル',
      description: null,
      displayOrder: 1,
    })

    expect(store.goals[0].title).toBe('更新後タイトル')
  })

  it('remove: 目標を削除してgoalsから除去する', async () => {
    const { useGoalsStore } = await import('~/stores/goals')
    const store = useGoalsStore()
    mockFetchGoals.mockResolvedValue([makeGoal()])
    await store.load('period-1', 'user-1')
    mockDeleteGoal.mockResolvedValue(undefined)

    await store.remove('period-1', 'user-1', 'goal-1')

    expect(store.goals).toHaveLength(0)
  })
})
