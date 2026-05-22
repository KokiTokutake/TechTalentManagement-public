<template>
  <div class="page-wrap">
    <header class="page-header">
      <div>
        <h1>1on1 面談記録 — 詳細</h1>
        <p v-if="store.currentMeeting" class="subtitle">
          {{ store.currentMeeting.talentName }} ・ {{ formatDate(store.currentMeeting.conductedAt) }}
        </p>
      </div>
      <NuxtLink :to="backLink" class="btn btn--secondary btn--sm">← 戻る</NuxtLink>
    </header>

    <div v-if="store.loading" class="loading-text">読み込み中…</div>

    <div v-else-if="store.error" class="alert alert--error">{{ store.error }}</div>

    <template v-else-if="store.currentMeeting">
      <!-- 基本情報 -->
      <div class="detail-card">
        <div class="card-section">
          <h2 class="section-title">基本情報</h2>
          <div class="meta-grid">
            <div class="meta-item">
              <span class="meta-label">実施日</span>
              <span class="meta-value mono">{{ formatDate(store.currentMeeting.conductedAt) }}</span>
            </div>
            <div class="meta-item">
              <span class="meta-label">形式</span>
              <span class="format-badge" :class="`format-${store.currentMeeting.format.toLowerCase()}`">
                {{ formatLabel(store.currentMeeting.format) }}
              </span>
            </div>
            <div class="meta-item">
              <span class="meta-label">実施上長</span>
              <span class="meta-value">{{ store.currentMeeting.managerName }}</span>
            </div>
            <div class="meta-item">
              <span class="meta-label">委員会共有</span>
              <span class="meta-value">{{ store.currentMeeting.shareWithMdc ? '共有済み' : '非共有' }}</span>
            </div>
          </div>
        </div>

        <!-- 面談メモ -->
        <div class="card-section">
          <h2 class="section-title">面談メモ</h2>
          <div class="notes-grid">
            <div v-if="store.currentMeeting.willTopics" class="note-block note-block--will">
              <span class="note-label">Will（やりたいこと）</span>
              <p class="note-text">{{ store.currentMeeting.willTopics }}</p>
            </div>
            <div v-if="store.currentMeeting.canTopics" class="note-block note-block--can">
              <span class="note-label">Can（できること）</span>
              <p class="note-text">{{ store.currentMeeting.canTopics }}</p>
            </div>
            <div v-if="store.currentMeeting.mustTopics" class="note-block note-block--must">
              <span class="note-label">Must（やるべきこと）</span>
              <p class="note-text">{{ store.currentMeeting.mustTopics }}</p>
            </div>
            <div v-if="store.currentMeeting.situationNote" class="note-block">
              <span class="note-label">近況・コンテキスト</span>
              <p class="note-text">{{ store.currentMeeting.situationNote }}</p>
            </div>
            <div v-if="store.currentMeeting.freeText" class="note-block">
              <span class="note-label">自由記述・その他</span>
              <p class="note-text">{{ store.currentMeeting.freeText }}</p>
            </div>
            <div
              v-if="!store.currentMeeting.willTopics && !store.currentMeeting.canTopics && !store.currentMeeting.mustTopics && !store.currentMeeting.situationNote && !store.currentMeeting.freeText"
              class="empty-note"
            >
              メモはありません。
            </div>
          </div>
        </div>

        <!-- 宿題 -->
        <div class="card-section">
          <h2 class="section-title">宿題・アクションアイテム</h2>
          <div v-if="store.currentMeeting.actionItems.length === 0" class="empty-note">
            宿題はありません。
          </div>
          <ul v-else class="action-item-list">
            <li
              v-for="item in store.currentMeeting.actionItems"
              :key="item.id"
              class="action-item"
              :class="{ 'action-item--done': item.status === 'Completed' }"
            >
              <input
                type="checkbox"
                :checked="item.status === 'Completed'"
                :data-testid="`meeting-action-item-check-${item.id}`"
                class="action-item-check"
                :disabled="!isManager"
                @change="toggleActionItem(item.id, item.status)"
              />
              <span class="action-item-text">{{ item.description }}</span>
              <span v-if="item.status === 'Completed'" class="action-item-badge">完了</span>
            </li>
          </ul>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
const route = useRoute()
const auth = useAuthStore()
const store = useOneOnOneStore()

