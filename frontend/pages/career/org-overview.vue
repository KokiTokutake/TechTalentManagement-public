<template>
  <main class="container">
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
  </main>
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
.container {
  max-width: 960px;
  margin: 0 auto;
  padding: 2rem 1rem;
  font-family: sans-serif;
}

.page-header {
  margin-bottom: 2rem;
}

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

.overview-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.overview-table th {
  text-align: left;
  padding: 0.65rem 1rem;
  background: #f9fafb;
  border-bottom: 2px solid #e5e7eb;
  font-weight: 600;
  color: #374151;
  white-space: nowrap;
}

.overview-table td {
  padding: 0.75rem 1rem;
  border-bottom: 1px solid #f3f4f6;
  vertical-align: top;
  color: #111827;
}

.overview-table tr:hover td {
  background: #f9fafb;
}

.user-link {
  font-weight: 600;
  color: #2563eb;
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
  color: #9ca3af;
  font-size: 0.8rem;
}

.year-badge {
  display: inline-block;
  background: #dbeafe;
  color: #1d4ed8;
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
}
</style>
