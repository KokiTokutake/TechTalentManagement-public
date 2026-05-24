<template>
  <div class="page-wrap-wide">
    <header class="page-header">
      <NuxtLink to="/" class="back-link">← ホームへ戻る</NuxtLink>
      <h1>組織スキルマップ</h1>
      <p class="subtitle">承認済みスキルのみ表示されます</p>
    </header>

    <!-- タブ切り替え -->
    <div class="tab-bar">
      <button
        class="tab-btn"
        :class="{ active: activeTab === 'skills' }"
        @click="activeTab = 'skills'"
      >スキルマップ</button>
      <button
        class="tab-btn"
        :class="{ active: activeTab === 'competency' }"
        data-testid="skillmap-competency-tab"
        @click="switchToCompetency"
      >コンピテンシーヒートマップ</button>
    </div>

    <!-- スキルマップタブ -->
    <div v-if="activeTab === 'skills'">
      <div v-if="skillError" class="error-box" role="alert">{{ skillError }}</div>
      <div v-if="skillLoading" class="loading">読み込み中…</div>

      <template v-else-if="skillMap">
        <div v-if="!skillMap.users.length" class="empty-note">タレントが登録されていません。</div>
        <div v-else-if="!skillMap.categories.length" class="empty-note">承認済みスキルがまだありません。</div>

        <div v-for="cat in skillMap.categories" :key="cat.categoryId" class="category-section">
          <h2 class="category-title">{{ cat.categoryName }}</h2>
          <div class="table-wrapper">
            <table class="skill-table">
              <thead>
                <tr>
                  <th class="skill-col">スキル</th>
                  <th v-for="user in skillMap.users" :key="user.id" class="user-col">
                    {{ shortName(user.displayName) }}
                  </th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in cat.items" :key="item.skillItemId">
                  <td class="skill-col">{{ item.skillItemName }}</td>
                  <td
                    v-for="user in skillMap.users"
                    :key="user.id"
                    class="level-cell"
                    :class="levelClass(getLevelForUser(item, user.id))"
                    :title="levelTooltip(getLevelForUser(item, user.id))"
                  >
                    <span v-if="getLevelForUser(item, user.id)">{{ getLevelForUser(item, user.id) }}</span>
                    <span v-else class="empty-cell">—</span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <section class="legend">
          <h3>レベル凡例</h3>
          <div class="legend-items">
            <span v-for="(label, idx) in levelLabels" :key="idx" class="legend-item" :class="`lv-${idx + 1}`">
              {{ idx + 1 }} - {{ label }}
            </span>
          </div>
        </section>
      </template>
    </div>

    <!-- コンピテンシーヒートマップタブ -->
    <div v-else-if="activeTab === 'competency'">
      <div v-if="!canViewCompetencyMap" class="error-box" role="alert">
        このマップは Senior / Admin のみ閲覧できます。
      </div>
      <template v-else>
        <div v-if="!auth.currentPeriod" class="error-box" role="alert">
          考課期間が選択されていません。ホームで期間を選択してください。
        </div>
        <template v-else>
          <div v-if="heatmapError" class="error-box" role="alert">{{ heatmapError }}</div>
          <div v-if="heatmapLoading" class="loading">読み込み中…</div>
          <div v-else-if="heatmapData" data-testid="competency-heatmap">
            <CompetencyHeatmap :categories="heatmapData.categories" />
          </div>
        </template>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { SkillMap, SkillMapItem, CompetencyHeatmapResponse } from '~/types/skill'

const auth = useAuthStore()
const config = useRuntimeConfig()

const activeTab = ref<'skills' | 'competency'>('skills')

// スキルマップ
const skillMap = ref<SkillMap | null>(null)
const skillLoading = ref(false)
const skillError = ref<string | null>(null)
const requesterId = computed(() => auth.currentUser?.id ?? '')

// コンピテンシーヒートマップ
const heatmapData = ref<CompetencyHeatmapResponse | null>(null)
const heatmapLoading = ref(false)
const heatmapError = ref<string | null>(null)

const levelLabels = ['知識あり', '学習中', '実務可', '指導可', 'エキスパート', '上級', 'マスター']

const canViewCompetencyMap = computed(() => {
  const role = auth.currentUser?.role
  return role === 'Senior' || role === 'Executive' || role === 'Admin'
})

onMounted(async () => {
  skillLoading.value = true
  skillError.value = null
  try {
    skillMap.value = await $fetch<SkillMap>(
      `${config.public.apiBaseUrl}/api/skill-map?requesterId=${requesterId.value}`,
    )
  }
  catch {
    skillError.value = 'スキルマップの取得に失敗しました。'
  }
  finally {
    skillLoading.value = false
  }
})

