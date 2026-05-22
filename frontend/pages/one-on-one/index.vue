<template>
  <div class="page-wrap">
    <!-- タレント選択モード -->
    <template v-if="!talentId">
      <header class="page-header">
        <h1>1on1 面談記録</h1>
      </header>
      <div v-if="loadingUsers" class="loading-text">読み込み中…</div>
      <div v-else-if="talents.length === 0" class="empty-state">
        配下タレントが見つかりません。
      </div>
      <div v-else class="talent-grid">
        <div
          v-for="talent in talents"
          :key="talent.id"
          class="talent-card"
          :data-testid="`one-on-one-talent-card-${talent.id}`"
        >
          <div class="talent-info">
            <span class="talent-name">{{ talent.displayName }}</span>
            <span class="talent-role">{{ talent.role }}</span>
          </div>
          <NuxtLink
            :to="`/one-on-one?talentId=${talent.id}&talentName=${encodeURIComponent(talent.displayName)}`"
            class="btn btn--secondary btn--sm"
            :data-testid="`one-on-one-talent-link-${talent.id}`"
          >
            面談記録を見る
          </NuxtLink>
        </div>
      </div>
    </template>

    <!-- 面談履歴一覧モード -->
    <template v-else>
      <header class="page-header">
        <div>
          <h1>1on1 面談記録</h1>
          <p v-if="talentName" class="subtitle">{{ talentName }} の面談履歴</p>
        </div>
        <div class="header-actions">
          <NuxtLink to="/one-on-one" class="btn btn--secondary btn--sm">← 戻る</NuxtLink>
          <NuxtLink
            :to="`/one-on-one/create?talentId=${talentId}&talentName=${encodeURIComponent(talentName)}`"
            class="btn btn--primary btn--sm"
            data-testid="meeting-new-button"
          >
            新規面談を記録
          </NuxtLink>
        </div>
      </header>

      <div v-if="store.loading" class="loading-text">読み込み中…</div>

      <div v-else-if="store.error" class="alert alert--error">{{ store.error }}</div>

      <div v-else-if="store.meetings.length === 0" class="empty-state">
        <p>面談記録がまだありません。</p>
        <NuxtLink
          :to="`/one-on-one/create?talentId=${talentId}&talentName=${encodeURIComponent(talentName)}`"
          class="btn btn--primary"
        >
          最初の面談を記録する
        </NuxtLink>
      </div>

      <div v-else class="meeting-list">
        <div
          v-for="meeting in store.meetings"
          :key="meeting.id"
          class="meeting-row"
          :data-testid="`meeting-list-row-${meeting.id}`"
        >
          <div class="meeting-row-main">
            <div class="meeting-meta">
              <span class="meeting-date">{{ formatDate(meeting.conductedAt) }}</span>
              <span class="format-badge" :class="`format-${meeting.format.toLowerCase()}`">
                {{ formatLabel(meeting.format) }}
              </span>
            </div>
            <p class="meeting-summary">{{ meeting.notesSummary || '（メモなし）' }}</p>
            <div class="meeting-stats">
              <span v-if="meeting.pendingActionItemCount > 0" class="stat stat--pending">
                未完了宿題 {{ meeting.pendingActionItemCount }}件
              </span>
              <span v-else-if="meeting.actionItemCount > 0" class="stat stat--done">
                宿題 {{ meeting.actionItemCount }}件 完了
              </span>
              <span v-if="meeting.shareWithMdc" class="stat stat--mdc">委員会共有</span>
            </div>
          </div>
          <NuxtLink :to="`/one-on-one/${meeting.id}`" class="btn btn--secondary btn--sm">
            詳細
          </NuxtLink>
        </div>

        <!-- ページネーション -->
        <div v-if="totalPages > 1" class="pagination">
          <button
            class="btn btn--secondary btn--sm"
            :disabled="currentPage <= 1"
            @click="changePage(currentPage - 1)"
          >
            前へ
          </button>
          <span class="pagination-info">{{ currentPage }} / {{ totalPages }}</span>
          <button
            class="btn btn--secondary btn--sm"
            :disabled="currentPage >= totalPages"
            @click="changePage(currentPage + 1)"
          >
            次へ
          </button>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
