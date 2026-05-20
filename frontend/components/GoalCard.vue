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
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 0.875rem 1rem;
}

.card-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.order {
  flex-shrink: 0;
  width: 1.6rem;
  height: 1.6rem;
  border-radius: 50%;
  background: var(--accent-light);
  color: var(--accent);
  font-size: 0.75rem;
  font-weight: 700;
  font-family: var(--font-mono);
  display: flex;
  align-items: center;
  justify-content: center;
}

.title {
  flex: 1;
  font-weight: 600;
  font-size: 0.9rem;
  color: var(--text);
}

.card-actions {
  display: flex;
  gap: 0.25rem;
  opacity: 0;
}

.goal-card:hover .card-actions {
  opacity: 1;
}

.btn-icon {
  background: none;
  border: none;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: var(--radius-sm);
  font-size: 0.95rem;
  line-height: 1;
}

.btn-icon:hover { background: var(--surface-muted); }
.btn-danger:hover { background: var(--danger-bg); }

.description {
  margin: 0.4rem 0 0 2.35rem;
  font-size: 0.85rem;
  color: var(--text-muted);
  white-space: pre-wrap;
}
</style>
