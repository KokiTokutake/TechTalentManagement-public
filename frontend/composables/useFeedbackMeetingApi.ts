import type {
  FeedbackMeetingDto,
  CreateFeedbackMeetingRequest,
  DisclosedAtDto,
} from '~/types/goalKpi'

export function useFeedbackMeetingApi() {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl
  const auth = useAuthStore()

  function headers() {
    return { 'X-User-Id': auth.currentUser?.id ?? '' }
  }

  async function fetchMeeting(talentId: string, periodId: string): Promise<FeedbackMeetingDto | null> {
    try {
      return await $fetch<FeedbackMeetingDto>(`${base}/api/feedback-meetings?talentId=${talentId}&periodId=${periodId}`, {
        headers: headers(),
      })
    } catch (e: any) {
      if (e?.status === 404) return null
      throw e
    }
  }

  async function createMeeting(data: CreateFeedbackMeetingRequest): Promise<FeedbackMeetingDto> {
    return $fetch<FeedbackMeetingDto>(`${base}/api/feedback-meetings`, {
      method: 'POST',
      body: data,
      headers: headers(),
    })
  }

  async function disclose(meetingId: string): Promise<DisclosedAtDto> {
    return $fetch<DisclosedAtDto>(`${base}/api/feedback-meetings/${meetingId}/disclose`, {
      method: 'PATCH',
      headers: headers(),
    })
  }

  return { fetchMeeting, createMeeting, disclose }
}
