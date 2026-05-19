<template>
  <main class="container">
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

      <div v-if="store.items.length === 0" class="empty-state">
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
  </main>
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
.container {
  max-width: 1080px;
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

.summary-bar {
  display: flex;
  align-items: baseline;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.summary-count {
  font-size: 1.25rem;
  font-weight: 700;
  color: #111827;
}

.summary-sub {
  font-size: 0.85rem;
  color: #6b7280;
}

.empty-state {
  color: #9ca3af;
  font-size: 0.875rem;
  padding: 3rem 0;
  text-align: center;
}

.summary-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.summary-table th {
  text-align: left;
  padding: 0.65rem 1rem;
  background: #f9fafb;
  border-bottom: 2px solid #e5e7eb;
  font-weight: 600;
  color: #374151;
  white-space: nowrap;
}

.summary-table th.center-col { text-align: center; }

.summary-table td {
  padding: 0.75rem 1rem;
  border-bottom: 1px solid #f3f4f6;
  vertical-align: top;
  color: #111827;
}

.summary-table tr:hover td { background: #f9fafb; }

.name-cell {
  font-weight: 700;
  white-space: nowrap;
  color: #111827;
}

.cell-text {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  color: #374151;
}

.cell-empty {
  color: #9ca3af;
  font-size: 0.8rem;
}

.center-col { text-align: center; }

.skill-badge {
  display: inline-block;
  background: #dbeafe;
  color: #1d4ed8;
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
  background: #f3f4f6;
  color: #9ca3af;
}

.action-cell { text-align: right; vertical-align: middle; }

.btn-detail {
  display: inline-block;
  border: 1px solid #93c5fd;
  color: #1d4ed8;
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
