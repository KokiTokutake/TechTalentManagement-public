import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { CommitteeSession, CreateCommitteeSessionRequest } from '~/types/committeeSession'

export const useCommitteeSessionStore = defineStore('committeeSession', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const sessions = ref<CommitteeSession[]>([])
  const currentSession = ref<CommitteeSession | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchList(requesterId: string) {
    loading.value = true
    error.value = null
    try {
      sessions.value = await $fetch<CommitteeSession[]>(
        `${base}/api/committee-sessions?requesterId=${requesterId}`,
      )
    }
    catch {
      error.value = '委員会セッション一覧の取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  async function createSession(requesterId: string, req: CreateCommitteeSessionRequest) {
    const data = await $fetch<CommitteeSession>(
      `${base}/api/committee-sessions?requesterId=${requesterId}`,
      { method: 'POST', body: req },
    )
    sessions.value = [data, ...sessions.value]
    return data
  }

  async function fetchDetail(requesterId: string, sessionId: string) {
    loading.value = true
    error.value = null
    try {
      currentSession.value = await $fetch<CommitteeSession>(
        `${base}/api/committee-sessions/${sessionId}?requesterId=${requesterId}`,
      )
    }
    catch {
      error.value = 'セッション詳細の取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  function $reset() {
    sessions.value = []
    currentSession.value = null
    loading.value = false
    error.value = null
  }

  return { sessions, currentSession, loading, error, fetchList, createSession, fetchDetail, $reset }
})
