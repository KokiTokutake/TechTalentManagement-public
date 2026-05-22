<template>
  <div class="page-wrap">
    <header class="page-header">
      <h1>9ボックス評価マップ</h1>
      <div class="page-header__actions">
        <NuxtLink
          v-if="auth.isManager"
          to="/mdc/nine-box/input"
          class="btn btn--primary btn--sm"
        >
          評価を入力
        </NuxtLink>
      </div>
    </header>

    <!-- フィルタ -->
    <div class="filters">
      <div class="filter-group">
        <label class="filter-label" for="period-select">考課期間</label>
        <select
          id="period-select"
          v-model="selectedPeriodId"
          class="filter-select"
          data-testid="nine-box-period-select"
          @change="onPeriodChange"
        >
          <option v-for="p in periods" :key="p.id" :value="p.id">
            {{ p.name }}
          </option>
        </select>
      </div>

      <div class="filter-group">
        <label class="filter-label" for="compare-select">前期比較</label>
        <select
          id="compare-select"
          v-model="comparePeriodId"
          class="filter-select"
          @change="onFilterChange"
        >
          <option value="">— 比較なし —</option>
          <option v-for="p in comparablePeriods" :key="p.id" :value="p.id">
            {{ p.name }}
          </option>
        </select>
      </div>

      <div class="filter-group filter-group--search">
        <label class="filter-label" for="search-input">名前で検索</label>
        <input
          id="search-input"
          v-model="searchName"
          type="text"
          class="filter-input"
          placeholder="田中 太郎"
          @input="onSearchInput"
        />
      </div>
    </div>

    <!-- サマリーバー -->
    <div v-if="store.mapData?.summary" class="summary-bar">
      <span class="summary-item">
        評価済み: <strong>{{ store.mapData.summary.totalAssessed }}</strong> / {{ store.mapData.summary.totalTalents }} 名
      </span>
      <span class="summary-divider">·</span>
      <span class="summary-item summary-item--hp">
        重点管理候補: <strong>{{ store.mapData.summary.highPotentialCount }}</strong> 名
      </span>
    </div>

    <!-- 読み込み中 -->
    <div v-if="store.loading" class="loading">読み込み中…</div>

    <!-- エラー -->
    <div v-else-if="store.error" class="alert alert--error">{{ store.error }}</div>

    <!-- 期間未選択 -->
    <div v-else-if="!selectedPeriodId" class="empty-state">
      考課期間を選択してください
    </div>

    <!-- グリッド（評価データなしでも常に表示） -->
    <div v-else-if="store.mapData" class="grid-container">
      <div v-if="store.mapData.summary.totalAssessed === 0" class="no-data-banner">
        この期間の評価データはまだありません。
        <NuxtLink v-if="auth.isManager" to="/mdc/nine-box/input">評価を入力する</NuxtLink>
      </div>
      <NineBoxGrid
        :map-data="store.mapData"
        @talent-click="openPopover"
      />
    </div>

    <!-- ポップオーバー -->
    <NineBoxTalentPopover
      v-if="selectedTalent"
      :talent="selectedTalent"
      :cell-color="selectedTalentCellColor"
      :period-id="selectedPeriodId ?? ''"
      :show-edit-link="auth.isManager"
      @close="selectedTalent = null"
    />
  </div>
</template>

<script setup lang="ts">
import type { NineBoxTalent } from '~/types/nineBox'
import type { EvaluationPeriod } from '~/types/user'

const auth = useAuthStore()
const store = useNineBoxStore()
const config = useRuntimeConfig()

const periods = ref<EvaluationPeriod[]>([])
const selectedPeriodId = ref<string>('')
const comparePeriodId = ref<string>('')
const searchName = ref<string>('')
const selectedTalent = ref<NineBoxTalent | null>(null)

const comparablePeriods = computed(() =>
  periods.value.filter(p => p.id !== selectedPeriodId.value),
)

const selectedTalentCellColor = computed(() => {
  if (!selectedTalent.value || !store.mapData) return '#e5e7eb'
  const cell = store.mapData.cells.find(c =>
    c.talents.some(t => t.talentId === selectedTalent.value?.talentId),
  )
  return cell?.color ?? '#e5e7eb'
})

let searchTimer: ReturnType<typeof setTimeout> | null = null

onMounted(async () => {
  await store.loadCellDefinitions()

  const res = await $fetch<EvaluationPeriod[]>(`${config.public.apiBaseUrl}/api/periods`)
  periods.value = res

  const defaultPeriod = res.find(p => p.isActive) ?? res[0]
  if (defaultPeriod) {
    selectedPeriodId.value = defaultPeriod.id
    await loadMap()
  }
})

async function loadMap() {
  if (!selectedPeriodId.value) return
  store.filters.searchName = searchName.value
  store.selectedComparePeriodId = comparePeriodId.value || null
  await store.loadMap(selectedPeriodId.value)
}

async function onPeriodChange() {
  await loadMap()
}

async function onFilterChange() {
  await loadMap()
}

function onSearchInput() {
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(() => loadMap(), 300)
}

function openPopover(talent: NineBoxTalent) {
  selectedTalent.value = talent
}
</script>

<style scoped>
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.25rem;
}

h1 {
  font-size: 1.5rem;
  font-weight: 800;
  margin: 0;
  color: var(--text);
  font-family: var(--font-heading);
}

.filters {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  margin-bottom: 1rem;
  padding: 1rem;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  min-width: 160px;
}

.filter-group--search {
  flex: 1;
  min-width: 200px;
}

.filter-label {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--text-secondary);
}

.filter-select,
.filter-input {
  padding: 0.45rem 0.65rem;
  border: 1px solid var(--border);
  border-radius: 7px;
  background: var(--surface);
  color: var(--text);
  font-size: 0.85rem;
}

.filter-select:focus,
.filter-input:focus {
  outline: none;
  border-color: var(--accent);
}

.summary-bar {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.5rem 0.75rem;
  background: var(--surface-muted);
  border-radius: 7px;
  margin-bottom: 1rem;
  font-size: 0.8rem;
  color: var(--text-secondary);
}

.summary-item strong {
  color: var(--text);
  font-weight: 700;
}

.summary-item--hp strong {
  color: #f59e0b;
}

.summary-divider {
  color: var(--border);
}

.grid-container {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 1.25rem;
  overflow: auto;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  color: var(--text-secondary);
  font-size: 0.9rem;
}

.empty-state a {
  color: var(--accent);
  margin-left: 0.25rem;
}

.no-data-banner {
  font-size: 0.82rem;
  color: var(--text-secondary);
  margin-bottom: 0.75rem;
}

.no-data-banner a {
  color: var(--accent);
  margin-left: 0.25rem;
}

.loading {
  text-align: center;
  padding: 3rem;
  color: var(--text-secondary);
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
}

.btn--primary {
  background: var(--accent);
  color: #fff;
}

.btn--primary:hover {
  opacity: 0.88;
}

.btn--sm {
  padding: 0.35rem 0.85rem;
  font-size: 0.8rem;
}
</style>
