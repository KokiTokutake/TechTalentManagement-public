import type {
  NineBoxAssessmentDto,
  NineBoxCellDefinition,
  NineBoxHistoryResponse,
  NineBoxMapResponse,
  CreateNineBoxAssessmentRequest,
  UpdateNineBoxAssessmentRequest,
} from '~/types/nineBox'

export function useNineBoxApi() {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  async function fetchCellDefinitions(): Promise<NineBoxCellDefinition[]> {
    return await $fetch<NineBoxCellDefinition[]>(`${base}/api/nine-box/cells`)
  }

  async function fetchMap(periodId: string, params?: {
    departmentId?: string
    role?: string
    searchName?: string
    comparePeriodId?: string
  }): Promise<NineBoxMapResponse> {
    const query = new URLSearchParams({ periodId })
    if (params?.departmentId) query.set('departmentId', params.departmentId)
    if (params?.role) query.set('role', params.role)
    if (params?.searchName) query.set('searchName', params.searchName)
    if (params?.comparePeriodId) query.set('comparePeriodId', params.comparePeriodId)
    return await $fetch<NineBoxMapResponse>(`${base}/api/nine-box?${query}`)
  }

  async function fetchHistory(talentId: string): Promise<NineBoxHistoryResponse> {
    return await $fetch<NineBoxHistoryResponse>(`${base}/api/nine-box/history?talentId=${talentId}`)
  }

  async function fetchByTalentAndPeriod(talentId: string, periodId: string): Promise<NineBoxAssessmentDto | null> {
    try {
      const mapData = await fetchMap(periodId)
      for (const cell of mapData.cells) {
        const talent = cell.talents.find(t => t.talentId === talentId)
        if (talent) {
          return {
            id: talent.assessmentId,
            talentId: talent.talentId,
            talentName: talent.talentName,
            periodId,
            performanceLevel: cell.performanceLevel,
            potentialLevel: cell.potentialLevel,
            performanceRationale: talent.performanceRationale,
            potentialRationale: talent.potentialRationale,
            isHighPotentialCandidate: talent.isHighPotentialCandidate,
            assessedAt: '',
          } satisfies NineBoxAssessmentDto
        }
      }
      return null
    } catch {
      return null
    }
  }

  async function createAssessment(req: CreateNineBoxAssessmentRequest, assessorId: string): Promise<NineBoxAssessmentDto> {
    return await $fetch<NineBoxAssessmentDto>(`${base}/api/nine-box`, {
      method: 'POST',
      body: req,
      headers: { 'X-User-Id': assessorId },
    })
  }

  async function updateAssessment(id: string, req: UpdateNineBoxAssessmentRequest, assessorId: string): Promise<NineBoxAssessmentDto> {
    return await $fetch<NineBoxAssessmentDto>(`${base}/api/nine-box/${id}`, {
      method: 'PUT',
      body: req,
      headers: { 'X-User-Id': assessorId },
    })
  }

  return { fetchCellDefinitions, fetchMap, fetchHistory, fetchByTalentAndPeriod, createAssessment, updateAssessment }
}
