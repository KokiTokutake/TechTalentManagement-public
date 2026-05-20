<template>
  <div class="page-wrap-wide">
    <header class="page-header">
      <NuxtLink to="/" class="back-link">← ホーム</NuxtLink>
      <h1>委員会サマリービュー</h1>
      <p class="subtitle">重点管理人物の Will・Can・Must 状況を俯瞰する</p>
    </header>

    <div v-if="store.loading" class="loading">読み込み中…</div>

    <div v-else-if="store.error" class="error-box">{{ store.error }}</div>

    <template v-else>
      <div class="summary-bar">
        <span class="summary-count">{{ store.items.length }} 名</span>
        <span class="summary-sub">（育成方針登録済み: {{ planCount }} 名）</span>
      </div>

      <div v-if="store.items.length === 0" class="empty-note">
        重点管理人物が登録されていません。
      </div>

      <table v-else class="summary-table">
        <thead>
          <tr>
            <th>氏名</th>
            <th>Will（中長期目標）</th>
            <th>Will（今期注力）</th>
            <th class="center-col">Can（スキル数）</th>
            <th class="center-col">Must（育成方針）</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in store.items" :key="item.keyPersonId">
            <td class="name-cell">{{ item.talentName }}</td>
            <td>
              <span v-if="item.longTermGoal" class="cell-text">{{ item.longTermGoal }}</span>
              <span v-else class="cell-empty">未登録</span>
            </td>
            <td>
              <span v-if="item.currentFocus" class="cell-text">{{ item.currentFocus }}</span>
              <span v-else class="cell-empty">未登録</span>
            </td>
            <td class="center-col">
              <span class="skill-badge">{{ item.skillCount }} 件</span>
            </td>
            <td class="center-col">
              <span v-if="item.hasDevelopmentPlan" class="status-badge status-badge--ok">登録済み</span>
              <span v-else class="status-badge status-badge--none">未登録</span>
            </td>
            <td class="action-cell">
              <NuxtLink :to="`/mdc/development-plans/${item.keyPersonId}`" class="btn-detail">
                育成方針
              </NuxtLink>
            </td>
          </tr>
        </tbody>
      </table>
    </template>
  </div>
</template>

<script setup lang="ts">
const auth = useAuthStore()
const store = useCommitteeSummaryStore()

const allowedRoles = ['Senior', 'Executive', 'Admin']
if (!auth.currentUser || !allowedRoles.includes(auth.currentUser.role)) {
  await navigateTo('/')
}

const planCount = computed(() => store.items.filter(i => i.hasDevelopmentPlan).length)

onMounted(async () => {
  if (auth.currentUser) {
    await store.fetchSummary(auth.currentUser.id)
  }
})
</script>

<style scoped>
.page-header { margin-bottom: 2rem; }

h1 {
  font-size: 1.5rem;
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

.summary-bar {
  display: flex;
  align-items: baseline;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.summary-count {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--text);
}

.summary-sub {
  font-size: 0.85rem;
  color: var(--text-secondary);
}

.summary-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.summary-table th {
  text-align: left;
  padding: 0.65rem 1rem;
  background: var(--surface-muted);
  border-bottom: 2px solid var(--border);
  font-weight: 600;
  color: var(--text-secondary);
  white-space: nowrap;
}

.summary-table th.center-col { text-align: center; }

.summary-table td {
  padding: 0.75rem 1rem;
  border-bottom: 1px solid var(--surface-muted);
  vertical-align: top;
  color: var(--text);
}

.summary-table tr:hover td { background: var(--surface-muted); }

.name-cell {
  font-weight: 700;
  white-space: nowrap;
  color: var(--text);
}

.cell-text {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  color: var(--text-secondary);
}

.cell-empty {
  color: var(--text-muted);
  font-size: 0.8rem;
}

.center-col { text-align: center; }

.skill-badge {
  display: inline-block;
  background: #dbeafe;
  color: var(--accent);
  font-size: 0.75rem;
  font-weight: 700;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
}

.status-badge {
  display: inline-block;
  font-size: 0.75rem;
  font-weight: 700;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
}

.status-badge--ok {
  background: #d1fae5;
  color: #065f46;
}

.status-badge--none {
  background: var(--surface-muted);
  color: var(--text-muted);
}

.action-cell { text-align: right; vertical-align: middle; }

.btn-detail {
  display: inline-block;
  border: 1px solid #93c5fd;
  color: var(--accent);
  background: #eff6ff;
  border-radius: 6px;
  padding: 0.3rem 0.75rem;
  font-size: 0.8rem;
  text-decoration: none;
  white-space: nowrap;
  transition: all 0.12s;
}
.btn-detail:hover { background: #dbeafe; }
</style>
