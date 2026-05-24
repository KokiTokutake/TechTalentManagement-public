import { defineStore } from 'pinia'
import type { CompetencyEvaluationsResponse } from '~/types/skill'

interface SaveItem {
  competencyId: string
  level: number
  comment: string | null
}

interface SavePayload {
  talentId: string
  periodId: string
  evaluatorType: 'Self' | 'Manager'
  evaluations: SaveItem[]
}

export const useCompetencyEvaluationStore = defineStore('competencyEvaluation', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const evaluationsData = ref<CompetencyEvaluationsResponse | null>(null)
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)

  async function fetchEvaluations(requesterId: string, talentId: string, periodId: string) {
    loading.value = true
    error.value = null
    try {
      evaluationsData.value = await $fetch<CompetencyEvaluationsResponse>(
        `${base}/api/competency-evaluations?requesterId=${requesterId}&talentId=${talentId}&periodId=${periodId}`,
      )
    }
    catch {
      error.value = 'コンピテンシー評価の取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  async function saveEvaluations(requesterId: string, payload: SavePayload): Promise<number> {
    saving.value = true
    error.value = null
    try {
      const res = await $fetch<{ savedCount: number }>(
        `${base}/api/competency-evaluations?requesterId=${requesterId}`,
        { method: 'POST', body: payload },
      )
      return res.savedCount
    }
    catch {
      error.value = 'コンピテンシー評価の保存に失敗しました。'
      throw new Error('save failed')
    }
    finally {
      saving.value = false
    }
  }

  return { evaluationsData, loading, saving, error, fetchEvaluations, saveEvaluations }
})
