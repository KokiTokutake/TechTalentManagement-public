import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { DevelopmentPlan, SaveDevelopmentPlanRequest } from '~/types/developmentPlan'

export const useDevelopmentPlanStore = defineStore('developmentPlan', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const plans = ref<DevelopmentPlan[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchList(requesterId: string, keyPersonId: string) {
    loading.value = true
    error.value = null
    try {
      plans.value = await $fetch<DevelopmentPlan[]>(
        `${base}/api/key-persons/${keyPersonId}/development-plans?requesterId=${requesterId}`,
      )
    }
    catch {
      error.value = '育成方針の取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  async function addPlan(requesterId: string, keyPersonId: string, req: SaveDevelopmentPlanRequest) {
    const data = await $fetch<DevelopmentPlan>(
      `${base}/api/key-persons/${keyPersonId}/development-plans?requesterId=${requesterId}`,
      { method: 'POST', body: req },
    )
    plans.value = [data, ...plans.value]
    return data
  }

  function $reset() {
    plans.value = []
    loading.value = false
    error.value = null
  }

  return { plans, loading, error, fetchList, addPlan, $reset }
})
