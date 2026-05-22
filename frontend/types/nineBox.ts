export type NineBoxLevel = 'Low' | 'Mid' | 'High'

export type MovementDirection = 'up' | 'down' | 'same' | null

export interface NineBoxCellDefinition {
  performanceLevel: NineBoxLevel
  potentialLevel: NineBoxLevel
  label: string
  description: string
  color: string
}

export interface NineBoxTalent {
  assessmentId: string
  talentId: string
  talentName: string
  initials: string
  role: string
  performanceRationale: string | null
  potentialRationale: string | null
  isHighPotentialCandidate: boolean
  previousCell: {
    performanceLevel: NineBoxLevel
    potentialLevel: NineBoxLevel
  } | null
  movement: MovementDirection
}

export interface NineBoxCell {
  performanceLevel: NineBoxLevel
  potentialLevel: NineBoxLevel
  label: string
  color: string
  count: number
  talents: NineBoxTalent[]
}

export interface NineBoxSummary {
  totalAssessed: number
  totalTalents: number
  highPotentialCount: number
}

export interface NineBoxMapResponse {
  periodId: string
  periodName: string
  cells: NineBoxCell[]
  summary: NineBoxSummary
}

export interface NineBoxAssessmentDto {
  id: string
  talentId: string
  talentName: string
  periodId: string
  performanceLevel: NineBoxLevel
  potentialLevel: NineBoxLevel
  performanceRationale: string | null
  potentialRationale: string | null
  isHighPotentialCandidate: boolean
  assessedAt: string
}

export interface CreateNineBoxAssessmentRequest {
  talentId: string
  periodId: string
  performanceLevel: NineBoxLevel
  potentialLevel: NineBoxLevel
  performanceRationale: string | null
  potentialRationale: string | null
}

export interface UpdateNineBoxAssessmentRequest {
  performanceLevel: NineBoxLevel
  potentialLevel: NineBoxLevel
  performanceRationale: string | null
  potentialRationale: string | null
}

export interface NineBoxHistoryEntry {
  periodId: string
  periodName: string
  performanceLevel: NineBoxLevel
  potentialLevel: NineBoxLevel
  label: string
  assessedAt: string
}

export interface NineBoxHistoryResponse {
  talentId: string
  talentName: string
  history: NineBoxHistoryEntry[]
}

export interface NineBoxDistribution {
  highHigh: number
  highMid: number
  highLow: number
  midHigh: number
  midMid: number
  midLow: number
  lowHigh: number
  lowMid: number
  lowLow: number
}

export interface NineBoxCalibrationTeam {
  managerId: string
  managerName: string
  talentCount: number
  assessedCount: number
  distribution: NineBoxDistribution
  avgPerformanceScore: number
  avgPotentialScore: number
}

export interface NineBoxCalibrationOverall {
  avgPerformanceScore: number
  avgPotentialScore: number
  distribution: NineBoxDistribution
}

export interface NineBoxCalibrationResponse {
  periodId: string
  teams: NineBoxCalibrationTeam[]
  overall: NineBoxCalibrationOverall
}
