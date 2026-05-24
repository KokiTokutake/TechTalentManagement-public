import { defineStore } from 'pinia'
import type { SkillLevelDefinitions } from '~/types/skill'

export const useSkillLevelDefinitionStore = defineStore('skillLevelDefinition', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const definitionsMap = ref<Map<string, SkillLevelDefinitions>>(new Map())
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchDefinitions(skillItemId: string, force = false) {
    if (!force && definitionsMap.value.has(skillItemId)) return
    loading.value = true
    error.value = null
    try {
      const data = await $fetch<SkillLevelDefinitions>(
        `${base}/api/skill-masters/${skillItemId}/level-definitions`,
      )
      definitionsMap.value.set(skillItemId, data)
    }
    catch {
      error.value = 'レベル定義の取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  function getDefinitions(skillItemId: string): SkillLevelDefinitions | null {
    return definitionsMap.value.get(skillItemId) ?? null
  }

  return { definitionsMap, loading, error, fetchDefinitions, getDefinitions }
})
