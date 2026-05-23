import type {
  TalentProfileDto,
  ManagerComment,
  UpsertManagerCommentRequest,
  KeyPersonFlagDto,
} from '~/types/talentProfile'

export function useTalentProfileApi() {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl
  const auth = useAuthStore()

  function headers() {
    return { 'X-User-Id': auth.currentUser?.id ?? '' }
  }

  async function fetchProfile(talentId: string, periodId?: string): Promise<TalentProfileDto> {
    const query = periodId ? `?periodId=${periodId}` : ''
    return await $fetch<TalentProfileDto>(`${base}/api/talents/${talentId}/profile${query}`, {
      headers: headers(),
    })
  }

  async function upsertManagerComment(
    talentId: string,
    periodId: string,
    data: UpsertManagerCommentRequest,
  ): Promise<ManagerComment> {
    return await $fetch<ManagerComment>(
      `${base}/api/talents/${talentId}/manager-comment?periodId=${periodId}`,
      { method: 'PUT', body: data, headers: headers() },
    )
  }

  async function toggleKeyPersonFlag(
    talentId: string,
    isActive: boolean,
    reason?: string,
  ): Promise<KeyPersonFlagDto> {
    return await $fetch<KeyPersonFlagDto>(
      `${base}/api/talents/${talentId}/key-person-flag`,
      { method: 'PATCH', body: { isActive, reason: reason ?? null }, headers: headers() },
    )
  }

  return { fetchProfile, upsertManagerComment, toggleKeyPersonFlag }
}
