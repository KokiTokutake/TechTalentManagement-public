import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { TalentProfileDto, UpsertManagerCommentRequest } from '~/types/talentProfile'

export const useTalentProfileStore = defineStore('talentProfile', () => {
  const profile = ref<TalentProfileDto | null>(null)
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)
  const selectedPeriodId = ref<string>('')

  const api = useTalentProfileApi()

  async function fetchProfile(talentId: string, periodId?: string) {
    loading.value = true
    error.value = null
    try {
      profile.value = await api.fetchProfile(talentId, periodId)
      selectedPeriodId.value = profile.value.periodId
    } catch {
      error.value = 'プロファイルの読み込みに失敗しました。'
    } finally {
      loading.value = false
    }
  }

  async function saveManagerComment(
    talentId: string,
    periodId: string,
    data: UpsertManagerCommentRequest,
  ) {
    saving.value = true
    error.value = null
    try {
      const updated = await api.upsertManagerComment(talentId, periodId, data)
      if (profile.value) {
        profile.value.managerComment = updated
      }
    } catch {
      error.value = '育成コメントの保存に失敗しました。'
      throw error.value
    } finally {
      saving.value = false
    }
  }

  async function toggleKeyPerson(talentId: string, isActive: boolean, reason?: string) {
    try {
      await api.toggleKeyPersonFlag(talentId, isActive, reason)
      if (profile.value) {
        profile.value.isKeyPerson = isActive
      }
    } catch {
      error.value = '重点管理フラグの変更に失敗しました。'
      throw error.value
    }
  }

  function $reset() {
    profile.value = null
    loading.value = false
    saving.value = false
    error.value = null
    selectedPeriodId.value = ''
  }

  return {
    profile, loading, saving, error, selectedPeriodId,
    fetchProfile, saveManagerComment, toggleKeyPerson, $reset,
  }
})
