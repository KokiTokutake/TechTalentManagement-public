<template>
  <div class="page-wrapper">
    <div class="page-header">
      <h1 class="page-title">通知</h1>
      <div class="page-header-actions">
        <label class="toggle-label">
          <input v-model="unreadOnly" type="checkbox" class="toggle-checkbox" @change="reload">
          未読のみ
        </label>
        <button
          v-if="store.unreadCount > 0"
          data-testid="notification-read-all-button"
          class="btn-secondary"
          :disabled="store.loading"
          @click="handleReadAll"
        >
          すべて既読にする
        </button>
      </div>
    </div>

    <div data-testid="notification-list" class="notif-list">
      <div v-if="store.loading" class="notif-empty">読み込み中...</div>
      <div v-else-if="store.notifications.length === 0" class="notif-empty">
        {{ unreadOnly ? '未読の通知はありません' : '通知はありません' }}
      </div>
      <template v-else>
        <NotificationItem
          v-for="n in store.notifications"
          :key="n.id"
          :notification="n"
          @read="handleRead"
        />
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
const auth = useAuthStore()
const store = useNotificationStore()

const unreadOnly = ref(false)
const userId = computed(() => auth.currentUser?.id ?? '')

async function reload() {
  if (userId.value) {
    await store.fetchNotifications(userId.value, userId.value, unreadOnly.value)
  }
}

async function handleRead(id: string) {
  await store.markAsRead(id, userId.value)
}

async function handleReadAll() {
  await store.markAllAsRead(userId.value, userId.value)
}

onMounted(reload)
</script>

<style scoped>
.page-wrapper {
  padding: 2rem;
  max-width: 720px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.5rem;
  gap: 1rem;
}

.page-title {
  font-size: 1.4rem;
  font-weight: 700;
  color: var(--text, #111827);
  margin: 0;
}

.page-header-actions {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.toggle-label {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.85rem;
  color: var(--text-muted, #6b7280);
  cursor: pointer;
}

.toggle-checkbox { cursor: pointer; }

.btn-secondary {
  padding: 0.4rem 0.9rem;
  font-size: 0.82rem;
  border: 1px solid var(--accent, #c05621);
  color: var(--accent, #c05621);
  background: none;
  border-radius: var(--radius-sm, 6px);
  cursor: pointer;
  font-family: var(--font-body);
  transition: background 0.12s;
}
.btn-secondary:hover { background: rgba(192, 86, 33, 0.08); }
.btn-secondary:disabled { opacity: 0.5; cursor: not-allowed; }

.notif-list {
  background: var(--surface, #fff);
  border: 1px solid var(--border, #e5e7eb);
  border-radius: var(--radius, 8px);
  overflow: hidden;
}

.notif-empty {
  padding: 3rem 1rem;
  text-align: center;
  font-size: 0.9rem;
  color: var(--text-muted, #6b7280);
}
</style>
