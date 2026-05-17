<template>
  <div class="goal-card">
    <div class="card-header">
      <span class="order">{{ goal.displayOrder }}</span>
      <span class="title">{{ goal.title }}</span>
      <div class="card-actions">
        <button class="btn-icon" title="編集" @click="$emit('edit', goal)">✏️</button>
        <button class="btn-icon btn-danger" title="削除" @click="confirmDelete">🗑</button>
      </div>
    </div>
    <p v-if="goal.description" class="description">{{ goal.description }}</p>
  </div>
</template>

<script setup lang="ts">
import type { Goal } from '~/types/goal'

const props = defineProps<{ goal: Goal }>()
const emit = defineEmits<{ edit: [Goal]; delete: [string] }>()

function confirmDelete() {
  // eslint-disable-next-line no-alert
  if (window.confirm('この目標を削除しますか？')) {
    emit('delete', props.goal.id)
  }
}
</script>

<style scoped>
.goal-card {
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 1rem 1.25rem;
}

.card-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.order {
  flex-shrink: 0;
  width: 1.75rem;
  height: 1.75rem;
  border-radius: 50%;
  background: #eff6ff;
  color: #2563eb;
  font-size: 0.8rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
}

.title {
  flex: 1;
  font-weight: 600;
  font-size: 0.95rem;
  color: #111827;
}

.card-actions {
  display: flex;
  gap: 0.25rem;
  opacity: 0;
  transition: opacity 0.15s;
}

.goal-card:hover .card-actions {
  opacity: 1;
}

.btn-icon {
  background: none;
  border: none;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 4px;
  font-size: 1rem;
  line-height: 1;
}

.btn-icon:hover {
  background: #f3f4f6;
}

.btn-danger:hover {
  background: #fee2e2;
}

.description {
  margin: 0.5rem 0 0 2.5rem;
  font-size: 0.875rem;
  color: #6b7280;
  white-space: pre-wrap;
}
</style>
