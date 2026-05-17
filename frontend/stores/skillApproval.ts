import { defineStore } from 'pinia'
import type { UserSkill } from '~/types/skill'

export const useSkillApprovalStore = defineStore('skillApproval', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const subordinateSkills = ref<{ user: { id: string, displayName: string }, skills: UserSkill[] }[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchPendingSkills(managerUserId: string, subordinateIds: string[]) {
    loading.value = true
    error.value = null
    try {
      const results = await Promise.all(
        subordinateIds.map(async (uid) => {
          const skills = await $fetch<UserSkill[]>(`${base}/api/users/${uid}/skills`)
          return { userId: uid, skills: skills.filter(s => s.status === 'Pending') }
        }),
      )

      const usersRes = await $fetch<{ id: string, displayName: string, managerId: string | null }[]>(`${base}/api/users`)
      const userMap = new Map(usersRes.map(u => [u.id, u]))

      subordinateSkills.value = results
        .filter(r => r.skills.length > 0)
        .map(r => ({
          user: { id: r.userId, displayName: userMap.get(r.userId)?.displayName ?? r.userId },
          skills: r.skills,
        }))
    }
    catch {
      error.value = '申請中スキルの取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  async function approveSkill(requesterId: string, userSkillId: string) {
    await $fetch(`${base}/api/user-skills/${userSkillId}/approve?requesterId=${requesterId}`, {
      method: 'POST',
    })
    removeSkill(userSkillId)
  }

  async function rejectSkill(requesterId: string, userSkillId: string, reason: string) {
    await $fetch(`${base}/api/user-skills/${userSkillId}/reject?requesterId=${requesterId}`, {
      method: 'POST',
      body: { reason },
    })
    removeSkill(userSkillId)
  }

  function removeSkill(userSkillId: string) {
    for (const group of subordinateSkills.value) {
      group.skills = group.skills.filter(s => s.id !== userSkillId)
    }
    subordinateSkills.value = subordinateSkills.value.filter(g => g.skills.length > 0)
  }

  return { subordinateSkills, loading, error, fetchPendingSkills, approveSkill, rejectSkill }
})
