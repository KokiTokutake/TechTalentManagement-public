import { defineStore } from 'pinia'
import type { CompetencyMaster, CompetencyCategory } from '~/types/skill'

export const useCompetencyMasterStore = defineStore('competencyMaster', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const competencies = ref<CompetencyMaster[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchCompetencies() {
    loading.value = true
    error.value = null
    try {
      const data = await $fetch<{ competencies: CompetencyMaster[] }>(`${base}/api/competency-masters`)
      competencies.value = data.competencies
    }
    catch {
      error.value = 'コンピテンシーマスタの取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  const byCategory = computed(() => {
    const map = new Map<CompetencyCategory, CompetencyMaster[]>()
    for (const c of competencies.value) {
      if (!map.has(c.category)) map.set(c.category, [])
      map.get(c.category)!.push(c)
    }
    return map
  })

  return { competencies, loading, error, byCategory, fetchCompetencies }
})