async function switchToCompetency() {
  activeTab.value = 'competency'
  if (heatmapData.value || !canViewCompetencyMap.value || !auth.currentPeriod) return
  heatmapLoading.value = true
  heatmapError.value = null
  try {
    heatmapData.value = await $fetch<CompetencyHeatmapResponse>(
      `${config.public.apiBaseUrl}/api/skill-map/competency?requesterId=${requesterId.value}&periodId=${auth.currentPeriod.id}`,
    )
  }
  catch {
    heatmapError.value = 'コンピテンシーヒートマップの取得に失敗しました。'
  }
  finally {
    heatmapLoading.value = false
  }
}

function getLevelForUser(item: SkillMapItem, userId: string): number | null {
  return item.userLevels.find(ul => ul.userId === userId)?.level ?? null
}

function shortName(displayName: string): string {
  return displayName.split(/\s+/)[0] ?? displayName
}

function levelClass(level: number | null): string {
  if (!level) return ''
  return `lv-${Math.min(level, 7)}`
}

function levelTooltip(level: number | null): string {
  if (!level) return 'スキルなし'
  return `Lv.${level} - ${levelLabels[level - 1] ?? ''}`
}
</script>

<style scoped>
.page-header { margin-bottom: 1.5rem; }
h1 { font-size: 1.4rem; font-weight: 800; margin: 0 0 0.25rem; color: var(--text); font-family: var(--font-heading); }
.subtitle { font-size: 0.9rem; color: var(--text-muted); margin: 0; }
.back-link { font-size: 0.85rem; color: var(--accent); text-decoration: none; display: block; margin-bottom: 0.5rem; }
.back-link:hover { text-decoration: underline; }

.tab-bar { display: flex; gap: 0; border-bottom: 2px solid var(--border); margin-bottom: 1.5rem; }
.tab-btn {
  padding: 0.55rem 1.1rem;
  border: none; background: none;
  cursor: pointer;
  font-size: 0.875rem; font-weight: 500;
  color: var(--text-muted);
  border-bottom: 2px solid transparent;
  margin-bottom: -2px;
}
.tab-btn.active { color: var(--accent); border-bottom-color: var(--accent); font-weight: 700; }

.category-section { margin-bottom: 2rem; }

.category-title {
  font-size: 1rem;
  font-weight: 700;
  color: var(--text-secondary);
  margin: 0 0 0.75rem;
  padding-bottom: 0.4rem;
  border-bottom: 2px solid var(--border);
}

.table-wrapper { overflow-x: auto; }

.skill-table { width: 100%; border-collapse: collapse; font-size: 0.85rem; }

.skill-table th {
  background: var(--surface-muted);
  font-weight: 700;
  color: var(--text-secondary);
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--border);
  white-space: nowrap;
}

.skill-col { text-align: left; min-width: 140px; }
.user-col { text-align: center; min-width: 60px; }

.skill-table td { padding: 0.4rem 0.5rem; border: 1px solid var(--border); text-align: center; font-weight: 700; }
td.skill-col { text-align: left; color: var(--text-secondary); font-weight: 400; background: var(--surface); }

.level-cell { font-size: 0.9rem; font-weight: 700; }
.empty-cell { color: var(--border-strong); font-weight: 400; font-size: 0.85rem; }

.lv-1 { background: #f0fdf4; color: #166534; }
.lv-2 { background: #ecfdf5; color: #065f46; }
.lv-3 { background: #dbeafe; color: #1d4ed8; }
.lv-4 { background: #e0e7ff; color: #3730a3; }
.lv-5 { background: #fef3c7; color: #92400e; }
.lv-6 { background: #fff7ed; color: #b45309; }
.lv-7 { background: #fef2f2; color: #991b1b; }

.legend { margin-top: 2rem; padding: 1rem 1.25rem; background: var(--surface-muted); border-radius: 8px; border: 1px solid var(--border); }
.legend h3 { font-size: 0.85rem; font-weight: 700; color: var(--text-secondary); margin: 0 0 0.5rem; }
.legend-items { display: flex; gap: 0.75rem; flex-wrap: wrap; }
.legend-item { font-size: 0.8rem; font-weight: 600; padding: 0.2rem 0.6rem; border-radius: 4px; }

.empty-note { color: var(--text-muted); font-size: 0.9rem; padding: 1rem 0; text-align: center; }
</style>
