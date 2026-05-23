<template>
  <section class="card carryover-section" data-testid="mdc-carryover-section">
    <div class="section-header">
      <h2 class="section-title">
        前回の積み残し
        <span
          class="count-badge"
          :class="{ 'count-badge-alert': items.length > 0 }"
          data-testid="mdc-carryover-count"
        >
          {{ items.length }} 件
        </span>
      </h2>
    </div>

    <div v-if="items.length === 0" class="empty-note">前回の積み残しはありません</div>

    <div v-for="item in items" :key="item.id" class="carryover-row">
      <div class="carryover-main">
        <span class="carryover-desc">{{ item.description }}</span>
        <div class="carryover-meta">
          <span class="meta-item">担当: {{ item.assigneeName }}</span>
          <span v-if="item.dueDate" class="meta-item due-overdue">期限: {{ item.dueDate }}</span>
          <span class="meta-item status-label">{{ ACTION_ITEM_STATUS_LABELS[item.status] }}</span>
        </div>
      </div>

      <div v-if="canEdit" class="carryover-actions">
        <button
          class="btn-ghost btn-xs btn-done"
          :data-testid="`mdc-carryover-done-${item.id}`"
          :disabled="updating === item.id"
          @click="onDone(item.id)"
        >
          完了
        </button>
        <button
          class="btn-ghost btn-xs btn-cancel"
          :data-testid="`mdc-carryover-cancel-${item.id}`"
          :disabled="updating === item.id"
          @click="onCancel(item.id)"
        >
          キャンセル
        </button>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import type { MdcActionItemDto, MdcActionItemStatus } from '~/types/mdcDecision'
import { ACTION_ITEM_STATUS_LABELS } from '~/types/mdcDecision'

defineProps<{
  items: MdcActionItemDto[]
  canEdit: boolean
}>()

const emit = defineEmits<{
  statusChange: [id: string, status: MdcActionItemStatus]
}>()

const updating = ref<string>('')

function onDone(id: string) {
  updating.value = id
  emit('statusChange', id, 'Done')
  updating.value = ''
}

function onCancel(id: string) {
  updating.value = id
  emit('statusChange', id, 'Cancelled')
  updating.value = ''
}
</script>

<style scoped>
.carryover-section {
  border-left: 4px solid #f59e0b;
}

.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1rem;
}

.section-title {
  font-size: 1rem;
  font-weight: 700;
  color: var(--text);
  margin: 0;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.count-badge {
  font-size: 0.75rem;
  font-weight: 400;
  color: var(--text-secondary);
  background: var(--surface-muted);
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
}

.count-badge-alert {
  background: #fef3c7;
  color: #92400e;
  font-weight: 700;
}

.empty-note {
  font-size: 0.875rem;
  color: var(--text-muted);
}

.carryover-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.6rem 0;
  border-bottom: 1px solid var(--surface-muted);
}
.carryover-row:last-child { border-bottom: none; }

.carryover-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  min-width: 0;
}

.carryover-desc {
  font-size: 0.875rem;
  color: var(--text);
  word-break: break-word;
}

.carryover-meta {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.meta-item {
  font-size: 0.75rem;
  color: var(--text-muted);
}

.due-overdue {
  color: #dc2626;
  font-weight: 600;
}

.status-label {
  font-weight: 600;
  color: var(--text-secondary);
}

.carryover-actions {
  display: flex;
  gap: 0.4rem;
  flex-shrink: 0;
}

.btn-xs { font-size: 0.75rem; padding: 0.2rem 0.6rem; }

.btn-done {
  color: #15803d;
  border: 1px solid #86efac;
  border-radius: var(--radius-sm);
}
.btn-done:hover { background: #f0fdf4; }

.btn-cancel {
  color: #9ca3af;
  border: 1px solid #e5e7eb;
  border-radius: var(--radius-sm);
}
.btn-cancel:hover { background: #f9fafb; }
</style>
