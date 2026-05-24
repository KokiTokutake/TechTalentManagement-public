import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { GoalKpiDto, GoalCheckinsResponse, SaveGoalKpiRequest, CreateCheckinRequest } from '~/types/goalKpi'

export const useGoalKpiStore = defineStore('goalKpi', () => {
  const kpiByGoalId = ref<Record<string, GoalKpiDto | null>>({})
  const checkinsByGoalId = ref<Record<string, GoalCheckinsResponse>>({})
  const loading = ref(false)
  const error = ref<string | null>(null)

  const api = useGoalKpiApi()

  async function fetchKpi(goalId: string) {
    loading.value = true
    error.value = null
    try {
      kpiByGoalId.value[goalId] = await api.fetchKpi(goalId)
    } catch {
      error.value = 'KPIの読み込みに失敗しました。'
    } finally {
      loading.value = false
    }
  }

  async function saveKpi(goalId: string, data: SaveGoalKpiRequest) {
    const existing = kpiByGoalId.value[goalId]
    const result = existing
      ? await api.updateKpi(goalId, data)
      : await api.createKpi(goalId, data)
    kpiByGoalId.value[goalId] = result
  }

  async function fetchCheckins(goalId: string) {
    loading.value = true
    error.value = null
    try {
      checkinsByGoalId.value[goalId] = await api.fetchCheckins(goalId)
    } catch {
      error.value = 'チェックイン履歴の読み込みに失敗しました。'
    } finally {
      loading.value = false
    }
  }

  async function addCheckin(goalId: string, data: CreateCheckinRequest) {
    await api.createCheckin(goalId, data)
    await fetchCheckins(goalId)
  }

  return { kpiByGoalId, checkinsByGoalId, loading, error, fetchKpi, saveKpi, fetchCheckins, addCheckin }
})
