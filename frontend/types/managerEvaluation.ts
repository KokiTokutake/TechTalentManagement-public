export interface ManagerEvaluation {
  id: string
  selfEvaluationId: string
  managerId: string
  overallScore: number
  feedbackComment: string | null
  isConfirmed: boolean
  confirmedAt: string | null
  createdAt: string
  updatedAt: string
}

export interface SaveManagerDraftRequest {
  overallScore: number
  feedbackComment: string | null
}
