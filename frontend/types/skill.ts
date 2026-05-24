export interface SkillCategory {
  id: string
  name: string
  displayOrder: number
  createdAt: string
  updatedAt: string
}

export interface SkillItem {
  id: string
  categoryId: string
  name: string
  displayOrder: number
  createdAt: string
  updatedAt: string
}

export type UserSkillStatus = 'Pending' | 'Approved' | 'Rejected'

export interface UserSkill {
  id: string
  userId: string
  skillItemId: string
  skillItemName: string
  categoryId: string
  categoryName: string
  level: number
  comment: string | null
  status: UserSkillStatus
  approvedBy: string | null
  approvedAt: string | null
  rejectedReason: string | null
  createdAt: string
  updatedAt: string
}

export interface SkillMapUser {
  id: string
  displayName: string
  email: string
}

export interface UserLevel {
  userId: string
  level: number
}

export interface SkillMapItem {
  skillItemId: string
  skillItemName: string
  displayOrder: number
  userLevels: UserLevel[]
}

export interface SkillMapCategory {
  categoryId: string
  categoryName: string
  displayOrder: number
  items: SkillMapItem[]
}

export interface SkillMap {
  users: SkillMapUser[]
  categories: SkillMapCategory[]
}

export interface SkillLevelDefinitionItem {
  level: number
  description: string
  examples: string | null
  isFallback: boolean
}

export interface SkillLevelDefinitions {
  skillItemId: string
  levelDefinitions: SkillLevelDefinitionItem[]
}

export type CompetencyCategory = 'Technology' | 'Methodology' | 'BusinessIndustry' | 'Common'

export interface CompetencyMaster {
  id: string
  category: CompetencyCategory
  name: string
  description: string | null
  displayOrder: number
  isActive: boolean
}

export interface EvaluationDetail {
  level: number
  comment: string | null
  updatedAt: string
}

export interface CompetencyEvaluationResult {
  competencyId: string
  competencyName: string
  category: CompetencyCategory
  selfEvaluation: EvaluationDetail | null
  managerEvaluation: EvaluationDetail | null
  gap: number | null
}

export interface CompetencyEvaluationsResponse {
  talentId: string
  periodId: string
  evaluations: CompetencyEvaluationResult[]
}

// SkillGap types
export interface SkillGapSkillItem {
  skillItemId: string
  skillItemName: string
  selfLevel: number | null
  managerLevel: number | null
  gap: number | null
  requiresAttention: boolean
}

export interface SkillGapCompetencyItem {
  competencyId: string
  competencyName: string
  category: CompetencyCategory
  selfLevel: number | null
  managerLevel: number | null
  gap: number | null
  requiresAttention: boolean
}

export interface SkillGapResult {
  talentId: string
  periodId: string
  gapThreshold: number
  skills: SkillGapSkillItem[]
  competencies: SkillGapCompetencyItem[]
}

// CompetencyHeatmap types
export interface CompetencyHeatmapAverage {
  label: string
  avgLevel: number
  count: number
}

export interface CompetencyHeatmapItem {
  competencyId: string
  competencyName: string
  averages: CompetencyHeatmapAverage[]
}

export interface CompetencyHeatmapCategory {
  category: CompetencyCategory
  competencies: CompetencyHeatmapItem[]
}

export interface CompetencyHeatmapResponse {
  periodId: string
  categories: CompetencyHeatmapCategory[]
}

// SkillMigration types
export interface MigrationPreviewItem {
  talentName: string
  skillName: string
  currentLevel: number
  newLevel: number
}

export interface MigrationPreviewResponse {
  conversionRule: Record<string, number>
  affectedCount: number
  preview: MigrationPreviewItem[]
}

export interface MigrationExecuteResponse {
  success: boolean
  affectedCount: number
  migrationLogId: string
  executedAt: string
}
