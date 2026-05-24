<template>
  <div class="heatmap-wrap" data-testid="competency-heatmap">
    <div v-if="!categories.length" class="empty-note">コンピテンシーヒートマップデータがありません。</div>

    <div v-for="cat in categories" :key="cat.category" class="heatmap-category">
      <h3 class="cat-title">{{ categoryLabel(cat.category) }}</h3>
      <div class="heatmap-table-wrap">
        <table class="heatmap-table">
          <thead>
            <tr>
              <th class="comp-col">コンピテンシー</th>
              <th v-for="avg in (cat.competencies[0]?.averages ?? [])" :key="avg.label" class="avg-col">
                {{ avg.label }}
              </th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="comp in cat.competencies" :key="comp.competencyId">
              <td class="comp-name">{{ comp.competencyName }}</td>
              <td
                v-for="avg in comp.averages"
                :key="avg.label"
                class="avg-cell"
                :class="heatClass(avg.avgLevel)"
                :title="`${avg.label}: ${avg.avgLevel.toFixed(1)} (n=${avg.count})`"
              >
                {{ avg.avgLevel.toFixed(1) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- 凡例 -->
    <div class="legend">
      <span class="legend-label">低 →</span>
      <span v-for="i in 5" :key="i" class="legend-chip" :class="`heat-${i}`">{{ i }}</span>
      <span class="legend-label">→ 高</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { CompetencyHeatmapCategory, CompetencyCategory } from '~/types/skill'

defineProps<{
  categories: CompetencyHeatmapCategory[]
}>()

const catLabels: Record<CompetencyCategory, string> = {
  Technology: 'テクノロジー',
  Methodology: 'メソドロジー',
  BusinessIndustry: 'ビジネス・インダストリー',
  Common: '共通',
}

function categoryLabel(cat: CompetencyCategory): string {
  return catLabels[cat] ?? cat
}

function heatClass(avg: number): string {
  const lv = Math.round(avg)
  return `heat-${Math.min(Math.max(lv, 1), 5)}`
}
</script>

<style scoped>
.heatmap-wrap { display: flex; flex-direction: column; gap: 1.5rem; }

.heatmap-category {}

.cat-title {
  font-size: 0.9rem;
  font-weight: 700;
  color: var(--text-secondary);
  margin: 0 0 0.6rem;
  padding-bottom: 0.3rem;
  border-bottom: 2px solid var(--border);
}

.heatmap-table-wrap { overflow-x: auto; }

.heatmap-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.82rem;
}

.heatmap-table th {
  background: var(--surface-muted);
  font-weight: 700;
  color: var(--text-secondary);
  padding: 0.45rem 0.6rem;
  border: 1px solid var(--border);
  white-space: nowrap;
}

.comp-col { text-align: left; min-width: 160px; }
.avg-col { text-align: center; min-width: 80px; }

.heatmap-table td {
  padding: 0.35rem 0.5rem;
  border: 1px solid var(--border);
}

.comp-name { color: var(--text-secondary); font-weight: 400; }

.avg-cell {
  text-align: center;
  font-weight: 700;
  cursor: default;
}

/* ヒートマップカラー（1〜5） */
.heat-1 { background: #f1f5f9; color: #475569; }
.heat-2 { background: #dbeafe; color: #1d4ed8; }
.heat-3 { background: #bbf7d0; color: #166534; }
.heat-4 { background: #fde68a; color: #92400e; }
.heat-5 { background: #fca5a5; color: #991b1b; }

.legend {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  margin-top: 0.75rem;
}

.legend-label { font-size: 0.75rem; color: var(--text-muted); }

.legend-chip {
  font-size: 0.75rem;
  font-weight: 700;
  padding: 0.15rem 0.5rem;
  border-radius: 3px;
  min-width: 28px;
  text-align: center;
}

.empty-note { color: var(--text-muted); font-size: 0.9rem; padding: 1.5rem 0; text-align: center; }
</style>
