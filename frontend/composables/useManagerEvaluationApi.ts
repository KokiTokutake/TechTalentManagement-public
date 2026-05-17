import type { ManagerEvaluation, SaveManagerDraftRequest } from '~/types/managerEvaluation'

export function useManagerEvaluationApi() {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  function baseUrl(selfEvaluationId: string) {
    return `${base}/api/self-evaluations/${selfEvaluationId}/manager-evaluation`
  }

  async function fetchManagerEvaluation(selfEvaluationId: string, managerId: string): Promise<ManagerEvaluation | null> {
    try {
      return await $fetch<ManagerEvaluation>(`${baseUrl(selfEvaluationId)}?managerId=${managerId}`)
    } catch (e: unknown) {
      if (e && typeof e === 'object' && 'status' in e && e.status === 404) return null
      throw e
    }
  }

  async function saveDraft(selfEvaluationId: string, managerId: string, req: SaveManagerDraftRequest): Promise<ManagerEvaluation> {
    return await $fetch<ManagerEvaluation>(`${baseUrl(selfEvaluationId)}/draft?managerId=${managerId}`, {
      method: 'PUT',
      body: req,
    })
  }

  async function confirm(selfEvaluationId: string, managerId: string): Promise<ManagerEvaluation> {
    return await $fetch<ManagerEvaluation>(`${baseUrl(selfEvaluationId)}/confirm?managerId=${managerId}`, {
      method: 'POST',
    })
  }

  return { fetchManagerEvaluation, saveDraft, confirm }
}
