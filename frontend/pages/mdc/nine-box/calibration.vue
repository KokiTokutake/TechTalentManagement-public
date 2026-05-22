<template>
  <div class="page-wrap">
    <header class="page-header">
      <div>
        <h1>9ボックス キャリブレーション</h1>
        <p class="subtitle">チーム別評価分布と統計を確認し、評価基準のすり合わせを行います</p>
      </div>
      <NuxtLink to="/mdc/nine-box" class="btn btn--secondary btn--sm">← マップに戻る</NuxtLink>
    </header>

    <!-- 期間セレクタ -->
    <div class="filter-bar">
      <div class="filter-group">
        <label class="filter-label" for="cal-period-select">考課期間</label>
        <select
          id="cal-period-select"
          v-model="selectedPeriodId"
          class="filter-select"
          data-testid="nine-box-calibration-period-select"
          @change="loadData"
        >
          <option v-for="p in periods" :key="p.id" :value="p.id">
            {{ p.name }}
          </option>
        </select>
      </div>
    </div>

    <!-- 読み込み中 -->
    <div v-if="loading" class="loading">読み込み中…</div>

    <!-- エラー -->
    <div v-else-if="error" class="alert alert--error">{{ error }}</div>

    <!-- 期間未選択 -->
    <div v-else-if="!selectedPeriodId" class="empty-state">
      考課期間を選択してください
    </div>

    <!-- データなし -->
    <div v-else-if="calibration && calibration.teams.length === 0" class="empty-state">
      この期間にはまだ評価データがありません。
    </div>

    <!-- キャリブレーションテーブル -->
    <div v-else-if="calibration" class="card">
      <NineBoxCalibrationTable
        :teams="calibration.teams"
        :overall="calibration.overall"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { NineBoxCalibrationResponse } from '~/types/nineBox'
import type { EvaluationPeriod } from '~/types/user'

const auth = useAuthStore()
const config = useRuntimeConfig()
const api = useNineBoxApi()

const periods = ref<EvaluationPeriod[]>([])
const selectedPeriodId = ref<string>('')
const calibration = ref<NineBoxCalibrationResponse | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)

onMounted(async () => {
  if (!auth.currentUser) {
    await navigateTo('/')
    return
  }

  const res = await $fetch<EvaluationPeriod[]>(`${config.public.apiBaseUrl}/api/periods`)
  periods.value = res

  const defaultPeriod = res.find(p => p.isActive) ?? res[0]
  if (defaultPeriod) {
    selectedPeriodId.value = defaultPeriod.id
    await loadData()
  }
})

async function loadData() {
  if (!selectedPeriodId.value) return
  loading.value = true
  error.value = null
  try {
    calibration.value = await api.fetchCalibration(selectedPeriodId.value)
  } catch {
    error.value = 'キャリブレーションデータの取得に失敗しました。'
  } finally {
    loading.value = false
  }
}
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

.filter-bar {
  display: flex;
  gap: 1rem;
  margin-bottom: 1.25rem;
  padding: 0.75rem 1rem;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  min-width: 200px;
}

.filter-label {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--text-secondary);
}

.filter-select {
  padding: 0.45rem 0.65rem;
  border: 1px solid var(--border);
  border-radius: 7px;
  background: var(--surface);
  color: var(--text);
  font-size: 0.85rem;
}

.filter-select:focus {
  outline: none;
  border-color: var(--accent);
}

.card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 1.25rem;
  overflow: auto;
}

.loading,
.empty-state {
  text-align: center;
  padding: 3rem;
  color: var(--text-secondary);
  font-size: 0.9rem;
}

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

.btn {
  padding: 0.45rem 1rem;
  border-radius: 7px;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  border: none;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  white-space: nowrap;
}

.btn--secondary {
  background: var(--surface-muted);
  color: var(--text-secondary);
}

.btn--secondary:hover {
  background: var(--border);
}

.btn--sm {
  padding: 0.35rem 0.85rem;
  font-size: 0.8rem;
}
</style>
