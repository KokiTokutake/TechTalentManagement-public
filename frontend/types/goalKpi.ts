export interface GoalKpiDto {
  id: string
  goalId: string
  metricName: string
  targetValue: number
  unit: string
  measurementMethod: string | null
  dueDate: string | null
  createdAt: string
  updatedAt: string
}

export interface GoalCheckinDto {
  id: string
  goalId: string
  kpiId: string | null
  userId: string
  currentValue: number | null
  progressPercent: number | null
  comment: string | null
  checkedInAt: string
}

export interface GoalCheckinsResponse {
  kpi: GoalKpiDto | null
  checkins: GoalCheckinDto[]
}

export interface SaveGoalKpiRequest {
  metricName: string
  targetValue: number
  unit: string
  measurementMethod: string | null
  dueDate: string | null
}

export interface CreateCheckinRequest {
  currentValue: number | null
  comment: string | null
}

export interface FeedbackMeetingDto {
  id: string
  selfEvaluationId: string
  talentId: string
  managerId: string
  conductedAt: string
  overallComment: string | null
  nextPeriodExpectation: string | null
  talentReactionNote: string | null
  isDisclosed: boolean
  disclosedAt: string | null
}

export interface CreateFeedbackMeetingRequest {
  selfEvaluationId: string
  talentId: string
  conductedAt: string
  overallComment: string | null
  nextPeriodExpectation: string | null
  talentReactionNote: string | null
}

export interface DisclosedAtDto {
  id: string
  isDisclosed: boolean
  disclosedAt: string
}
