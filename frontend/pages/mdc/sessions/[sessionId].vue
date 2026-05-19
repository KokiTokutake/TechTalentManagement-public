<template>
  <main class="container">
    <header class="page-header">
      <NuxtLink to="/mdc/sessions" class="back-link">← 委員会セッション一覧</NuxtLink>
      <h1>委員会セッション詳細</h1>
    </header>

    <div v-if="store.loading" class="loading">読み込み中…</div>

    <div v-else-if="store.error" class="error-box">{{ store.error }}</div>

    <template v-else-if="store.currentSession">
      <div class="meta-bar">
        <span class="session-date-large">{{ store.currentSession.heldOn }}</span>
        <span class="created-at">作成日: {{ formatDate(store.currentSession.createdAt) }}</span>
      </div>

      <section class="card">
        <h2 class="section-title">議事録</h2>
        <p class="minutes-text">{{ store.currentSession.minutes }}</p>
      </section>

      <div class="two-col">
        <section class="card">
          <h2 class="section-title">
            参加者
            <span class="count-badge">{{ store.currentSession.participantIds.length }} 名</span>
          </h2>
          <ul v-if="store.currentSession.participantIds.length > 0" class="person-list">
            <li v-for="uid in store.currentSession.participantIds" :key="uid" class="person-item">
              {{ userName(uid) }}
            </li>
          </ul>
          <p v-else class="cell-empty">参加者なし</p>
        </section>

        <section class="card">
          <h2 class="section-title">
            対象重点管理人物
            <span class="count-badge">{{ store.currentSession.keyPersonIds.length }} 名</span>
          </h2>
          <ul v-if="store.currentSession.keyPersonIds.length > 0" class="person-list">
            <li v-for="kpId in store.currentSession.keyPersonIds" :key="kpId" class="person-item">
              <NuxtLink :to="`/mdc/development-plans/${kpId}`" class="person-link">
                {{ keyPersonName(kpId) }}
              </NuxtLink>
            </li>
          </ul>
          <p v-else class="cell-empty">対象者なし</p>
        </section>
      </div>
    </template>
  </main>
</template>

<script setup lang="ts">
const route = useRoute()
const auth = useAuthStore()
const store = useCommitteeSessionStore()
const keyPersonStore = useKeyPersonStore()

const sessionId = route.params.sessionId as string

const allowedRoles = ['Senior', 'Executive', 'Admin', 'Manager']
if (!auth.currentUser || !allowedRoles.includes(auth.currentUser.role)) {
  await navigateTo('/')
}

const allUsers = ref<import('~/types/user').AppUser[]>([])

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('ja-JP', {
    year: 'numeric', month: '2-digit', day: '2-digit',
  })
}

function userName(userId: string) {
  return allUsers.value.find(u => u.id === userId)?.displayName ?? userId
}

function keyPersonName(keyPersonId: string) {
  const kp = keyPersonStore.keyPersons.find(k => k.id === keyPersonId)
  if (!kp) return keyPersonId
  return allUsers.value.find(u => u.id === kp.talentId)?.displayName ?? keyPersonId
}

onMounted(async () => {
  if (auth.currentUser) {
    const config = useRuntimeConfig()
    const [users] = await Promise.all([
      $fetch<import('~/types/user').AppUser[]>(`${config.public.apiBaseUrl}/api/users`),
      store.fetchDetail(auth.currentUser.id, sessionId),
      keyPersonStore.fetchList(auth.currentUser.id),
    ])
    allUsers.value = users
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

.page-header { margin-bottom: 1.5rem; }

.back-link {
  display: inline-block;
  margin-bottom: 0.75rem;
  font-size: 0.85rem;
  color: #6b7280;
  text-decoration: none;
}
.back-link:hover { color: #2563eb; }

h1 {
  font-size: 1.5rem;
  font-weight: 800;
  margin: 0;
  color: #111827;
}

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

.meta-bar {
  display: flex;
  align-items: baseline;
  gap: 1rem;
  margin-bottom: 1.5rem;
  flex-wrap: wrap;
}

.session-date-large {
  font-size: 1.5rem;
  font-weight: 800;
  color: #1d4ed8;
}

.created-at {
  font-size: 0.8rem;
  color: #9ca3af;
}

.card {
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 1.5rem;
  margin-bottom: 1.5rem;
}

.section-title {
  font-size: 1rem;
  font-weight: 700;
  color: #111827;
  margin: 0 0 1rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.count-badge {
  font-size: 0.75rem;
  font-weight: 400;
  color: #6b7280;
  background: #f3f4f6;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
}

.minutes-text {
  font-size: 0.9rem;
  color: #111827;
  white-space: pre-wrap;
  word-break: break-word;
  line-height: 1.7;
  margin: 0;
}

.two-col {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

@media (max-width: 600px) {
  .two-col { grid-template-columns: 1fr; }
}

.person-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.person-item {
  font-size: 0.875rem;
  color: #111827;
  padding: 0.35rem 0;
  border-bottom: 1px solid #f3f4f6;
}
.person-item:last-child { border-bottom: none; }

.person-link {
  color: #2563eb;
  text-decoration: none;
  font-weight: 600;
}
.person-link:hover { text-decoration: underline; }

.cell-empty {
  font-size: 0.875rem;
  color: #9ca3af;
}
</style>
