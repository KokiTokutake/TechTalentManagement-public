import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { AppUser, EvaluationPeriod } from '~/types/user'

const USER_KEY = 'ttm_current_user'
const PERIOD_KEY = 'ttm_current_period'

export const useAuthStore = defineStore('auth', () => {
  const currentUser = ref<AppUser | null>(null)
  const currentPeriod = ref<EvaluationPeriod | null>(null)

  const isAuthenticated = computed(() => currentUser.value !== null)
  const isManager = computed(() =>
    currentUser.value?.role === 'Manager' ||
    currentUser.value?.role === 'Senior' ||
    currentUser.value?.role === 'Executive'
  )

  function init() {
    if (import.meta.client) {
      const savedUser = localStorage.getItem(USER_KEY)
      const savedPeriod = localStorage.getItem(PERIOD_KEY)
      if (savedUser) currentUser.value = JSON.parse(savedUser)
      if (savedPeriod) currentPeriod.value = JSON.parse(savedPeriod)
    }
  }

  function selectUser(user: AppUser) {
    currentUser.value = user
    if (import.meta.client) {
      localStorage.setItem(USER_KEY, JSON.stringify(user))
    }
  }

  function selectPeriod(period: EvaluationPeriod) {
    currentPeriod.value = period
    if (import.meta.client) {
      localStorage.setItem(PERIOD_KEY, JSON.stringify(period))
    }
  }

  function logout() {
    currentUser.value = null
    currentPeriod.value = null
    if (import.meta.client) {
      localStorage.removeItem(USER_KEY)
      localStorage.removeItem(PERIOD_KEY)
    }
  }

  return { currentUser, currentPeriod, isAuthenticated, isManager, init, selectUser, selectPeriod, logout }
})
