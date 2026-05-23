<template>
  <div class="page-wrap">
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

      <!-- 前回積み残し -->
      <MdcCarryoverSection
        v-if="decisionStore.carryoverItems.length > 0 || prevSessionId"
        :items="decisionStore.carryoverItems"
        :can-edit="canEdit"
        @status-change="onCarryoverStatusChange"
      />

      <!-- 決定事項 -->
      <MdcDecisionList
        :decisions="decisionStore.decisions"
        :session-id="sessionId"
        :can-edit="canEdit"
        :all-users="allUsers"
        @decision-create="onDecisionCreate"
        @decision-update="onDecisionUpdate"
        @action-item-create="onActionItemCreate"
        @status-change="onActionItemStatusChange"
      />
    </template>
  </div>
</template>

<script setup lang="ts">
import MdcDecisionList from '~/components/mdc/MdcDecisionList.vue'
import MdcCarryoverSection from '~/components/mdc/MdcCarryoverSection.vue'
import type { CreateDecisionRequest, CreateActionItemRequest, MdcActionItemStatus } from '~/types/mdcDecision'

const route = useRoute()
const auth = useAuthStore()
const store = useCommitteeSessionStore()
const keyPersonStore = useKeyPersonStore()
const decisionStore = useMdcDecisionStore()

const sessionId = route.params.sessionId as string

if (import.meta.client) {
  const allowedRoles = ['Senior', 'Executive', 'Admin', 'Manager']
  if (!auth.currentUser || !allowedRoles.includes(auth.currentUser.role)) {
    await navigateTo('/')
  }
}

const canEdit = computed(() =>
  ['Senior', 'Executive', 'Admin'].includes(auth.currentUser?.role ?? ''),
)

const allUsers = ref<import('~/types/user').AppUser[]>([])
const prevSessionId = ref<string>('')

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
  if (!auth.currentUser) return

  const config = useRuntimeConfig()
  const [users] = await Promise.all([
    $fetch<import('~/types/user').AppUser[]>(`${config.public.apiBaseUrl}/api/users`),
    store.fetchDetail(auth.currentUser.id, sessionId),
    store.fetchList(auth.currentUser.id),
    keyPersonStore.fetchList(auth.currentUser.id),
    decisionStore.fetchDecisions(sessionId),
  ])
  allUsers.value = users

  // 前回セッション（開催日が厳密に古い中で最も直近のもの）を特定して積み残しを取得
  const sessions = store.sessions
  const currentSession = sessions.find(s => s.id === sessionId)
  if (currentSession) {
    const olderSessions = sessions.filter(s => s.heldOn < currentSession.heldOn)
    if (olderSessions.length > 0) {
      const sorted = [...olderSessions].sort((a, b) => {
        const byDate = b.heldOn.localeCompare(a.heldOn)
        if (byDate !== 0) return byDate
        return b.createdAt.localeCompare(a.createdAt)
      })
      prevSessionId.value = sorted[0].id
      await decisionStore.fetchCarryoverItems(prevSessionId.value)
    }
  }
})

onUnmounted(() => {
  decisionStore.$reset()
})

async function onDecisionCreate(sid: string, data: CreateDecisionRequest) {
  await decisionStore.createDecision(sid, data)
}

async function onDecisionUpdate(id: string, data: CreateDecisionRequest) {
  await decisionStore.updateDecision(id, data)
}

async function onActionItemCreate(decisionId: string, data: CreateActionItemRequest) {
  await decisionStore.addActionItem(decisionId, data)
}

async function onActionItemStatusChange(id: string, status: MdcActionItemStatus) {
  await decisionStore.updateActionItemStatus(id, status)
}

async function onCarryoverStatusChange(id: string, status: MdcActionItemStatus) {
  await decisionStore.updateActionItemStatus(id, status)
}
</script>

<style scoped>
.page-header { margin-bottom: 1.5rem; }

h1 {
  font-size: 1.5rem;
  font-weight: 800;
  margin: 0;
  color: var(--text);
  font-family: var(--font-heading);
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
  color: var(--accent);
}

.created-at {
  font-size: 0.8rem;
  color: var(--text-muted);
}

.section-title {
  font-size: 1rem;
  font-weight: 700;
  color: var(--text);
  margin: 0 0 1rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.count-badge {
  font-size: 0.75rem;
  font-weight: 400;
  color: var(--text-secondary);
  background: var(--surface-muted);
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
}

.minutes-text {
  font-size: 0.9rem;
  color: var(--text);
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
  color: var(--text);
  padding: 0.35rem 0;
  border-bottom: 1px solid var(--surface-muted);
}
.person-item:last-child { border-bottom: none; }

.person-link {
  color: var(--accent);
  text-decoration: none;
  font-weight: 600;
}
.person-link:hover { text-decoration: underline; }

.cell-empty {
  font-size: 0.875rem;
  color: var(--text-muted);
}
</style>
