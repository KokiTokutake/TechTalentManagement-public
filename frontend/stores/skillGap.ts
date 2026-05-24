import { defineStore } from 'pinia'
import type { SkillGapResult } from '~/types/skill'

export const useSkillGapStore = defineStore('skillGap', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const gapData = ref<SkillGapResult | null>(null)
  const threshold = ref(2)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchGap(requesterId: string, talentId: string, periodId: string, th = 2) {
    loading.value = true
    error.value = null
    threshold.value = th
    try {
      gapData.value = await $fetch<SkillGapResult>(
        `${base}/api/skills/gap?requesterId=${requesterId}&talentId=${talentId}&periodId=${periodId}&threshold=${th}`,
      )
    }
    catch {
      error.value = 'スキルギャップの取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  return { gapData, threshold, loading, error, fetchGap }
})
