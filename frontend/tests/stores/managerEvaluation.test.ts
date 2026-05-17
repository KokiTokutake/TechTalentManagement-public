import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { ManagerEvaluation } from '~/types/managerEvaluation'
import type { SelfEvaluation } from '~/types/selfEvaluation'
import type { Goal } from '~/types/goal'

const mockFetchManager = vi.fn()
const mockSaveDraft = vi.fn()
const mockConfirm = vi.fn()
const mockFetchSelf = vi.fn()
const mockFetchGoals = vi.fn()

vi.stubGlobal('useManagerEvaluationApi', () => ({
  fetchManagerEvaluation: mockFetchManager,
  saveDraft: mockSaveDraft,
  confirm: mockConfirm,
}))
vi.stubGlobal('useSelfEvaluationApi', () => ({
  fetchSelfEvaluation: mockFetchSelf,
  saveDraft: vi.fn(),
  submit: vi.fn(),
}))
vi.stubGlobal('useGoalsApi', () => ({
  fetchGoals: mockFetchGoals,
  createGoal: vi.fn(),
  updateGoal: vi.fn(),
  deleteGoal: vi.fn(),
}))

const makeGoal = (id: string): Goal => ({
  id,
  userId: 'talent-1',
  evaluationPeriodId: 'period-1',
  category: 'Must',
  title: `目標 ${id}`,
  description: null,
  displayOrder: 1,
  createdAt: '2026-05-17T00:00:00Z',
  updatedAt: '2026-05-17T00:00:00Z',
})

const makeSelfEval = (isSubmitted = true): SelfEvaluation => ({
  id: 'self-eval-1',
  userId: 'talent-1',
  evaluationPeriodId: 'period-1',
  isSubmitted,
  submittedAt: isSubmitted ? '2026-05-17T01:00:00Z' : null,
  createdAt: '2026-05-17T00:00:00Z',
  updatedAt: '2026-05-17T00:00:00Z',
  items: [{ id: 'item-1', goalId: 'goal-1', achievementScore: 4, comment: '良い', updatedAt: '2026-05-17T00:00:00Z' }],
})

const makeManagerEval = (isConfirmed = false): ManagerEvaluation => ({
  id: 'mgr-eval-1',
  selfEvaluationId: 'self-eval-1',
  managerId: 'manager-1',
  overallScore: isConfirmed ? 4 : 0,
  feedbackComment: isConfirmed ? 'よく頑張りました' : null,
  isConfirmed,
  confirmedAt: isConfirmed ? '2026-05-17T02:00:00Z' : null,
  createdAt: '2026-05-17T00:00:00Z',
  updatedAt: '2026-05-17T00:00:00Z',
})

describe('useManagerEvaluationStore', () => {
  beforeEach(async () => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.resetModules()
    vi.useFakeTimers()
  })

  it('load: 自己評価・上長評価・目標を並行取得して状態を初期化する', async () => {
    const { useManagerEvaluationStore } = await import('~/stores/managerEvaluation')
    const store = useManagerEvaluationStore()
    mockFetchSelf.mockResolvedValue(makeSelfEval())
    mockFetchManager.mockResolvedValue(makeManagerEval())
    mockFetchGoals.mockResolvedValue([makeGoal('goal-1')])

    await store.load('self-eval-1', 'manager-1', 'period-1', 'talent-1')

    expect(store.selfEvaluation?.isSubmitted).toBe(true)
    expect(store.goals).toHaveLength(1)
    expect(store.isConfirmed).toBe(false)
  })

  it('load: 上長評価が未作成の場合はoverallScore=0で初期化する', async () => {
    const { useManagerEvaluationStore } = await import('~/stores/managerEvaluation')
    const store = useManagerEvaluationStore()
    mockFetchSelf.mockResolvedValue(makeSelfEval())
    mockFetchManager.mockResolvedValue(null)
    mockFetchGoals.mockResolvedValue([])

    await store.load('self-eval-1', 'manager-1', 'period-1', 'talent-1')

    expect(store.overallScore).toBe(0)
    expect(store.feedbackComment).toBe('')
  })

  it('scheduleAutoSave: 3秒後にsaveDraftを呼ぶ', async () => {
    const { useManagerEvaluationStore } = await import('~/stores/managerEvaluation')
    const store = useManagerEvaluationStore()
    mockFetchSelf.mockResolvedValue(makeSelfEval())
    mockFetchManager.mockResolvedValue(null)
    mockFetchGoals.mockResolvedValue([])
    mockSaveDraft.mockResolvedValue(makeManagerEval())
    await store.load('self-eval-1', 'manager-1', 'period-1', 'talent-1')

    store.overallScore = 3
    store.scheduleAutoSave('self-eval-1', 'manager-1')
    expect(mockSaveDraft).not.toHaveBeenCalled()

    await vi.runAllTimersAsync()
    expect(mockSaveDraft).toHaveBeenCalledTimes(1)
  })

  it('confirmEvaluation: スコア未入力の場合はエラーを返す', async () => {
    const { useManagerEvaluationStore } = await import('~/stores/managerEvaluation')
    const store = useManagerEvaluationStore()
    mockFetchSelf.mockResolvedValue(makeSelfEval())
    mockFetchManager.mockResolvedValue(null)
    mockFetchGoals.mockResolvedValue([])
    await store.load('self-eval-1', 'manager-1', 'period-1', 'talent-1')
    // overallScore = 0のまま

    const result = await store.confirmEvaluation('self-eval-1', 'manager-1')

    expect(result).toBe(false)
    expect(store.confirmError).toBeTruthy()
    expect(mockConfirm).not.toHaveBeenCalled()
  })

  it('confirmEvaluation: スコア入力済みなら確定してisConfirmedがtrueになる', async () => {
    const { useManagerEvaluationStore } = await import('~/stores/managerEvaluation')
    const store = useManagerEvaluationStore()
    mockFetchSelf.mockResolvedValue(makeSelfEval())
    mockFetchManager.mockResolvedValue(makeManagerEval())
    mockFetchGoals.mockResolvedValue([])
    mockSaveDraft.mockResolvedValue(makeManagerEval())
    mockConfirm.mockResolvedValue(makeManagerEval(true))
    await store.load('self-eval-1', 'manager-1', 'period-1', 'talent-1')
    store.overallScore = 4

    const result = await store.confirmEvaluation('self-eval-1', 'manager-1')
    await vi.runAllTimersAsync()

    expect(result).toBe(true)
    expect(store.isConfirmed).toBe(true)
  })
})
