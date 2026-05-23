export interface WcmEntryDto {
  id: string | null
  content: string
  displayOrder: number
  linkedSkillItemId?: string | null
  linkedSkillName?: string | null
}

export interface WcmWillDto {
  shortTerm: WcmEntryDto[]
  midTerm: WcmEntryDto[]
  longTerm: WcmEntryDto[]
}

export interface WcmCanDto {
  strengths: WcmEntryDto[]
  challenges: WcmEntryDto[]
}

export interface WcmDto {
  talentId: string
  periodId: string
  will: WcmWillDto
  can: WcmCanDto
  must: WcmEntryDto[]
  managerExpectedRole: string | null
}

export interface SaveWcmEntryRequest {
  id: string | null
  content: string
  linkedSkillItemId?: string | null
  linkedSkillName?: string | null
}

export interface SaveWcmRequest {
  talentId: string
  periodId: string
  will: {
    shortTerm: SaveWcmEntryRequest[]
    midTerm: SaveWcmEntryRequest[]
    longTerm: SaveWcmEntryRequest[]
  }
  can: {
    strengths: SaveWcmEntryRequest[]
    challenges: SaveWcmEntryRequest[]
  }
  must: SaveWcmEntryRequest[]
}

export interface CareerValuesDto {
  id: string | null
  talentId: string
  periodId: string
  importantThings: string | null
  motivationSituations: string | null
  avoidSituations: string | null
  updatedAt: string | null
}

export interface SaveCareerValuesRequest {
  talentId: string
  periodId: string
  importantThings: string | null
  motivationSituations: string | null
  avoidSituations: string | null
}
