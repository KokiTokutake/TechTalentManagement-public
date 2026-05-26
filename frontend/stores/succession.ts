import { defineStore } from 'pinia'
import { ref } from 'vue'
import type {
  CriticalPosition,
  CandidateListResponse,
  CandidateDto,
  SuccessionMapResponse,
  SuggestResponse,
  CreateCriticalPositionRequest,
  UpdateCriticalPositionRequest,
  AddCandidateRequest,
  UpdateCandidateRequest,
  AddDevelopmentActionRequest,
  UpdateDevelopmentActionRequest,
  DevelopmentActionDto,
} from '~/types/succession'

export const useSuccessionStore = defineStore('succession', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const positions = ref<CriticalPosition[]>([])
  const candidateList = ref<CandidateListResponse | null>(null)
  const mapData = ref<SuccessionMapResponse | null>(null)
  const suggestions = ref<SuggestResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchPositions(requesterId: string) {
    loading.value = true
    error.value = null
    try {
      const res = await $fetch<{ positions: CriticalPosition[] }>(
        `${base}/api/critical-positions?requesterId=${requesterId}`,
      )
      positions.value = res.positions
    }
    catch {
      error.value = '重要ポジションの取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  async function createPosition(requesterId: string, req: CreateCriticalPositionRequest) {
    const data = await $fetch<CriticalPosition>(`${base}/api/critical-positions`, {
      method: 'POST',
      headers: { 'X-User-Id': requesterId },
      body: req,
    })
    positions.value.push(data)
    return data
  }

  async function updatePosition(requesterId: string, positionId: string, req: UpdateCriticalPositionRequest) {
    const data = await $fetch<CriticalPosition>(`${base}/api/critical-positions/${positionId}`, {
      method: 'PUT',
      headers: { 'X-User-Id': requesterId },
      body: req,
    })
    const idx = positions.value.findIndex(p => p.id === positionId)
    if (idx >= 0) positions.value[idx] = data
    return data
  }

  async function fetchCandidates(requesterId: string, positionId: string) {
    loading.value = true
    error.value = null
    try {
      candidateList.value = await $fetch<CandidateListResponse>(
        `${base}/api/critical-positions/${positionId}/candidates?requesterId=${requesterId}`,
      )
    }
    catch {
      error.value = '候補者一覧の取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  async function addCandidate(nominatedBy: string, positionId: string, req: AddCandidateRequest) {
    const data = await $fetch<CandidateDto>(
      `${base}/api/critical-positions/${positionId}/candidates`,
      { method: 'POST', headers: { 'X-User-Id': nominatedBy }, body: req },
    )
    if (candidateList.value) candidateList.value.candidates.push(data)
    return data
  }

  async function updateCandidate(requesterId: string, candidateId: string, req: UpdateCandidateRequest) {
    const data = await $fetch<CandidateDto>(
      `${base}/api/succession-candidates/${candidateId}`,
      { method: 'PUT', headers: { 'X-User-Id': requesterId }, body: req },
    )
    if (candidateList.value) {
      const idx = candidateList.value.candidates.findIndex(c => c.id === candidateId)
      if (idx >= 0) candidateList.value.candidates[idx] = data
    }
    return data
  }

  async function removeCandidate(requesterId: string, candidateId: string) {
    await $fetch(`${base}/api/succession-candidates/${candidateId}?requesterId=${requesterId}`, {
      method: 'DELETE',
    })
    if (candidateList.value) {
      candidateList.value.candidates = candidateList.value.candidates.filter(c => c.id !== candidateId)
    }
  }

  async function addDevelopmentAction(requesterId: string, candidateId: string, req: AddDevelopmentActionRequest) {
    const data = await $fetch<DevelopmentActionDto>(
      `${base}/api/succession-candidates/${candidateId}/development-actions`,
      { method: 'POST', headers: { 'X-User-Id': requesterId }, body: req },
    )
    if (candidateList.value) {
      const c = candidateList.value.candidates.find(c => c.id === candidateId)
      if (c) c.developmentActions.push(data)
    }
    return data
  }

  async function updateDevelopmentAction(requesterId: string, actionId: string, req: UpdateDevelopmentActionRequest) {
    const data = await $fetch<DevelopmentActionDto>(
      `${base}/api/succession-development-actions/${actionId}`,
      { method: 'PUT', headers: { 'X-User-Id': requesterId }, body: req },
    )
    if (candidateList.value) {
      for (const c of candidateList.value.candidates) {
        const idx = c.developmentActions.findIndex(a => a.id === actionId)
        if (idx >= 0) { c.developmentActions[idx] = data; break }
      }
    }
    return data
  }

  async function fetchMap(requesterId: string) {
    loading.value = true
    error.value = null
    try {
      mapData.value = await $fetch<SuccessionMapResponse>(
        `${base}/api/succession-map?requesterId=${requesterId}`,
      )
    }
    catch {
      error.value = 'サクセッションマップの取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  async function fetchSuggestions(requesterId: string, periodId: string, positionId: string) {
    suggestions.value = await $fetch<SuggestResponse>(
      `${base}/api/succession-map/suggest?requesterId=${requesterId}&periodId=${periodId}&positionId=${positionId}`,
    )
  }

  function $reset() {
    positions.value = []
    candidateList.value = null
    mapData.value = null
    suggestions.value = null
    loading.value = false
    error.value = null
  }

  return {
    positions, candidateList, mapData, suggestions, loading, error,
    fetchPositions, createPosition, updatePosition,
    fetchCandidates, addCandidate, updateCandidate, removeCandidate,
    addDevelopmentAction, updateDevelopmentAction,
    fetchMap, fetchSuggestions,
    $reset,
  }
})
