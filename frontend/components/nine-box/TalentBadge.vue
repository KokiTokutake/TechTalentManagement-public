<template>
  <div
    class="talent-badge"
    :data-testid="`nine-box-talent-${talent.talentId}`"
    :class="{ 'talent-badge--hp': talent.isHighPotentialCandidate }"
    :title="talent.talentName"
    @click.stop="$emit('click', talent)"
  >
    <span class="talent-badge__initials">{{ talent.initials }}</span>
    <span
      v-if="talent.movement && talent.movement !== 'same'"
      class="talent-badge__movement"
      :class="`talent-badge__movement--${talent.movement}`"
      :data-testid="`nine-box-movement-${talent.talentId}`"
    >
      {{ talent.movement === 'up' ? '↑' : '↓' }}
    </span>
  </div>
</template>

<script setup lang="ts">
import type { NineBoxTalent } from '~/types/nineBox'

defineProps<{
  talent: NineBoxTalent
}>()

defineEmits<{
  click: [talent: NineBoxTalent]
}>()
</script>

<style scoped>
.talent-badge {
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--accent);
  color: #fff;
  font-size: 0.7rem;
  font-weight: 700;
  cursor: pointer;
  transition: transform 0.15s, box-shadow 0.15s;
  border: 2px solid rgba(255,255,255,0.4);
}

.talent-badge:hover {
  transform: scale(1.15);
  box-shadow: 0 2px 8px rgba(0,0,0,0.2);
  z-index: 1;
}

.talent-badge--hp {
  box-shadow: 0 0 0 2px #f59e0b;
}

.talent-badge__initials {
  line-height: 1;
  letter-spacing: -0.5px;
}

.talent-badge__movement {
  position: absolute;
  top: -6px;
  right: -6px;
  font-size: 0.65rem;
  font-weight: 800;
  line-height: 1;
  width: 14px;
  height: 14px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.talent-badge__movement--up {
  background: #10b981;
  color: #fff;
}

.talent-badge__movement--down {
  background: #ef4444;
  color: #fff;
}
</style>
