import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { CareerValuesDto, SaveCareerValuesRequest } from '~/types/wcm'

export const useCareerValuesStore = defineStore('careerValues', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const values = ref<CareerValuesDto | null>(null)
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)

  async function fetchValues(talentId: string, periodId: string, requesterId: string) {
    loading.value = true
    error.value = null
    try {
      values.value = await $fetch<CareerValuesDto>(
        `${base}/api/career/values?talentId=${talentId}&periodId=${periodId}`,
        { headers: { 'X-User-Id': requesterId } },
      )
    }
    catch {
      error.value = '価値観データの取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  async function saveValues(req: SaveCareerValuesRequest, requesterId: string) {
    saving.value = true
    error.value = null
    try {
      values.value = await $fetch<CareerValuesDto>(`${base}/api/career/values`, {
        method: 'PUT',
        body: req,
        headers: { 'X-User-Id': requesterId },
      })
    }
    catch (e: unknown) {
      const err = e as { data?: { error?: string } }
      error.value = err?.data?.error ?? '価値観の保存に失敗しました。'
      throw e
    }
    finally {
      saving.value = false
    }
  }

  function $reset() {
    values.value = null
    loading.value = false
    saving.value = false
    error.value = null
  }

  return { values, loading, saving, error, fetchValues, saveValues, $reset }
})
