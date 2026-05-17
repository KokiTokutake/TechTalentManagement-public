import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { ManagerEvaluation } from '~/types/managerEvaluation'
import type { SelfEvaluation } from '~/types/selfEvaluation'
import type { Goal } from '~/types/goal'

export const useManagerEvaluationStore = defineStore('managerEvaluation', () => {
  const managerEvaluation = ref<ManagerEvaluation | null>(null)
  const selfEvaluation = ref<SelfEvaluation | null>(null)
  const goals = ref<Goal[]>([])

  const overallScore = ref(0)
  const feedbackComment = ref('')

  const loading = ref(false)
  const saving = ref(false)
  const saveStatus = ref<'idle' | 'saving' | 'saved' | 'error'>('idle')
  const error = ref<string | null>(null)
  const confirmError = ref<string | null>(null)

  const isConfirmed = computed(() => managerEvaluation.value?.isConfirmed ?? false)

  const api = useManagerEvaluationApi()
  const selfEvalApi = useSelfEvaluationApi()
  const goalsApi = useGoalsApi()

  let debounceTimer: ReturnType<typeof setTimeout> | null = null

  async function load(selfEvaluationId: string, managerId: string, periodId: string, talentUserId: string) {
    loading.value = true
    error.value = null
    try {
      const [ev, me, gs] = await Promise.all([
        selfEvalApi.fetchSelfEvaluation(periodId, talentUserId),
        api.fetchManagerEvaluation(selfEvaluationId, managerId),
        goalsApi.fetchGoals(periodId, talentUserId),
      ])
      selfEvaluation.value = ev
      managerEvaluation.value = me
      goals.value = gs

      overallScore.value = me?.overallScore ?? 0
      feedbackComment.value = me?.feedbackComment ?? ''
    } catch {
      error.value = '上長評価の読み込みに失敗しました。'
    } finally {
      loading.value = false
    }
  }

  function scheduleAutoSave(selfEvaluationId: string, managerId: string) {
    if (debounceTimer) clearTimeout(debounceTimer)
    saveStatus.value = 'idle'
    debounceTimer = setTimeout(() => {
      saveDraft(selfEvaluationId, managerId)
    }, 3000)
  }

  async function saveDraft(selfEvaluationId: string, managerId: string) {
    if (isConfirmed.value) return
    saving.value = true
    saveStatus.value = 'saving'
    try {
      const result = await api.saveDraft(selfEvaluationId, managerId, {
        overallScore: overallScore.value,
        feedbackComment: feedbackComment.value.trim() || null,
      })
      managerEvaluation.value = result
      saveStatus.value = 'saved'
    } catch {
      saveStatus.value = 'error'
    } finally {
      saving.value = false
    }
  }

  async function confirmEvaluation(selfEvaluationId: string, managerId: string): Promise<boolean> {
    confirmError.value = null
    if (overallScore.value === 0) {
      confirmError.value = '総合評価スコアを入力してください。'
      return false
    }

    await saveDraft(selfEvaluationId, managerId)

    try {
      const result = await api.confirm(selfEvaluationId, managerId)
      managerEvaluation.value = result
      return true
    } catch {
      confirmError.value = '確定に失敗しました。'
      return false
    }
  }

  return {
    managerEvaluation, selfEvaluation, goals,
    overallScore, feedbackComment,
    loading, saving, saveStatus, error, confirmError,
    isConfirmed, load, scheduleAutoSave, saveDraft, confirmEvaluation,
  }
})
