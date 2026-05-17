export type WillCanMust = 'Will' | 'Can' | 'Must'

export interface Goal {
  id: string
  userId: string
  evaluationPeriodId: string
  category: WillCanMust
  title: string
  description: string | null
  displayOrder: number
  createdAt: string
  updatedAt: string
}

export interface CreateGoalRequest {
  category: WillCanMust
  title: string
  description: string | null
  displayOrder: number
}

export interface UpdateGoalRequest {
  title: string
  description: string | null
  displayOrder: number
}
