<template>
  <div
    :data-testid="`notification-item-${notification.id}`"
    class="notif-item"
    :class="{ 'notif-item--unread': !notification.isRead }"
    @click="handleClick"
  >
    <div class="notif-item-dot" :class="{ 'notif-item-dot--unread': !notification.isRead }" />
    <div class="notif-item-body">
      <p class="notif-item-title">{{ notification.title }}</p>
      <p class="notif-item-text">{{ notification.body }}</p>
      <p class="notif-item-time">{{ timeAgo(notification.createdAt) }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Notification } from '~/types/notification'

const props = defineProps<{ notification: Notification }>()
const emit = defineEmits<{ (e: 'read', id: string): void }>()

function handleClick() {
  if (!props.notification.isRead) {
    emit('read', props.notification.id)
  }
  if (props.notification.linkUrl) {
    navigateTo(props.notification.linkUrl)
  }
}

function timeAgo(iso: string) {
  const diff = Date.now() - new Date(iso).getTime()
  const m = Math.floor(diff / 60000)
  if (m < 1) return 'たった今'
  if (m < 60) return `${m}分前`
  const h = Math.floor(m / 60)
  if (h < 24) return `${h}時間前`
  return `${Math.floor(h / 24)}日前`
}
</script>

<style scoped>
.notif-item {
  display: flex;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  cursor: pointer;
  transition: background 0.12s;
  border-bottom: 1px solid var(--border, #e5e7eb);
}
.notif-item:hover { background: var(--surface-hover, #f9fafb); }
.notif-item--unread { background: rgba(192, 86, 33, 0.05); }

.notif-item-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  margin-top: 0.4rem;
  flex-shrink: 0;
  background: transparent;
}
.notif-item-dot--unread { background: var(--accent, #c05621); }

.notif-item-body { flex: 1; min-width: 0; }

.notif-item-title {
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--text, #111827);
  margin: 0 0 0.2rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.notif-item-text {
  font-size: 0.78rem;
  color: var(--text-muted, #6b7280);
  margin: 0 0 0.2rem;
  overflow: hidden;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.notif-item-time {
  font-size: 0.7rem;
  color: var(--text-muted, #9ca3af);
  margin: 0;
}
</style>
