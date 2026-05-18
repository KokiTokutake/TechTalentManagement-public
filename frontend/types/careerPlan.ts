export interface CareerPlan {
  id: string
  userId: string
  periodYear: number
  longTermGoal: string
  mediumTermGoal: string
  currentFocus: string
  createdAt: string
  updatedAt: string
}

export interface CareerPlanComment {
  id: string
  careerPlanId: string
  authorId: string
  authorName: string
  content: string
  createdAt: string
}

export interface IntegratedViewGoal {
  id: string
  category: string
  title: string
  description: string | null
  displayOrder: number
}

export interface IntegratedViewSkill {
  id: string
  skillItemId: string
  skillItemName: string
  categoryName: string
  level: number
}

export interface IntegratedView {
  will: CareerPlan | null
  can: IntegratedViewSkill[]
  must: IntegratedViewGoal[]
}

export interface SaveCareerPlanRequest {
  longTermGoal: string
  mediumTermGoal: string
  currentFocus: string
}

export interface OrgOverviewItem {
  userId: string
  userName: string
  longTermGoal: string | null
  currentFocus: string | null
  periodYear: number | null
}
