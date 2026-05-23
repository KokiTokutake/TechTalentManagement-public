import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { WcmDto, SaveWcmRequest } from '~/types/wcm'

export const useWcmStore = defineStore('wcm', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const wcm = ref<WcmDto | null>(null)
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)

  async function fetchWcm(talentId: string, periodId: string, requesterId: string) {
    loading.value = true
    error.value = null
    try {
      wcm.value = await $fetch<WcmDto>(
        `${base}/api/career/wcm?talentId=${talentId}&periodId=${periodId}`,
        { headers: { 'X-User-Id': requesterId } },
      )
    }
    catch {
      error.value = 'WCMデータの取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  async function saveWcm(req: SaveWcmRequest, requesterId: string) {
    saving.value = true
    error.value = null
    try {
      wcm.value = await $fetch<WcmDto>(`${base}/api/career/wcm`, {
        method: 'PUT',
        body: req,
        headers: { 'X-User-Id': requesterId },
      })
    }
    catch (e: unknown) {
      const err = e as { data?: { error?: string } }
      error.value = err?.data?.error ?? 'WCMの保存に失敗しました。'
      throw e
    }
    finally {
      saving.value = false
    }
  }

  function $reset() {
    wcm.value = null
    loading.value = false
    saving.value = false
    error.value = null
  }

  return { wcm, loading, saving, error, fetchWcm, saveWcm, $reset }
})
