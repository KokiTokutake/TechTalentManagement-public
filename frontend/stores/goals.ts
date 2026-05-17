import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Goal, CreateGoalRequest, UpdateGoalRequest, WillCanMust } from '~/types/goal'

export const useGoalsStore = defineStore('goals', () => {
  const goals = ref<Goal[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const api = useGoalsApi()

  function byCategory(category: WillCanMust) {
    return computed(() =>
      goals.value
        .filter(g => g.category === category)
        .sort((a, b) => a.displayOrder - b.displayOrder)
    )
  }

  async function load(periodId: string, userId: string) {
    loading.value = true
    error.value = null
    try {
      goals.value = await api.fetchGoals(periodId, userId)
    } catch {
      error.value = '目標の読み込みに失敗しました。'
    } finally {
      loading.value = false
    }
  }

  async function add(periodId: string, userId: string, req: CreateGoalRequest) {
    const created = await api.createGoal(periodId, userId, req)
    goals.value.push(created)
  }

  async function update(periodId: string, userId: string, goalId: string, req: UpdateGoalRequest) {
    const updated = await api.updateGoal(periodId, userId, goalId, req)
    const idx = goals.value.findIndex(g => g.id === goalId)
    if (idx !== -1) goals.value[idx] = updated
  }

  async function remove(periodId: string, userId: string, goalId: string) {
    await api.deleteGoal(periodId, userId, goalId)
    goals.value = goals.value.filter(g => g.id !== goalId)
  }

  return { goals, loading, error, byCategory, load, add, update, remove }
})
