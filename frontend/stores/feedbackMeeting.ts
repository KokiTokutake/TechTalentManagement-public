import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { FeedbackMeetingDto, CreateFeedbackMeetingRequest } from '~/types/goalKpi'

export const useFeedbackMeetingStore = defineStore('feedbackMeeting', () => {
  const meetings = ref<Record<string, FeedbackMeetingDto | null>>({})
  const loading = ref(false)
  const error = ref<string | null>(null)

  const api = useFeedbackMeetingApi()

  function key(talentId: string, periodId: string) {
    return `${talentId}_${periodId}`
  }

  async function fetchMeeting(talentId: string, periodId: string) {
    loading.value = true
    error.value = null
    try {
      meetings.value[key(talentId, periodId)] = await api.fetchMeeting(talentId, periodId)
    } catch {
      error.value = '面談記録の読み込みに失敗しました。'
    } finally {
      loading.value = false
    }
  }

  async function createMeeting(data: CreateFeedbackMeetingRequest) {
    const result = await api.createMeeting(data)
    meetings.value[key(data.talentId, '')] = result
    return result
  }

  async function disclose(meetingId: string, talentId: string, periodId: string) {
    const result = await api.disclose(meetingId)
    const k = key(talentId, periodId)
    if (meetings.value[k]) {
      meetings.value[k] = {
        ...meetings.value[k]!,
        isDisclosed: result.isDisclosed,
        disclosedAt: result.disclosedAt,
      }
    }
    return result
  }

  function getMeeting(talentId: string, periodId: string) {
    return meetings.value[key(talentId, periodId)] ?? null
  }

  return { meetings, loading, error, fetchMeeting, createMeeting, disclose, getMeeting }
})
