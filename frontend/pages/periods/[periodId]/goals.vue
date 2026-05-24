<template>
  <div class="page-wrap">
    <header class="page-header">
      <h1>目標設定</h1>
    </header>

    <div v-if="store.loading" class="loading">読み込み中…</div>
    <p v-else-if="store.error" class="error-box" role="alert">{{ store.error }}</p>

    <template v-else>
      <section
        v-for="cat in CATEGORIES"
        :key="cat.value"
        class="category-section"
      >
        <div class="section-header">
          <span class="category-badge" :class="`badge-${cat.value.toLowerCase()}`">
            {{ cat.value }}
          </span>
          <h2>{{ cat.label }}</h2>
          <button class="btn-add" @click="openCreate(cat.value)">
            ＋ 追加
          </button>
        </div>

        <div v-if="store.byCategory(cat.value).value.length === 0" class="empty">
          目標が登録されていません
        </div>
        <div v-else class="goal-list">
          <GoalCard
            v-for="goal in store.byCategory(cat.value).value"
            :key="goal.id"
            :goal="goal"
            :latest-progress="cat.value === 'Must' ? latestProgress(goal.id) : null"
            @edit="openEdit"
            @delete="handleDelete"
          />
        </div>
      </section>
    </template>

    <GoalForm
      v-if="showForm"
      :initial="editTarget ?? undefined"
      :default-category="formCategory"
      @saved="handleSaved"
      @cancel="showForm = false"
    />
  </div>
</template>

<script setup lang="ts">
import type { Goal, WillCanMust, CreateGoalRequest, UpdateGoalRequest } from '~/types/goal'

const route = useRoute()
const periodId = route.params.periodId as string
const auth = useAuthStore()
const userId = computed(() => auth.currentUser!.id)

const CATEGORIES: { value: WillCanMust; label: string }[] = [
  { value: 'Will', label: 'やりたいこと（Will）' },
  { value: 'Can', label: 'できること（Can）' },
  { value: 'Must', label: 'やるべきこと（Must）' },
]

const store = useGoalsStore()
const kpiStore = useGoalKpiStore()

onMounted(async () => {
  await store.load(periodId, userId.value)
  const mustGoals = store.byCategory('Must').value
  for (const goal of mustGoals) {
    kpiStore.fetchCheckins(goal.id).catch(() => {})
  }
})

function latestProgress(goalId: string): number | null {
  const checkins = kpiStore.checkinsByGoalId[goalId]?.checkins ?? []
  return checkins[0]?.progressPercent ?? null
}

const showForm = ref(false)
const editTarget = ref<Goal | null>(null)
const formCategory = ref<WillCanMust>('Will')

function openCreate(category: WillCanMust) {
  editTarget.value = null
  formCategory.value = category
  showForm.value = true
}

function openEdit(goal: Goal) {
  editTarget.value = goal
  formCategory.value = goal.category
  showForm.value = true
}

async function handleSaved(payload: CreateGoalRequest | UpdateGoalRequest & { category: WillCanMust }) {
  try {
    if (editTarget.value) {
      await store.update(periodId, userId.value, editTarget.value.id, {
        title: payload.title,
        description: payload.description,
        displayOrder: payload.displayOrder,
      })
    } else {
      await store.add(periodId, userId.value, payload as CreateGoalRequest)
    }
    showForm.value = false
  } catch {
    // エラーはストアの error に反映済み
  }
}

async function handleDelete(goalId: string) {
  await store.remove(periodId, userId.value, goalId)
}
</script>

<style scoped>
.page-header {
  margin-bottom: 2rem;
}

.page-header h1 {
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0 0 0.25rem;
  color: var(--text);
  font-family: var(--font-heading);
}

.warn {
  color: #d97706;
  font-size: 0.875rem;
  background: #fffbeb;
  border: 1px solid #fcd34d;
  border-radius: 6px;
  padding: 0.5rem 0.75rem;
}

.category-section {
  margin-bottom: 2.5rem;
}

.section-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}

.section-header h2 {
  flex: 1;
  font-size: 1rem;
  font-weight: 700;
  margin: 0;
  color: var(--text-secondary);
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

.btn-add {
  background: none;
  border: 1px dashed var(--text-muted);
  border-radius: 6px;
  padding: 0.3rem 0.75rem;
  font-size: 0.875rem;
  color: #4b5563;
  cursor: pointer;
}

.btn-add:hover:not(:disabled) {
  border-color: var(--accent);
  color: var(--accent);
  background: #eff6ff;
}

.btn-add:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.empty {
  color: var(--text-muted);
  font-size: 0.875rem;
  padding: 1rem;
  border: 1px dashed var(--border);
  border-radius: 8px;
  text-align: center;
}

.goal-list {
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
}
</style>
