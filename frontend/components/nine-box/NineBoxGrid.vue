<template>
  <div class="grid-wrap" data-testid="nine-box-grid">
    <!-- 上部：ポテンシャル軸ラベル -->
    <div class="grid-axis-header">
      <div class="grid-axis-corner">
        <span class="grid-axis-label-text">↑ パフォーマンス</span>
      </div>
      <div class="grid-axis-potential">
        <span v-for="pot in POTENTIAL_ORDER" :key="pot" class="grid-axis-pot-label">
          {{ potentialLabel(pot) }}
        </span>
        <span class="grid-axis-arrow">ポテンシャル →</span>
      </div>
    </div>

    <!-- グリッド本体 -->
    <div class="grid-body">
      <!-- 行ラベル（パフォーマンス軸） -->
      <div class="grid-row-labels">
        <div v-for="perf in PERFORMANCE_ORDER" :key="perf" class="grid-row-label">
          {{ performanceLabel(perf) }}
        </div>
      </div>

      <!-- 9セル -->
      <div class="grid-cells">
        <template v-for="perf in PERFORMANCE_ORDER" :key="perf">
          <NineBoxCell
            v-for="pot in POTENTIAL_ORDER"
            :key="`${perf}-${pot}`"
            :performance-level="perf"
            :potential-level="pot"
            :label="getCell(perf, pot)?.label ?? ''"
            :color="getCell(perf, pot)?.color ?? '#e5e7eb'"
            :count="getCell(perf, pot)?.count ?? 0"
            :talents="getCell(perf, pot)?.talents ?? []"
            @talent-click="$emit('talent-click', $event)"
          />
        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { NineBoxMapResponse, NineBoxTalent } from '~/types/nineBox'

const props = defineProps<{
  mapData: NineBoxMapResponse
}>()

defineEmits<{
  'talent-click': [talent: NineBoxTalent]
}>()

const PERFORMANCE_ORDER = ['High', 'Mid', 'Low'] as const
const POTENTIAL_ORDER = ['Low', 'Mid', 'High'] as const

function getCell(perf: string, pot: string) {
  return props.mapData.cells.find(
    c => c.performanceLevel === perf && c.potentialLevel === pot,
  )
}

function performanceLabel(level: string): string {
  return { High: 'High', Mid: 'Mid', Low: 'Low' }[level] ?? level
}

function potentialLabel(level: string): string {
  return { High: 'High', Mid: 'Mid', Low: 'Low' }[level] ?? level
}
</script>

<style scoped>
.grid-wrap {
  display: flex;
  flex-direction: column;
  gap: 0;
  overflow: auto;
}

.grid-axis-header {
  display: flex;
  align-items: flex-end;
  margin-bottom: 4px;
}

.grid-axis-corner {
  width: 52px;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
}

.grid-axis-label-text {
  font-size: 0.65rem;
  color: var(--text-muted);
  writing-mode: vertical-rl;
  transform: rotate(180deg);
  white-space: nowrap;
}

.grid-axis-potential {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 4px;
  flex: 1;
  position: relative;
}

.grid-axis-pot-label {
  font-size: 0.7rem;
  font-weight: 600;
  color: var(--text-secondary);
  text-align: center;
}

.grid-axis-arrow {
  position: absolute;
  bottom: -14px;
  right: 0;
  font-size: 0.6rem;
  color: var(--text-muted);
}

.grid-body {
  display: flex;
  gap: 4px;
}

.grid-row-labels {
  display: flex;
  flex-direction: column;
  gap: 4px;
  width: 52px;
  flex-shrink: 0;
}

.grid-row-label {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.7rem;
  font-weight: 600;
  color: var(--text-secondary);
  writing-mode: vertical-rl;
  transform: rotate(180deg);
  min-height: 100px;
}

.grid-cells {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  grid-template-rows: repeat(3, 1fr);
  gap: 4px;
  flex: 1;
}
</style>
