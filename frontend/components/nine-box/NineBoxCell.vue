<template>
  <div
    class="cell"
    :class="`cell--${performanceLevel.toLowerCase()}-${potentialLevel.toLowerCase()}`"
    :style="{ '--cell-color': color }"
    :data-testid="`nine-box-cell-${performanceLevel.toLowerCase()}-${potentialLevel.toLowerCase()}`"
  >
    <div class="cell__header">
      <span class="cell__label">{{ label }}</span>
      <span
        class="cell__count"
        :data-testid="`nine-box-count-${performanceLevel.toLowerCase()}-${potentialLevel.toLowerCase()}`"
      >{{ count }}</span>
    </div>

    <div class="cell__badges">
      <NineBoxTalentBadge
        v-for="talent in talents"
        :key="talent.talentId"
        :talent="talent"
        @click="$emit('talent-click', talent)"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { NineBoxTalent } from '~/types/nineBox'

defineProps<{
  performanceLevel: string
  potentialLevel: string
  label: string
  color: string
  count: number
  talents: NineBoxTalent[]
}>()

defineEmits<{
  'talent-click': [talent: NineBoxTalent]
}>()
</script>

<style scoped>
.cell {
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 0.5rem;
  min-height: 100px;
  background: var(--surface);
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  position: relative;
  transition: box-shadow 0.15s;
  border-top: 3px solid var(--cell-color);
}

.cell__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.25rem;
}

.cell__label {
  font-size: 0.7rem;
  font-weight: 700;
  color: var(--cell-color);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.cell__count {
  font-size: 0.65rem;
  font-weight: 700;
  background: var(--cell-color);
  color: #fff;
  border-radius: 999px;
  min-width: 18px;
  height: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  padding: 0 4px;
}

.cell__badges {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  min-height: 40px;
}
</style>
