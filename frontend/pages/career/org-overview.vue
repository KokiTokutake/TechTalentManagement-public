<template>
  <div class="page-wrap-wide">
    <header class="page-header">
      <NuxtLink to="/" class="back-link">← ホーム</NuxtLink>
      <h1>組織キャリアプラン俯瞰</h1>
      <p class="subtitle">全タレントの今期キャリアプランサマリー</p>
    </header>

    <div v-if="store.loading" class="loading">読み込み中…</div>

    <div v-else-if="store.error" class="error-box">{{ store.error }}</div>

    <template v-else>
      <div class="summary-bar">
        <span class="summary-count">{{ store.orgOverview.length }} 名</span>
        <span class="summary-sub">（今期プラン登録済み: {{ registeredCount }} 名）</span>
      </div>

      <table class="overview-table">
        <thead>
          <tr>
            <th>氏名</th>
            <th>今期注力テーマ</th>
            <th>中長期目標</th>
            <th>プラン年度</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in store.orgOverview" :key="item.userId">
            <td>
              <NuxtLink :to="`/career/${item.userId}`" class="user-link">
                {{ item.userName }}
              </NuxtLink>
            </td>
            <td>
              <span v-if="item.currentFocus" class="cell-text">{{ item.currentFocus }}</span>
              <span v-else class="cell-empty">未登録</span>
            </td>
            <td>
              <span v-if="item.longTermGoal" class="cell-text">{{ item.longTermGoal }}</span>
              <span v-else class="cell-empty">未登録</span>
            </td>
            <td>
              <span v-if="item.periodYear" class="year-badge">{{ item.periodYear }}年</span>
              <span v-else class="cell-empty">—</span>
            </td>
          </tr>
        </tbody>
      </table>
    </template>
  </div>
</template>

<script setup lang="ts">
const auth = useAuthStore()
const store = useCareerPlanStore()

const allowedRoles = ['Senior', 'Executive', 'Admin']
if (!auth.currentUser || !allowedRoles.includes(auth.currentUser.role)) {
  await navigateTo('/')
}

const registeredCount = computed(
  () => store.orgOverview.filter(item => item.periodYear !== null).length,
)

onMounted(async () => {
  if (auth.currentUser) {
    await store.fetchOrgOverview(auth.currentUser.id)
  }
})
</script>

<style scoped>
.page-header {
  margin-bottom: 2rem;
}

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

.overview-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.overview-table th {
  text-align: left;
  padding: 0.65rem 1rem;
  background: var(--surface-muted);
  border-bottom: 2px solid var(--border);
  font-weight: 600;
  color: var(--text-secondary);
  white-space: nowrap;
}

.overview-table td {
  padding: 0.75rem 1rem;
  border-bottom: 1px solid var(--surface-muted);
  vertical-align: top;
  color: var(--text);
}

.overview-table tr:hover td {
  background: var(--surface-muted);
}

.user-link {
  font-weight: 600;
  color: var(--accent);
  text-decoration: none;
  white-space: nowrap;
}

.user-link:hover { text-decoration: underline; }

.cell-text {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.cell-empty {
  color: var(--text-muted);
  font-size: 0.8rem;
}

.year-badge {
  display: inline-block;
  background: #dbeafe;
  color: var(--accent);
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
}
</style>
