<template>
  <main class="container">
    <header class="page-header">
      <NuxtLink to="/" class="back-link">← ホーム</NuxtLink>
      <div class="header-row">
        <div>
          <h1>委員会セッション</h1>
          <p class="subtitle">人材開発委員会の開催記録</p>
        </div>
        <NuxtLink v-if="canCreate" to="/mdc/sessions/new" class="btn-primary">
          + 新しいセッションを作成
        </NuxtLink>
      </div>
    </header>

    <div v-if="store.loading" class="loading">読み込み中…</div>

    <div v-else-if="store.error" class="error-box">{{ store.error }}</div>

    <template v-else>
      <div v-if="store.sessions.length === 0" class="empty-state">
        委員会セッションはまだ登録されていません。
      </div>

      <div v-else class="session-list">
        <NuxtLink
          v-for="session in store.sessions"
          :key="session.id"
          :to="`/mdc/sessions/${session.id}`"
          class="session-card"
        >
          <div class="session-card-left">
            <span class="session-date">{{ session.heldOn }}</span>
            <p class="session-minutes-preview">{{ truncate(session.minutes) }}</p>
          </div>
          <div class="session-card-right">
            <span class="badge-count">参加者 {{ session.participantIds.length }} 名</span>
            <span class="badge-count">対象 {{ session.keyPersonIds.length }} 名</span>
            <span class="arrow">›</span>
          </div>
        </NuxtLink>
      </div>
    </template>
  </main>
</template>

<script setup lang="ts">
const auth = useAuthStore()
const store = useCommitteeSessionStore()

const allowedRoles = ['Senior', 'Executive', 'Admin', 'Manager']
if (!auth.currentUser || !allowedRoles.includes(auth.currentUser.role)) {
  await navigateTo('/')
}

const canCreate = computed(() =>
  auth.currentUser?.role === 'Senior' ||
  auth.currentUser?.role === 'Executive' ||
  auth.currentUser?.role === 'Admin',
)

function truncate(text: string, max = 80) {
  return text.length <= max ? text : text.slice(0, max) + '…'
}

onMounted(async () => {
  if (auth.currentUser) {
    await store.fetchList(auth.currentUser.id)
  }
})
</script>

<style scoped>
.container {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem 1rem;
  font-family: sans-serif;
}

.page-header { margin-bottom: 2rem; }

.back-link {
  display: inline-block;
  margin-bottom: 0.75rem;
  font-size: 0.85rem;
  color: #6b7280;
  text-decoration: none;
}
.back-link:hover { color: #2563eb; }

.header-row {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

h1 {
  font-size: 1.5rem;
  font-weight: 800;
  margin: 0 0 0.25rem;
  color: #111827;
}

.subtitle {
  font-size: 0.875rem;
  color: #6b7280;
  margin: 0;
}

.btn-primary {
  display: inline-block;
  background: #2563eb;
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 0.6rem 1.25rem;
  font-size: 0.875rem;
  font-weight: 700;
  text-decoration: none;
  white-space: nowrap;
  transition: background 0.15s;
}
.btn-primary:hover { background: #1d4ed8; }

.loading {
  text-align: center;
  color: #9ca3af;
  padding: 3rem 0;
}

.error-box {
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 8px;
  padding: 1rem 1.25rem;
  color: #dc2626;
  font-size: 0.875rem;
}

.empty-state {
  color: #9ca3af;
  font-size: 0.875rem;
  padding: 3rem 0;
  text-align: center;
}

.session-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.session-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 1.25rem 1.5rem;
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  background: #fff;
  text-decoration: none;
  transition: all 0.12s;
}
.session-card:hover {
  border-color: #93c5fd;
  box-shadow: 0 2px 8px rgba(37, 99, 235, 0.08);
}

.session-card-left { flex: 1; min-width: 0; }

.session-date {
  font-size: 0.9rem;
  font-weight: 700;
  color: #1d4ed8;
}

.session-minutes-preview {
  font-size: 0.8rem;
  color: #6b7280;
  margin: 0.35rem 0 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.session-card-right {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-shrink: 0;
}

.badge-count {
  background: #f3f4f6;
  color: #374151;
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  white-space: nowrap;
}

.arrow {
  font-size: 1.2rem;
  color: #9ca3af;
}
</style>
