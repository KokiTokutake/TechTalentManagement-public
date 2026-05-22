import { defineStore } from 'pinia'
import type {
  MeetingListItem,
  MeetingDetail,
  CreateMeetingRequest,
  UpdateMeetingRequest,
  ActionItem,
  OneOnOneTemplate,
} from '~/types/oneOnOne'

export const useOneOnOneStore = defineStore('oneOnOne', () => {
  const config = useRuntimeConfig()
  const base = computed(() => config.public.apiBaseUrl)

  const meetings = ref<MeetingListItem[]>([])
  const currentMeeting = ref<MeetingDetail | null>(null)
  const previousMeeting = ref<MeetingDetail | null>(null)
  const templates = ref<OneOnOneTemplate[]>([])
  const totalCount = ref(0)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchMeetings(talentId: string, page = 1, limit = 10) {
    loading.value = true
    error.value = null
    try {
      const res = await $fetch<{ items: MeetingListItem[]; totalCount: number; page: number }>(
        `${base.value}/api/one-on-ones?talentId=${talentId}&page=${page}&limit=${limit}`,
      )
      meetings.value = res.items
      totalCount.value = res.totalCount
    } catch {
      error.value = '面談記録の取得に失敗しました。'
    } finally {
      loading.value = false
    }
  }

  async function fetchMeeting(id: string) {
    loading.value = true
    error.value = null
    try {
      currentMeeting.value = await $fetch<MeetingDetail>(`${base.value}/api/one-on-ones/${id}`)
    } catch {
      error.value = '面談記録の取得に失敗しました。'
      currentMeeting.value = null
    } finally {
      loading.value = false
    }
  }

  async function fetchPreviousMeeting(talentId: string) {
    try {
      previousMeeting.value = await $fetch<MeetingDetail | null>(
        `${base.value}/api/talents/${talentId}/previous-meeting`,
      )
    } catch {
      previousMeeting.value = null
    }
  }

  async function createMeeting(req: CreateMeetingRequest, managerId: string): Promise<MeetingDetail> {
    loading.value = true
    error.value = null
    try {
      const result = await $fetch<MeetingDetail>(`${base.value}/api/one-on-ones`, {
        method: 'POST',
        body: req,
        headers: { 'X-User-Id': managerId },
      })
      currentMeeting.value = result
      return result
    } catch {
      error.value = '面談記録の保存に失敗しました。'
      throw new Error('create failed')
    } finally {
      loading.value = false
    }
  }

  async function updateMeeting(id: string, req: UpdateMeetingRequest, managerId: string): Promise<MeetingDetail> {
    loading.value = true
    error.value = null
    try {
      const result = await $fetch<MeetingDetail>(`${base.value}/api/one-on-ones/${id}`, {
        method: 'PUT',
        body: req,
        headers: { 'X-User-Id': managerId },
      })
      currentMeeting.value = result
      return result
    } catch {
      error.value = '面談記録の更新に失敗しました。'
      throw new Error('update failed')
    } finally {
      loading.value = false
    }
  }

  async function addActionItem(meetingId: string, description: string, managerId: string): Promise<ActionItem> {
    try {
      const result = await $fetch<ActionItem>(`${base.value}/api/one-on-ones/${meetingId}/action-items`, {
        method: 'POST',
        body: { description },
        headers: { 'X-User-Id': managerId },
      })
      if (currentMeeting.value) {
        currentMeeting.value.actionItems.push(result)
      }
      return result
    } catch {
      error.value = '宿題の追加に失敗しました。'
      throw new Error('addActionItem failed')
    }
  }

  async function updateActionItemStatus(itemId: string, status: 'Pending' | 'Completed', managerId: string) {
    try {
      await $fetch(`${base.value}/api/action-items/${itemId}/status`, {
        method: 'PATCH',
        body: { status },
        headers: { 'X-User-Id': managerId },
      })
      if (currentMeeting.value) {
        const item = currentMeeting.value.actionItems.find(i => i.id === itemId)
        if (item) item.status = status
      }
    } catch {
      error.value = '宿題ステータスの更新に失敗しました。'
    }
  }

  async function loadTemplates() {
    if (templates.value.length > 0) return
    try {
      templates.value = await $fetch<OneOnOneTemplate[]>(`${base.value}/api/one-on-one-templates`)
    } catch {
      templates.value = []
    }
  }

  return {
    meetings,
    currentMeeting,
    previousMeeting,
    templates,
    totalCount,
    loading,
    error,
    fetchMeetings,
    fetchMeeting,
    fetchPreviousMeeting,
    createMeeting,
    updateMeeting,
    addActionItem,
    updateActionItemStatus,
    loadTemplates,
  }
})
