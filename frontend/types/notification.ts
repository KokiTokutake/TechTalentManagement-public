export interface Notification {
  id: string
  type: string
  title: string
  body: string
  linkUrl: string | null
  isRead: boolean
  readAt: string | null
  createdAt: string
}

export interface UnreadCountResponse {
  count: number
}

export interface ReadNotificationResponse {
  id: string
  isRead: boolean
  readAt: string | null
}

export interface ReadAllResponse {
  updatedCount: number
}