interface TalentUser {
  id: string
  displayName: string
  role: string
}

const route = useRoute()
const auth = useAuthStore()
const store = useOneOnOneStore()
const config = useRuntimeConfig()

const talentId = computed(() => route.query.talentId as string ?? '')
const talentName = computed(() => route.query.talentName as string ?? '')

const loadingUsers = ref(false)
const talents = ref<TalentUser[]>([])
const currentPage = ref(1)
const limit = 10

const totalPages = computed(() => Math.ceil(store.totalCount / limit))

function formatDate(dateStr: string) {
  return dateStr.replace(/-/g, '/')
}

function formatLabel(format: string) {
  const map: Record<string, string> = { InPerson: '対面', Online: 'オンライン', Hybrid: 'ハイブリッド' }
  return map[format] ?? format
}

async function changePage(page: number) {
  currentPage.value = page
  await store.fetchMeetings(talentId.value, page, limit)
}

onMounted(async () => {
  if (!auth.currentUser) {
    await navigateTo('/')
    return
  }

  if (talentId.value) {
    await store.fetchMeetings(talentId.value, 1, limit)
  } else {
    loadingUsers.value = true
    try {
      const users = await $fetch<TalentUser[]>(`${config.public.apiBaseUrl}/api/users`, {
        headers: { 'X-User-Id': auth.currentUser.id },
      })
      talents.value = users.filter(u => u.role === 'Talent')
    } catch {
      talents.value = []
    } finally {
      loadingUsers.value = false
    }
  }
})
</script>

<style scoped>
.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
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

.header-actions {
  display: flex;
  gap: 0.5rem;
  flex-shrink: 0;
}

/* Talent selector */
.loading-text {
  color: var(--text-secondary);
  font-size: 0.9rem;
}

.empty-state {
  color: var(--text-muted);
  font-size: 0.9rem;
  padding: 2.5rem;
  text-align: center;
  border: 1px dashed var(--border);
  border-radius: 10px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.talent-grid {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.talent-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.25rem;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;
}

.talent-info {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}

.talent-name {
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--text);
}

.talent-role {
  font-size: 0.75rem;
  color: var(--text-muted);
}

/* Meeting list */
.meeting-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.meeting-row {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 1.25rem;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;
  transition: border-color 0.15s;
}

.meeting-row:hover {
  border-color: var(--accent);
}

.meeting-row-main {
  flex: 1;
  min-width: 0;
}

.meeting-meta {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.35rem;
}

.meeting-date {
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--text);
  font-family: var(--font-mono);
}

.format-badge {
  font-size: 0.7rem;
  font-weight: 600;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.format-inperson { background: #dcfce7; color: #166534; }
.format-online   { background: #dbeafe; color: #1e40af; }
.format-hybrid   { background: #fef9c3; color: #854d0e; }

.meeting-summary {
  font-size: 0.85rem;
  color: var(--text-secondary);
  margin: 0 0 0.4rem;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.meeting-stats {
  display: flex;
  gap: 0.4rem;
  flex-wrap: wrap;
}

.stat {
  font-size: 0.7rem;
  font-weight: 600;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
}

.stat--pending { background: #fee2e2; color: #991b1b; }
.stat--done    { background: #d1fae5; color: #065f46; }
.stat--mdc     { background: #ede9fe; color: #5b21b6; }

/* Pagination */
.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  margin-top: 0.5rem;
}

.pagination-info {
  font-size: 0.8rem;
  color: var(--text-secondary);
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
  white-space: nowrap;
}

.btn--primary { background: var(--accent); color: #fff; }
.btn--primary:hover { opacity: 0.88; }

.btn--secondary { background: var(--surface-muted); color: var(--text-secondary); }
.btn--secondary:hover:not(:disabled) { background: var(--border); }
.btn--secondary:disabled { opacity: 0.5; cursor: not-allowed; }

.btn--sm { padding: 0.35rem 0.85rem; font-size: 0.8rem; }
</style>
