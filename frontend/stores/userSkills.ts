import { defineStore } from 'pinia'
import type { UserSkill } from '~/types/skill'

export const useUserSkillsStore = defineStore('userSkills', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const skills = ref<UserSkill[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchSkills(userId: string) {
    loading.value = true
    error.value = null
    try {
      skills.value = await $fetch<UserSkill[]>(`${base}/api/users/${userId}/skills`)
    }
    catch {
      error.value = 'スキルの取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  async function addSkill(requesterId: string, skillItemId: string, level: number, comment: string | null) {
    const data = await $fetch<UserSkill>(
      `${base}/api/users/${requesterId}/skills?requesterId=${requesterId}`,
      { method: 'POST', body: { skillItemId, level, comment } },
    )
    skills.value.push(data)
    return data
  }

  async function updateSkill(requesterId: string, userSkillId: string, level: number, comment: string | null) {
    const data = await $fetch<UserSkill>(
      `${base}/api/user-skills/${userSkillId}?requesterId=${requesterId}`,
      { method: 'PUT', body: { level, comment } },
    )
    const idx = skills.value.findIndex(s => s.id === userSkillId)
    if (idx !== -1) skills.value[idx] = data
    return data
  }

  async function deleteSkill(requesterId: string, userSkillId: string) {
    await $fetch(`${base}/api/user-skills/${userSkillId}?requesterId=${requesterId}`, {
      method: 'DELETE',
    })
    skills.value = skills.value.filter(s => s.id !== userSkillId)
  }

  const skillsByCategory = computed(() => {
    const map = new Map<string, { categoryName: string, skills: UserSkill[] }>()
    for (const skill of skills.value) {
      if (!map.has(skill.categoryId)) {
        map.set(skill.categoryId, { categoryName: skill.categoryName, skills: [] })
      }
      map.get(skill.categoryId)!.skills.push(skill)
    }
    return [...map.entries()].map(([id, val]) => ({ categoryId: id, ...val }))
  })

  return { skills, loading, error, skillsByCategory, fetchSkills, addSkill, updateSkill, deleteSkill }
})
