<template>
  <span class="badge" :class="isDisclosed ? 'disclosed' : 'undisclosed'">
    {{ isDisclosed ? '開示済み' : '未開示' }}
  </span>
</template>

<script setup lang="ts">
defineProps<{ isDisclosed: boolean }>()
</script>

<style scoped>
.badge {
  font-size: 0.75rem;
  font-weight: 700;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
}
.disclosed   { background: #d1fae5; color: #065f46; }
.undisclosed { background: #f3f4f6; color: #6b7280; }
</style>
