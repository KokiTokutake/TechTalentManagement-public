import type { SelfEvaluation, SaveDraftRequest } from '~/types/selfEvaluation'

export function useSelfEvaluationApi() {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  function baseUrl(periodId: string, userId: string) {
    return `${base}/api/periods/${periodId}/users/${userId}/self-evaluation`
  }

  async function fetchSelfEvaluation(periodId: string, userId: string): Promise<SelfEvaluation | null> {
    try {
      return await $fetch<SelfEvaluation>(baseUrl(periodId, userId))
    } catch (e: unknown) {
      if (e && typeof e === 'object' && 'status' in e && e.status === 404) return null
      throw e
    }
  }

  async function saveDraft(periodId: string, userId: string, req: SaveDraftRequest): Promise<SelfEvaluation> {
    return await $fetch<SelfEvaluation>(`${baseUrl(periodId, userId)}/draft`, {
      method: 'PUT',
      body: req,
    })
  }

  async function submit(periodId: string, userId: string): Promise<SelfEvaluation> {
    return await $fetch<SelfEvaluation>(`${baseUrl(periodId, userId)}/submit`, {
      method: 'POST',
    })
  }

  return { fetchSelfEvaluation, saveDraft, submit }
}
