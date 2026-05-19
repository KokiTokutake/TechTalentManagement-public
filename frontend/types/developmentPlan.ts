export interface DevelopmentPlan {
  id: string
  keyPersonId: string
  authorId: string
  challenge: string
  goal: string
  supportAction: string
  dueDate: string | null
  version: number
  createdAt: string
}

export interface SaveDevelopmentPlanRequest {
  challenge: string
  goal: string
  supportAction: string
  dueDate: string | null
}
