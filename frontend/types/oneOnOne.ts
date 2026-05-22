export type MeetingFormat = 'InPerson' | 'Online' | 'Hybrid'

export type ActionItemStatus = 'Pending' | 'Completed'

export interface ActionItem {
  id: string
  description: string
  status: ActionItemStatus
  createdAt: string
}

export interface MeetingListItem {
  id: string
  talentId: string
  talentName: string
  managerId: string
  managerName: string
  conductedAt: string
  format: MeetingFormat
  notesSummary: string
  actionItemCount: number
  pendingActionItemCount: number
  shareWithMdc: boolean
  createdAt: string
}

export interface MeetingDetail {
  id: string
  talentId: string
  talentName: string
  managerId: string
  managerName: string
  conductedAt: string
  format: MeetingFormat
  willTopics: string | null
  canTopics: string | null
  mustTopics: string | null
  situationNote: string | null
  freeText: string | null
  shareWithMdc: boolean
  actionItems: ActionItem[]
  createdAt: string
  updatedAt: string | null
}

export interface MeetingListResponse {
  items: MeetingListItem[]
  totalCount: number
  page: number
  limit: number
}

export interface CreateMeetingRequest {
  talentId: string
  conductedAt: string
  format: MeetingFormat
  willTopics: string | null
  canTopics: string | null
  mustTopics: string | null
  situationNote: string | null
  freeText: string | null
  shareWithMdc: boolean
}

export interface UpdateMeetingRequest {
  conductedAt: string
  format: MeetingFormat
  willTopics: string | null
  canTopics: string | null
  mustTopics: string | null
  situationNote: string | null
  freeText: string | null
  shareWithMdc: boolean
}

export interface OneOnOneSummaryMeeting {
  id: string
  conductedAt: string
  notesSummary: string
}

export interface OneOnOneSummary {
  talentId: string
  talentName: string
  lastMeetingAt: string | null
  totalMeetingCount: number
  sharedMeetings: OneOnOneSummaryMeeting[]
}

export interface OneOnOneTemplate {
  id: string
  name: string
  willPlaceholder: string | null
  canPlaceholder: string | null
  mustPlaceholder: string | null
  freeTextPlaceholder: string | null
}
