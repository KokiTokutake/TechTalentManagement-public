<template>
  <div v-if="kpi" class="kpi-display">
    <div class="kpi-row">
      <span class="kpi-label">指標名</span>
      <span class="kpi-value">{{ kpi.metricName }}</span>
    </div>
    <div class="kpi-row">
      <span class="kpi-label">目標値</span>
      <span class="kpi-value">{{ kpi.targetValue }} {{ kpi.unit }}</span>
    </div>
    <div v-if="kpi.measurementMethod" class="kpi-row">
      <span class="kpi-label">測定方法</span>
      <span class="kpi-value">{{ kpi.measurementMethod }}</span>
    </div>
    <div v-if="kpi.dueDate" class="kpi-row">
      <span class="kpi-label">期限</span>
      <span class="kpi-value">{{ kpi.dueDate }}</span>
    </div>
  </div>
  <p v-else class="no-kpi">KPI未設定</p>
</template>

<script setup lang="ts">
import type { GoalKpiDto } from '~/types/goalKpi'
defineProps<{ kpi: GoalKpiDto | null }>()
</script>

<style scoped>
.kpi-display {
  background: #f8fafc;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  padding: 0.6rem 0.8rem;
  font-size: 0.875rem;
}
.kpi-row {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 0.25rem;
}
.kpi-row:last-child { margin-bottom: 0; }
.kpi-label { color: var(--text-muted); min-width: 5rem; }
.kpi-value { color: var(--text); font-weight: 500; }
.no-kpi { font-size: 0.875rem; color: var(--text-muted); }
</style>
