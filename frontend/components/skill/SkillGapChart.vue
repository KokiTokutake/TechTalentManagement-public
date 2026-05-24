<template>
  <div class="gap-chart">
    <div v-if="!items.length" class="empty-note">ギャップデータがありません。</div>
    <div v-else>
      <div
        v-for="item in items"
        :key="item.id"
        class="gap-row"
        :data-testid="`skill-gap-row-${item.id}`"
        :class="{ attention: item.requiresAttention }"
      >
        <div class="gap-label" :title="item.name">{{ item.name }}</div>
        <div class="gap-bars">
          <div class="bar-wrap">
            <span class="bar-label">自己</span>
            <div class="bar-track">
              <div class="bar self" :style="{ width: barWidth(item.selfLevel) }"></div>
            </div>
            <span class="bar-val">{{ item.selfLevel ?? '—' }}</span>
          </div>
          <div class="bar-wrap">
            <span class="bar-label">上長</span>
            <div class="bar-track">
              <div class="bar manager" :style="{ width: barWidth(item.managerLevel) }"></div>
            </div>
            <span class="bar-val">{{ item.managerLevel ?? '—' }}</span>
          </div>
        </div>
        <div
          class="gap-value"
          :class="gapClass(item.gap)"
          :data-testid="`skill-gap-value-${item.id}`"
        >
          <template v-if="item.gap !== null">
            Δ{{ item.gap > 0 ? '+' : '' }}{{ item.gap }}
          </template>
          <template v-else>—</template>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
interface GapItem {
  id: string
  name: string
  selfLevel: number | null
  managerLevel: number | null
  gap: number | null
  requiresAttention: boolean
}

const props = defineProps<{
  items: GapItem[]
  maxLevel?: number
}>()

const maxLv = computed(() => props.maxLevel ?? 7)

function barWidth(level: number | null): string {
  if (!level) return '0%'
  return `${(level / maxLv.value) * 100}%`
}

function gapClass(gap: number | null): string {
  if (gap === null) return 'gap-none'
  if (Math.abs(gap) >= 2) return 'gap-large'
  if (gap !== 0) return 'gap-small'
  return 'gap-zero'
}
</script>

<style scoped>
.gap-row {
  display: grid;
  grid-template-columns: 160px 1fr 56px;
  align-items: center;
  gap: 0.75rem;
  padding: 0.6rem 0.75rem;
  border-radius: var(--radius);
  border: 1px solid var(--border);
  margin-bottom: 0.5rem;
  background: var(--surface);
}

.gap-row.attention {
  border-color: color-mix(in srgb, var(--danger-text) 30%, transparent);
  background: color-mix(in srgb, var(--danger-text) 4%, var(--surface));
}

.gap-label {
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--text-secondary);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.gap-bars {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.bar-wrap {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.75rem;
}

.bar-label {
  width: 24px;
  color: var(--text-muted);
  flex-shrink: 0;
}

.bar-track {
  flex: 1;
  height: 8px;
  background: var(--surface-muted);
  border-radius: 4px;
  overflow: hidden;
}

.bar {
  height: 100%;
  border-radius: 4px;
  transition: width 0.3s ease;
}

.bar.self { background: var(--accent); }
.bar.manager { background: #8b5cf6; }

.bar-val {
  width: 20px;
  text-align: right;
  font-weight: 700;
  color: var(--text-secondary);
  font-size: 0.75rem;
}

.gap-value {
  text-align: center;
  font-size: 0.8rem;
  font-weight: 700;
  padding: 0.2rem 0.3rem;
  border-radius: var(--radius-sm);
}

.gap-none { color: var(--text-muted); }
.gap-zero { color: var(--text-muted); background: var(--border); }
.gap-small { color: #92400e; background: #fef3c7; }
.gap-large { color: var(--danger-text); background: color-mix(in srgb, var(--danger-text) 12%, transparent); }

.empty-note { color: var(--text-muted); font-size: 0.9rem; padding: 1rem 0; text-align: center; }
</style>
