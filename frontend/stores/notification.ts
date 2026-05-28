import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { Notification } from '~/types/notification'

export const useNotificationStore = defineStore('notification', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const notifications = ref<Notification[]>([])
  const unreadCount = ref(0)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchNotifications(userId: string, requesterId: string, unreadOnly = false) {
    loading.value = true
    error.value = null
    try {
      const data = await $fetch<Notification[]>(
        `${base}/api/notifications?userId=${userId}&unreadOnly=${unreadOnly}`,
        { headers: { 'X-User-Id': requesterId } },
      )
      notifications.value = data
    }
    catch {
      error.value = '通知の取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  async function fetchUnreadCount(userId: string, requesterId: string) {
    try {
      const data = await $fetch<{ count: number }>(
        `${base}/api/notifications/unread-count?userId=${userId}`,
        { headers: { 'X-User-Id': requesterId } },
      )
      unreadCount.value = data.count
    }
    catch {
      // サイレントフェイル（バッジ更新失敗は致命的でない）
    }
  }

  async function markAsRead(notificationId: string, requesterId: string) {
    try {
      await $fetch(
        `${base}/api/notifications/${notificationId}/read`,
        { method: 'PUT', headers: { 'X-User-Id': requesterId } },
      )
      const target = notifications.value.find(n => n.id === notificationId)
      if (target) {
        target.isRead = true
        target.readAt = new Date().toISOString()
      }
      if (unreadCount.value > 0) unreadCount.value--
    }
    catch {
      error.value = '既読化に失敗しました。'
    }
  }

  async function markAllAsRead(userId: string, requesterId: string) {
    try {
      await $fetch(
        `${base}/api/notifications/read-all?userId=${userId}`,
        { method: 'PUT', headers: { 'X-User-Id': requesterId } },
      )
      notifications.value.forEach((n) => {
        n.isRead = true
        n.readAt = new Date().toISOString()
      })
      unreadCount.value = 0
    }
    catch {
      error.value = '一括既読化に失敗しました。'
    }
  }

  function $reset() {
    notifications.value = []
    unreadCount.value = 0
    loading.value = false
    error.value = null
  }

  return {
    notifications, unreadCount, loading, error,
    fetchNotifications, fetchUnreadCount, markAsRead, markAllAsRead, $reset,
  }
})
