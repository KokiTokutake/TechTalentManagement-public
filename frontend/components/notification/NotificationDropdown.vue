<template>
  <div data-testid="notification-dropdown" class="notif-dropdown">
    <div class="notif-dropdown-header">
      <span class="notif-dropdown-title">通知</span>
      <button
        v-if="store.unreadCount > 0"
        data-testid="notification-read-all-button"
        class="notif-read-all-btn"
        @click="handleReadAll"
      >
        すべて既読
      </button>
    </div>

    <div v-if="store.loading" class="notif-dropdown-empty">読み込み中...</div>
    <div v-else-if="recent.length === 0" class="notif-dropdown-empty">通知はありません</div>
    <div v-else class="notif-dropdown-list">
      <NotificationItem
        v-for="n in recent"
        :key="n.id"
        :notification="n"
        @read="handleRead"
      />
    </div>

    <div class="notif-dropdown-footer">
      <NuxtLink to="/notifications" class="notif-all-link" @click="emit('close')">
        すべての通知を見る
      </NuxtLink>
    </div>
  </div>
</template>

<script setup lang="ts">
const emit = defineEmits<{ (e: 'close'): void }>()
const auth = useAuthStore()
const store = useNotificationStore()

const recent = computed(() => store.notifications.slice(0, 10))

const userId = computed(() => auth.currentUser?.id ?? '')

async function handleRead(id: string) {
  await store.markAsRead(id, userId.value)
}

async function handleReadAll() {
  await store.markAllAsRead(userId.value, userId.value)
}
</script>

<style scoped>
.notif-dropdown {
  position: absolute;
  top: calc(100% + 0.5rem);
  right: 0;
  width: 360px;
  background: var(--surface, #fff);
  border: 1px solid var(--border, #e5e7eb);
  border-radius: var(--radius, 8px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  z-index: 200;
  overflow: hidden;
}

.notif-dropdown-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.75rem 1rem;
  border-bottom: 1px solid var(--border, #e5e7eb);
}

.notif-dropdown-title {
  font-size: 0.9rem;
  font-weight: 700;
  color: var(--text, #111827);
}

.notif-read-all-btn {
  font-size: 0.75rem;
  color: var(--accent, #c05621);
  background: none;
  border: none;
  cursor: pointer;
  padding: 0;
  font-family: var(--font-body);
}
.notif-read-all-btn:hover { text-decoration: underline; }

.notif-dropdown-list { max-height: 400px; overflow-y: auto; }

.notif-dropdown-empty {
  padding: 2rem 1rem;
  text-align: center;
  font-size: 0.85rem;
  color: var(--text-muted, #6b7280);
}

.notif-dropdown-footer {
  padding: 0.6rem 1rem;
  border-top: 1px solid var(--border, #e5e7eb);
  text-align: center;
}

.notif-all-link {
  font-size: 0.8rem;
  color: var(--accent, #c05621);
  text-decoration: none;
}
.notif-all-link:hover { text-decoration: underline; }
</style>