const meetingId = computed(() => route.params.id as string)
const backLink = computed(() => {
  const m = store.currentMeeting
  return m ? `/one-on-one?talentId=${m.talentId}&talentName=${encodeURIComponent(m.talentName)}` : '/one-on-one'
})

const isManager = computed(() =>
  ['Manager', 'Senior', 'Executive', 'Admin'].includes(auth.currentUser?.role ?? ''),
)

function formatDate(dateStr: string) {
  return dateStr.replace(/-/g, '/')
}

function formatLabel(format: string) {
  const map: Record<string, string> = { InPerson: '対面', Online: 'オンライン', Hybrid: 'ハイブリッド' }
  return map[format] ?? format
}

async function toggleActionItem(itemId: string, currentStatus: string) {
  if (!auth.currentUser) return
  const newStatus = currentStatus === 'Completed' ? 'Pending' : 'Completed'
  await store.updateActionItemStatus(itemId, newStatus, auth.currentUser.id)
}

onMounted(async () => {
  if (!auth.currentUser) {
    await navigateTo('/')
    return
  }
  await store.fetchMeeting(meetingId.value)
})
</script>

<style scoped>
.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.25rem;
}

h1 {
  font-size: 1.4rem;
  font-weight: 800;
  margin: 0 0 0.25rem;
  color: var(--text);
  font-family: var(--font-heading);
}

.subtitle {
  font-size: 0.875rem;
  color: var(--text-secondary);
  margin: 0;
}

.loading-text {
  color: var(--text-secondary);
  font-size: 0.9rem;
}

.detail-card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 12px;
  overflow: hidden;
}

.card-section {
  padding: 1.5rem 1.75rem;
  border-bottom: 1px solid var(--border);
}

.card-section:last-child {
  border-bottom: none;
}

.section-title {
  font-size: 0.8rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--text-muted);
  margin: 0 0 1rem;
}

.meta-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem 2rem;
}

.meta-item {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}

.meta-label {
  font-size: 0.7rem;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.meta-value {
  font-size: 0.9rem;
  color: var(--text);
}

.meta-value.mono {
  font-family: var(--font-mono);
}

.format-badge {
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  display: inline-block;
}

.format-inperson { background: #dcfce7; color: #166534; }
.format-online   { background: #dbeafe; color: #1e40af; }
.format-hybrid   { background: #fef9c3; color: #854d0e; }

.notes-grid {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.note-block {
  padding: 0.75rem 1rem;
  background: var(--surface-muted);
  border-radius: 8px;
  border-left: 3px solid var(--border);
}

.note-block--will { border-left-color: #7c3aed; }
.note-block--can  { border-left-color: #0891b2; }
.note-block--must { border-left-color: #059669; }

.note-label {
  font-size: 0.72rem;
  font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  display: block;
  margin-bottom: 0.35rem;
}

.note-text {
  font-size: 0.875rem;
  color: var(--text);
  margin: 0;
  white-space: pre-wrap;
  line-height: 1.6;
}

.empty-note {
  font-size: 0.875rem;
  color: var(--text-muted);
  font-style: italic;
}

/* Action items */
.action-item-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.action-item {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  padding: 0.5rem 0.75rem;
  background: var(--surface-muted);
  border-radius: 6px;
  font-size: 0.875rem;
}

.action-item--done .action-item-text {
  text-decoration: line-through;
  color: var(--text-muted);
}

.action-item-check {
  width: 1rem;
  height: 1rem;
  flex-shrink: 0;
  cursor: pointer;
  accent-color: var(--accent);
}

.action-item-check:disabled {
  cursor: not-allowed;
}

.action-item-text {
  flex: 1;
  color: var(--text-secondary);
}

.action-item-badge {
  font-size: 0.7rem;
  font-weight: 600;
  padding: 0.1rem 0.45rem;
  border-radius: 999px;
  background: #d1fae5;
  color: #065f46;
}

/* Alert */
.alert {
  padding: 0.75rem 1rem;
  border-radius: 8px;
  font-size: 0.875rem;
}

.alert--error {
  background: #fee2e2;
  color: #991b1b;
  border: 1px solid #fca5a5;
}

/* Buttons */
.btn {
  padding: 0.55rem 1.25rem;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  border: none;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  transition: all 0.15s;
}

.btn--secondary { background: var(--surface-muted); color: var(--text-secondary); }
.btn--secondary:hover { background: var(--border); }
.btn--sm { padding: 0.35rem 0.85rem; font-size: 0.8rem; }
</style>
