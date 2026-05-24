<template>
  <span
    v-if="progressPercent !== null"
    class="progress-badge"
    :class="colorClass"
    data-testid="goal-progress-badge"
  >
    {{ Math.round(progressPercent) }}%
  </span>
</template>

<script setup lang="ts">
const props = defineProps<{ progressPercent: number | null }>()

const colorClass = computed(() => {
  if (props.progressPercent === null) return ''
  if (props.progressPercent >= 80) return 'green'
  if (props.progressPercent >= 50) return 'yellow'
  return 'red'
})
</script>

<style scoped>
.progress-badge {
  font-size: 0.75rem;
  font-weight: 700;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
}
.green  { background: #d1fae5; color: #065f46; }
.yellow { background: #fef3c7; color: #92400e; }
.red    { background: #fee2e2; color: #991b1b; }
</style>
