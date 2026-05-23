import type {
  MdcDecisionDto,
  MdcActionItemDto,
  CreateDecisionRequest,
  UpdateDecisionRequest,
  CreateActionItemRequest,
  MdcActionItemStatus,
} from '~/types/mdcDecision'

export function useMdcDecisionApi() {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl
  const auth = useAuthStore()

  function headers() {
    return { 'X-User-Id': auth.currentUser?.id ?? '' }
  }

  async function fetchDecisions(sessionId: string): Promise<MdcDecisionDto[]> {
    return $fetch<MdcDecisionDto[]>(`${base}/api/mdc-sessions/${sessionId}/decisions`, {
      headers: headers(),
    })
  }

  async function createDecision(sessionId: string, data: CreateDecisionRequest): Promise<MdcDecisionDto> {
    return $fetch<MdcDecisionDto>(`${base}/api/mdc-sessions/${sessionId}/decisions`, {
      method: 'POST',
      body: data,
      headers: headers(),
    })
  }

  async function updateDecision(id: string, data: UpdateDecisionRequest): Promise<MdcDecisionDto> {
    return $fetch<MdcDecisionDto>(`${base}/api/mdc-decisions/${id}`, {
      method: 'PUT',
      body: data,
      headers: headers(),
    })
  }

  async function fetchActionItems(sessionId: string, status?: MdcActionItemStatus): Promise<MdcActionItemDto[]> {
    const query = status ? `?status=${status}` : ''
    return $fetch<MdcActionItemDto[]>(`${base}/api/mdc-sessions/${sessionId}/action-items${query}`, {
      headers: headers(),
    })
  }

  async function addActionItem(decisionId: string, data: CreateActionItemRequest): Promise<MdcActionItemDto> {
    return $fetch<MdcActionItemDto>(`${base}/api/mdc-decisions/${decisionId}/action-items`, {
      method: 'POST',
      body: data,
      headers: headers(),
    })
  }

  async function updateActionItemStatus(id: string, status: MdcActionItemStatus): Promise<MdcActionItemDto> {
    return $fetch<MdcActionItemDto>(`${base}/api/mdc-action-items/${id}/status`, {
      method: 'PATCH',
      body: { status },
      headers: headers(),
    })
  }

  async function fetchTalentDecisions(talentId: string): Promise<MdcDecisionDto[]> {
    return $fetch<MdcDecisionDto[]>(`${base}/api/talents/${talentId}/mdc-decisions`, {
      headers: headers(),
    })
  }

  return {
    fetchDecisions,
    createDecision,
    updateDecision,
    fetchActionItems,
    addActionItem,
    updateActionItemStatus,
    fetchTalentDecisions,
  }
}
