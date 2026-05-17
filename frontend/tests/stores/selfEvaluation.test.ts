import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { SelfEvaluation } from '~/types/selfEvaluation'
import type { Goal } from '~/types/goal'

const mockFetch = vi.fn()
const mockSaveDraft = vi.fn()
const mockSubmit = vi.fn()

vi.stubGlobal('useSelfEvaluationApi', () => ({
  fetchSelfEvaluation: mockFetch,
  saveDraft: mockSaveDraft,
  submit: mockSubmit,
}))

const makeGoal = (id: string, title: string): Goal => ({
  id,
  userId: 'user-1',
  evaluationPeriodId: 'period-1',
  category: 'Must',
  title,
  description: null,
  displayOrder: 1,
  createdAt: '2026-05-17T00:00:00Z',
  updatedAt: '2026-05-17T00:00:00Z',
})

const makeSelfEval = (isSubmitted = false): SelfEvaluation => ({
  id: 'eval-1',
  userId: 'user-1',
  evaluationPeriodId: 'period-1',
  isSubmitted,
  submittedAt: isSubmitted ? '2026-05-17T01:00:00Z' : null,
  createdAt: '2026-05-17T00:00:00Z',
  updatedAt: '2026-05-17T00:00:00Z',
  items: [
    {
      id: 'item-1',
      goalId: 'goal-1',
      achievementScore: 4,
      comment: '良い',
      updatedAt: '2026-05-17T00:00:00Z',
    },
  ],
})

describe('useSelfEvaluationStore', () => {
  beforeEach(async () => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.resetModules()
    vi.useFakeTimers()
  })

  it('load: 目標と既存評価を結合してformItemsを初期化する', async () => {
    const { useSelfEvaluationStore } = await import('~/stores/selfEvaluation')
    const store = useSelfEvaluationStore()
    const goals = [makeGoal('goal-1', '目標A'), makeGoal('goal-2', '目標B')]
    mockFetch.mockResolvedValue(makeSelfEval())

    await store.load('period-1', 'user-1', goals)

    expect(store.formItems).toHaveLength(2)
    expect(store.formItems[0].achievementScore).toBe(4) // 既存データ反映
    expect(store.formItems[0].comment).toBe('良い')
    expect(store.formItems[1].achievementScore).toBe(0) // 未入力
  })

  it('load: 自己評価が未作成(null)の場合でも全goal分のformItemsを生成する', async () => {
    const { useSelfEvaluationStore } = await import('~/stores/selfEvaluation')
    const store = useSelfEvaluationStore()
    mockFetch.mockResolvedValue(null)

    await store.load('period-1', 'user-1', [makeGoal('goal-1', '目標A')])

    expect(store.formItems).toHaveLength(1)
    expect(store.formItems[0].achievementScore).toBe(0)
    expect(store.isSubmitted).toBe(false)
  })

  it('scheduleAutoSave: 3秒後にsaveDraftを呼ぶ', async () => {
    const { useSelfEvaluationStore } = await import('~/stores/selfEvaluation')
    const store = useSelfEvaluationStore()
    mockFetch.mockResolvedValue(null)
    mockSaveDraft.mockResolvedValue(makeSelfEval())
    await store.load('period-1', 'user-1', [makeGoal('goal-1', '目標A')])

    store.scheduleAutoSave('period-1', 'user-1')
    expect(mockSaveDraft).not.toHaveBeenCalled()

    await vi.runAllTimersAsync()
    expect(mockSaveDraft).toHaveBeenCalledTimes(1)
  })

  it('scheduleAutoSave: 連続して呼ばれた場合は最後の1回だけ実行する', async () => {
    const { useSelfEvaluationStore } = await import('~/stores/selfEvaluation')
    const store = useSelfEvaluationStore()
    mockFetch.mockResolvedValue(null)
    mockSaveDraft.mockResolvedValue(makeSelfEval())
    await store.load('period-1', 'user-1', [makeGoal('goal-1', '目標A')])

    store.scheduleAutoSave('period-1', 'user-1')
    store.scheduleAutoSave('period-1', 'user-1')
    store.scheduleAutoSave('period-1', 'user-1')

    await vi.runAllTimersAsync()
    expect(mockSaveDraft).toHaveBeenCalledTimes(1)
  })

  it('submitEvaluation: 未スコアの項目があるとエラーを返す', async () => {
    const { useSelfEvaluationStore } = await import('~/stores/selfEvaluation')
    const store = useSelfEvaluationStore()
    mockFetch.mockResolvedValue(null)
    await store.load('period-1', 'user-1', [makeGoal('goal-1', '目標A')])
    // スコアは 0（未入力）のまま

    const result = await store.submitEvaluation('period-1', 'user-1')

    expect(result).toBe(false)
    expect(store.submitError).toBeTruthy()
    expect(mockSubmit).not.toHaveBeenCalled()
  })

  it('submitEvaluation: 全スコア入力済みなら提出してisSubmittedがtrueになる', async () => {
    const { useSelfEvaluationStore } = await import('~/stores/selfEvaluation')
    const store = useSelfEvaluationStore()
    mockFetch.mockResolvedValue(makeSelfEval()) // score=4 の既存評価あり
    mockSaveDraft.mockResolvedValue(makeSelfEval())
    mockSubmit.mockResolvedValue(makeSelfEval(true))
    await store.load('period-1', 'user-1', [makeGoal('goal-1', '目標A')])
    await vi.runAllTimersAsync()

    const result = await store.submitEvaluation('period-1', 'user-1')
    await vi.runAllTimersAsync()

    expect(result).toBe(true)
    expect(store.isSubmitted).toBe(true)
  })
})
