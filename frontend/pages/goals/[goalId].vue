<template>
  <div class="page-wrap">
    <div v-if="loading" class="loading">読み込み中…</div>
    <template v-else-if="goal">
      <header class="page-header">
        <NuxtLink :to="`/periods/${goal.evaluationPeriodId}/goals`" class="back-link">← 目標一覧</NuxtLink>
        <h1>{{ goal.title }}</h1>
        <span class="category-badge" :class="`badge-${goal.category.toLowerCase()}`">{{ goal.category }}</span>
      </header>

      <p v-if="goal.description" class="description">{{ goal.description }}</p>

      <section v-if="goal.category === 'Must'" class="section">
        <h2 class="section-title">定量KPI</h2>
        <GoalKpiDisplay :kpi="kpiStore.kpiByGoalId[goalId] ?? null" />
        <GoalKpiForm
          :goal-id="goalId"
          :kpi="kpiStore.kpiByGoalId[goalId] ?? null"
          :editable="isGoalRegistrationOpen"
          @saved="onKpiSaved"
        />
      </section>

      <section class="section">
        <h2 class="section-title">チェックイン</h2>
        <GoalCheckinPanel
          :goal-id="goalId"
          :kpi="kpiStore.kpiByGoalId[goalId] ?? null"
          :checkins="checkins"
          @saved="onCheckinSaved"
        />
      </section>
    </template>
    <p v-else class="error-box" role="alert">目標が見つかりませんでした。</p>
  </div>
</template>

<script setup lang="ts">
import type { Goal } from '~/types/goal'
import type { GoalCheckinDto } from '~/types/goalKpi'
import GoalKpiDisplay from '~/components/goals/GoalKpiDisplay.vue'
import GoalKpiForm from '~/components/goals/GoalKpiForm.vue'
import GoalCheckinPanel from '~/components/goals/GoalCheckinPanel.vue'

const route = useRoute()
const goalId = route.params.goalId as string

const auth = useAuthStore()
const goalsStore = useGoalsStore()
const kpiStore = useGoalKpiStore()

const goal = ref<Goal | null>(null)
const loading = ref(true)

const checkins = computed<GoalCheckinDto[]>(
  () => kpiStore.checkinsByGoalId[goalId]?.checkins ?? []
)

const isGoalRegistrationOpen = computed(() => {
  const period = auth.currentPeriod
  if (!period) return false
  const today = new Date().toISOString().split('T')[0]
  return today >= period.goalStartDate && today <= period.goalEndDate
})

onMounted(async () => {
  await goalsStore.load(auth.currentPeriod?.id ?? '', auth.currentUser!.id)
  goal.value = goalsStore.goals.find(g => g.id === goalId) ?? null

  if (goal.value?.category === 'Must') {
    await kpiStore.fetchKpi(goalId)
  }
  await kpiStore.fetchCheckins(goalId)
  loading.value = false
})

async function onKpiSaved() {
  await kpiStore.fetchKpi(goalId)
}

async function onCheckinSaved() {
  await kpiStore.fetchCheckins(goalId)
}
</script>

<style scoped>
.page-header {
  margin-bottom: 1.5rem;
}
.back-link {
  display: inline-block;
  font-size: 0.85rem;
  color: var(--accent);
  text-decoration: none;
  margin-bottom: 0.5rem;
}
.back-link:hover { text-decoration: underline; }
.page-header h1 {
  font-size: 1.3rem;
  font-weight: 700;
  margin: 0.25rem 0;
  color: var(--text);
}
.category-badge {
  font-size: 0.75rem;
  font-weight: 700;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
}
.badge-will { background: #dbeafe; color: var(--accent); }
.badge-can  { background: #d1fae5; color: #065f46; }
.badge-must { background: #fee2e2; color: var(--danger-text); }
.description {
  font-size: 0.9rem;
  color: var(--text-muted);
  white-space: pre-wrap;
  margin-bottom: 1.5rem;
}
.section { margin-bottom: 2rem; }
.section-title {
  font-size: 1rem;
  font-weight: 700;
  margin: 0 0 0.5rem;
  color: var(--text-secondary);
}
.loading { color: var(--text-muted); }
.error-box {
  color: var(--danger-text);
  background: var(--danger-bg);
  border: 1px solid var(--danger-border);
  border-radius: var(--radius-sm);
  padding: 0.5rem 0.75rem;
}
</style>
