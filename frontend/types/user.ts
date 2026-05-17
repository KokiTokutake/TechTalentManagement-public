export type UserRole = 'Talent' | 'Manager' | 'Senior' | 'Executive' | 'Admin'

export interface AppUser {
  id: string
  displayName: string
  email: string
  role: UserRole
  managerId: string | null
}

export interface EvaluationPeriod {
  id: string
  year: number
  name: string
  goalStartDate: string
  goalEndDate: string
  evaluationStartDate: string
  evaluationEndDate: string
  approvalDeadline: string
  isActive: boolean
}
