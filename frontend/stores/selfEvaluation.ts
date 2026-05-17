import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { SelfEvaluation } from '~/types/selfEvaluation'
import type { Goal } from '~/types/goal'

export interface EvalFormItem {
  goalId: string
  goalTitle: string
  goalCategory: string
  achievementScore: number // 0 = 未入力
  comment: string
}

export const useSelfEvaluationStore = defineStore('selfEvaluation', () => {
  const evaluation = ref<SelfEvaluation | null>(null)
  const formItems = ref<EvalFormItem[]>([])
  const loading = ref(false)
  const saving = ref(false)
  const saveStatus = ref<'idle' | 'saving' | 'saved' | 'error'>('idle')
  const error = ref<string | null>(null)
  const submitError = ref<string | null>(null)

  const isSubmitted = computed(() => evaluation.value?.isSubmitted ?? false)

  const api = useSelfEvaluationApi()

  let debounceTimer: ReturnType<typeof setTimeout> | null = null

  async function load(periodId: string, userId: string, goals: Goal[]) {
    loading.value = true
    error.value = null
    try {
      const ev = await api.fetchSelfEvaluation(periodId, userId)
      evaluation.value = ev

      // goals を元にフォーム状態を初期化、既存データがあれば反映
      formItems.value = goals.map(goal => {
        const existing = ev?.items.find(i => i.goalId === goal.id)
        return {
          goalId: goal.id,
          goalTitle: goal.title,
          goalCategory: goal.category,
          achievementScore: existing?.achievementScore ?? 0,
          comment: existing?.comment ?? '',
        }
      })
    } catch {
      error.value = '自己評価の読み込みに失敗しました。'
    } finally {
      loading.value = false
    }
  }

  function scheduleAutoSave(periodId: string, userId: string) {
    if (debounceTimer) clearTimeout(debounceTimer)
    saveStatus.value = 'idle'
    debounceTimer = setTimeout(() => {
      saveDraft(periodId, userId)
    }, 3000)
  }

  async function saveDraft(periodId: string, userId: string) {
    if (isSubmitted.value) return
    saving.value = true
    saveStatus.value = 'saving'
    try {
      const result = await api.saveDraft(periodId, userId, {
        items: formItems.value
          .filter(i => i.achievementScore > 0)
          .map(i => ({
            goalId: i.goalId,
            achievementScore: i.achievementScore,
            comment: i.comment.trim() || null,
          })),
      })
      evaluation.value = result
      saveStatus.value = 'saved'
    } catch {
      saveStatus.value = 'error'
    } finally {
      saving.value = false
    }
  }

  async function submitEvaluation(periodId: string, userId: string): Promise<boolean> {
    submitError.value = null
    const unscored = formItems.value.filter(i => i.achievementScore === 0)
    if (unscored.length > 0) {
      submitError.value = `${unscored.length} 件の目標に達成度が未入力です。`
      return false
    }

    // 提出前に最新下書きを保存
    await saveDraft(periodId, userId)

    try {
      const result = await api.submit(periodId, userId)
      evaluation.value = result
      return true
    } catch {
      submitError.value = '提出に失敗しました。'
      return false
    }
  }

  return {
    evaluation, formItems, loading, saving, saveStatus, error, submitError,
    isSubmitted, load, scheduleAutoSave, saveDraft, submitEvaluation,
  }
})
