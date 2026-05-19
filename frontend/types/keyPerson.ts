export interface KeyPerson {
  id: string
  talentId: string
  designatedBy: string
  reason: string | null
  designatedAt: string
  isActive: boolean
}

export interface KeyPersonMyStatus {
  id: string | null
  talentId: string
  designatedBy: string | null
  reason: string | null
  designatedAt: string | null
  isActive: boolean
}

export interface DesignateKeyPersonRequest {
  talentId: string
  reason: string | null
}
