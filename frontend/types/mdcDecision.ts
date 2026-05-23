export type DecisionType =
  | 'KeyPersonAssign'
  | 'KeyPersonRemove'
  | 'DevelopmentPolicyChange'
  | 'TransferConsideration'
  | 'Other'

export type MdcActionItemStatus = 'NotStarted' | 'InProgress' | 'Done' | 'Cancelled'

export interface MdcDecisionTalentDto {
  talentId: string
  talentName: string
}

export interface MdcActionItemDto {
  id: string
  sessionId: string
  decisionId: string | null
  description: string
  assigneeId: string
  assigneeName: string
  dueDate: string | null
  status: MdcActionItemStatus
  completedAt: string | null
  createdAt: string
  updatedAt: string
}

export interface MdcDecisionDto {
  id: string
  sessionId: string
  decisionType: DecisionType
  content: string
  decidedAt: string
  createdBy: string
  createdAt: string
  updatedAt: string
  talents: MdcDecisionTalentDto[]
  actionItems: MdcActionItemDto[]
}

export interface CreateDecisionRequest {
  decisionType: DecisionType
  content: string
  decidedAt: string
  talentIds: string[]
}

export interface UpdateDecisionRequest {
  decisionType: DecisionType
  content: string
  decidedAt: string
  talentIds: string[]
}

export interface CreateActionItemRequest {
  description: string
  assigneeId: string
  dueDate: string | null
}

export const DECISION_TYPE_LABELS: Record<DecisionType, string> = {
  KeyPersonAssign: '重点管理指定',
  KeyPersonRemove: '重点管理解除',
  DevelopmentPolicyChange: '育成方針変更',
  TransferConsideration: '異動検討',
  Other: 'その他',
}

export const ACTION_ITEM_STATUS_LABELS: Record<MdcActionItemStatus, string> = {
  NotStarted: '未着手',
  InProgress: '進行中',
  Done: '完了',
  Cancelled: 'キャンセル',
}
