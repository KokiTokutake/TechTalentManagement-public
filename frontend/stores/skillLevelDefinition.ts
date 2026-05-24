import { defineStore } from 'pinia'
import type { SkillLevelDefinitions } from '~/types/skill'

export const useSkillLevelDefinitionStore = defineStore('skillLevelDefinition', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const definitionsMap = ref<Map<string, SkillLevelDefinitions>>(new Map())
  const loading = ref(false)

  async function fetchDefinitions(skillItemId: string) {
    if (definitionsMap.value.has(skillItemId)) return
    loading.value = true
    try {
      const data = await $fetch<SkillLevelDefinitions>(
        `${base}/api/skill-masters/${skillItemId}/level-definitions`,
      )
      definitionsMap.value.set(skillItemId, data)
    }
    finally {
      loading.value = false
    }
  }

  function getDefinitions(skillItemId: string): SkillLevelDefinitions | null {
    return definitionsMap.value.get(skillItemId) ?? null
  }

  return { loading, fetchDefinitions, getDefinitions }
})
