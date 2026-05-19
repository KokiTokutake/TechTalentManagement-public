export interface CommitteeSession {
  id: string
  heldOn: string
  minutes: string
  createdBy: string
  createdAt: string
  participantIds: string[]
  keyPersonIds: string[]
}

export interface CreateCommitteeSessionRequest {
  heldOn: string
  minutes: string
  participantIds: string[]
  keyPersonIds: string[]
}
