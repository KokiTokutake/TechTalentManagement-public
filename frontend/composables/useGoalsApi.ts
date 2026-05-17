import type { Goal, CreateGoalRequest, UpdateGoalRequest } from '~/types/goal'

export function useGoalsApi() {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  function baseUrl(periodId: string, userId: string) {
    return `${base}/api/periods/${periodId}/users/${userId}/goals`
  }

  async function fetchGoals(periodId: string, userId: string): Promise<Goal[]> {
    const res = await $fetch<Goal[]>(baseUrl(periodId, userId))
    return res
  }

  async function createGoal(periodId: string, userId: string, req: CreateGoalRequest): Promise<Goal> {
    return await $fetch<Goal>(baseUrl(periodId, userId), {
      method: 'POST',
      body: req,
    })
  }

  async function updateGoal(periodId: string, userId: string, goalId: string, req: UpdateGoalRequest): Promise<Goal> {
    return await $fetch<Goal>(`${baseUrl(periodId, userId)}/${goalId}`, {
      method: 'PUT',
      body: req,
    })
  }

  async function deleteGoal(periodId: string, userId: string, goalId: string): Promise<void> {
    await $fetch(`${baseUrl(periodId, userId)}/${goalId}`, { method: 'DELETE' })
  }

  return { fetchGoals, createGoal, updateGoal, deleteGoal }
}
