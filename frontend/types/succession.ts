export type PositionImportance = 'Critical' | 'High' | 'Medium'
export type ReadinessLevel = 'ReadyNow' | 'ReadySoon' | 'ReadyMidTerm'
export type ActionStatus = 'NotStarted' | 'InProgress' | 'Done'

export interface CriticalPosition {
  id: string
  positionName: string
  importance: PositionImportance
  currentHolderId: string | null
  currentHolderName: string | null
  candidateCount: number
  hasReadyNowCandidate: boolean
  createdAt: string
  updatedAt: string
}

export interface CriticalPositionListResponse {
  positions: CriticalPosition[]
}

export interface DevelopmentActionDto {
  id: string
  description: string
  dueDate: string | null
  status: ActionStatus
  completedAt: string | null
}

export interface CandidateDto {
  id: string
  talentId: string
  talentName: string
  nominatedById: string
  nominatedByName: string
  readiness: ReadinessLevel
  developmentGap: string | null
  nominatedAt: string
  updatedAt: string
  developmentActions: DevelopmentActionDto[]
}

export interface CandidateListResponse {
  positionId: string
  positionName: string
  candidates: CandidateDto[]
}

export interface MapCandidateItem {
  talentId: string
  talentName: string
  candidateId: string
}

export interface SuccessionMapPosition {
  positionId: string
  positionName: string
  importance: PositionImportance
  currentHolderId: string | null
  currentHolderName: string | null
  readyNow: MapCandidateItem[]
  readySoon: MapCandidateItem[]
  readyMidTerm: MapCandidateItem[]
  hasReadyNowGap: boolean
}

export interface SuccessionMapResponse {
  positions: SuccessionMapPosition[]
}

export interface SuggestionItem {
  talentId: string
  talentName: string
  potentialLevel: string
  performanceLevel: string
  alreadyCandidate: boolean
}

export interface SuggestResponse {
  suggestions: SuggestionItem[]
}

export interface CreateCriticalPositionRequest {
  positionName: string
  importance: PositionImportance
  currentHolderId: string | null
}

export interface UpdateCriticalPositionRequest {
  positionName: string
  importance: PositionImportance
  currentHolderId: string | null
  isActive: boolean
}

export interface AddCandidateRequest {
  talentId: string
  readiness: ReadinessLevel
  developmentGap: string | null
}

export interface UpdateCandidateRequest {
  readiness: ReadinessLevel
  developmentGap: string | null
}

export interface AddDevelopmentActionRequest {
  description: string
  dueDate: string | null
}

export interface UpdateDevelopmentActionRequest {
  description: string
  dueDate: string | null
  status: ActionStatus
}
