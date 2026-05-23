export interface NineBoxSummary {
  performanceLevel: string
  potentialLevel: string
  cellLabel: string
  cellColor: string
  assessedAt: string
}

export interface WillCanMust {
  longTermGoal: string
  mediumTermGoal: string
  currentFocus: string
}

export interface SkillSummaryItem {
  skillName: string
  categoryName: string
  level: number
}

export interface MeetingSummaryItem {
  id: string
  conductedAt: string
  notesSummary: string
}

export interface ManagerComment {
  id: string
  talentId: string
  periodId: string
  currentChallenges: string | null
  developmentPolicy: string | null
  expectedRole: string | null
  updatedAt: string
}

export interface TalentProfileDto {
  talentId: string
  talentName: string
  role: string
  periodId: string
  periodName: string
  isKeyPerson: boolean
  nineBox: NineBoxSummary | null
  willCanMust: WillCanMust | null
  skillSummary: SkillSummaryItem[]
  meetingSummary: MeetingSummaryItem[]
  managerComment: ManagerComment | null
}

export interface UpsertManagerCommentRequest {
  currentChallenges: string | null
  developmentPolicy: string | null
  expectedRole: string | null
}

export interface KeyPersonFlagDto {
  talentId: string
  isActive: boolean
  updatedAt: string | null
}
