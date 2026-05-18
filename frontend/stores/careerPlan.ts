import { defineStore } from 'pinia'
import type { CareerPlan, CareerPlanComment, IntegratedView, SaveCareerPlanRequest } from '~/types/careerPlan'

export const useCareerPlanStore = defineStore('careerPlan', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const integratedView = ref<IntegratedView | null>(null)
  const years = ref<number[]>([])
  const selectedPlan = ref<CareerPlan | null>(null)
  const comments = ref<CareerPlanComment[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchIntegratedView(userId: string, requesterId: string) {
    loading.value = true
    error.value = null
    try {
      integratedView.value = await $fetch<IntegratedView>(
        `${base}/api/users/${userId}/career-plans/integrated-view?requesterId=${requesterId}`,
      )
    }
    catch {
      error.value = '統合ビューの取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  async function fetchYears(userId: string, requesterId: string) {
    try {
      years.value = await $fetch<number[]>(
        `${base}/api/users/${userId}/career-plans/years?requesterId=${requesterId}`,
      )
    }
    catch {
      years.value = []
    }
  }

  async function fetchPlanByYear(userId: string, requesterId: string, year: number) {
    error.value = null
    try {
      selectedPlan.value = await $fetch<CareerPlan>(
        `${base}/api/users/${userId}/career-plans?requesterId=${requesterId}&year=${year}`,
      )
    }
    catch {
      selectedPlan.value = null
    }
  }

  async function createPlan(userId: string, requesterId: string, req: SaveCareerPlanRequest) {
    const data = await $fetch<CareerPlan>(
      `${base}/api/users/${userId}/career-plans?requesterId=${requesterId}`,
      { method: 'POST', body: req },
    )
    integratedView.value = integratedView.value
      ? { ...integratedView.value, will: data }
      : { will: data, can: [], must: [] }
    if (!years.value.includes(data.periodYear))
      years.value = [data.periodYear, ...years.value]
    return data
  }

  async function updatePlan(careerPlanId: string, requesterId: string, req: SaveCareerPlanRequest) {
    const data = await $fetch<CareerPlan>(
      `${base}/api/career-plans/${careerPlanId}?requesterId=${requesterId}`,
      { method: 'PUT', body: req },
    )
    if (integratedView.value)
      integratedView.value = { ...integratedView.value, will: data }
    if (selectedPlan.value?.id === careerPlanId)
      selectedPlan.value = data
    return data
  }

  async function fetchComments(careerPlanId: string, requesterId: string) {
    try {
      comments.value = await $fetch<CareerPlanComment[]>(
        `${base}/api/career-plans/${careerPlanId}/comments?requesterId=${requesterId}`,
      )
    }
    catch {
      comments.value = []
    }
  }

  async function addComment(careerPlanId: string, requesterId: string, content: string) {
    const data = await $fetch<CareerPlanComment>(
      `${base}/api/career-plans/${careerPlanId}/comments?requesterId=${requesterId}`,
      { method: 'POST', body: { content } },
    )
    comments.value.push(data)
    return data
  }

  function $reset() {
    integratedView.value = null
    years.value = []
    selectedPlan.value = null
    comments.value = []
    loading.value = false
    error.value = null
  }

  return {
    integratedView, years, selectedPlan, comments, loading, error,
    fetchIntegratedView, fetchYears, fetchPlanByYear,
    createPlan, updatePlan, fetchComments, addComment, $reset,
  }
})
