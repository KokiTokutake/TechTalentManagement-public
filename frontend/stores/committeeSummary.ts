import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { CommitteeSummaryItem } from '~/types/committeeSummary'

export const useCommitteeSummaryStore = defineStore('committeeSummary', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const items = ref<CommitteeSummaryItem[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchSummary(requesterId: string) {
    loading.value = true
    error.value = null
    try {
      items.value = await $fetch<CommitteeSummaryItem[]>(
        `${base}/api/key-persons/committee-summary?requesterId=${requesterId}`,
      )
    }
    catch {
      error.value = '委員会サマリーの取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  function $reset() {
    items.value = []
    loading.value = false
    error.value = null
  }

  return { items, loading, error, fetchSummary, $reset }
})
