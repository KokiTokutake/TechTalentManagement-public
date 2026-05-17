export interface SelfEvaluationItem {
  id: string
  goalId: string
  achievementScore: number
  comment: string | null
  updatedAt: string
}

export interface SelfEvaluation {
  id: string
  userId: string
  evaluationPeriodId: string
  isSubmitted: boolean
  submittedAt: string | null
  createdAt: string
  updatedAt: string
  items: SelfEvaluationItem[]
}

export interface SaveDraftItemRequest {
  goalId: string
  achievementScore: number
  comment: string | null
}

export interface SaveDraftRequest {
  items: SaveDraftItemRequest[]
}
