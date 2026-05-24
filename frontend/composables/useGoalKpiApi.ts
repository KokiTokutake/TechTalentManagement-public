import type {
  GoalKpiDto,
  GoalCheckinsResponse,
  SaveGoalKpiRequest,
  CreateCheckinRequest,
} from '~/types/goalKpi'

export function useGoalKpiApi() {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl
  const auth = useAuthStore()

  function headers() {
    return { 'X-User-Id': auth.currentUser?.id ?? '' }
  }

  async function fetchKpi(goalId: string): Promise<GoalKpiDto | null> {
    try {
      return await $fetch<GoalKpiDto>(`${base}/api/goals/${goalId}/kpi`, {
        headers: headers(),
      })
    } catch (e: any) {
      if (e?.status === 404) return null
      throw e
    }
  }

  async function createKpi(goalId: string, data: SaveGoalKpiRequest): Promise<GoalKpiDto> {
    return $fetch<GoalKpiDto>(`${base}/api/goals/${goalId}/kpi`, {
      method: 'POST',
      body: data,
      headers: headers(),
    })
  }

  async function updateKpi(goalId: string, data: SaveGoalKpiRequest): Promise<GoalKpiDto> {
    return $fetch<GoalKpiDto>(`${base}/api/goals/${goalId}/kpi`, {
      method: 'PUT',
      body: data,
      headers: headers(),
    })
  }

  async function fetchCheckins(goalId: string): Promise<GoalCheckinsResponse> {
    return $fetch<GoalCheckinsResponse>(`${base}/api/goals/${goalId}/checkins`, {
      headers: headers(),
    })
  }

  async function createCheckin(goalId: string, data: CreateCheckinRequest): Promise<any> {
    return $fetch(`${base}/api/goals/${goalId}/checkins`, {
      method: 'POST',
      body: data,
      headers: headers(),
    })
  }

  return { fetchKpi, createKpi, updateKpi, fetchCheckins, createCheckin }
}
