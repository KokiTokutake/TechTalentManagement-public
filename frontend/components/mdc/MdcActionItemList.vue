<template>
  <div data-testid="mdc-action-item-list">
    <div v-if="overdueCount > 0" class="overdue-banner" data-testid="mdc-overdue-badge">
      期限超過 {{ overdueCount }} 件
    </div>

    <div v-if="items.length === 0" class="empty-note">アクションアイテムなし</div>

    <div v-for="item in sortedItems" :key="item.id" class="ai-row" :class="{ overdue: isOverdue(item) }">
      <div class="ai-main">
        <span class="ai-desc">{{ item.description }}</span>
        <div class="ai-meta">
          <span class="ai-assignee">担当: {{ item.assigneeName }}</span>
          <span v-if="item.dueDate" class="ai-due" :class="{ 'due-overdue': isOverdue(item) }">
            期限: {{ item.dueDate }}
          </span>
          <span v-else class="ai-due">期限: 未定</span>
        </div>
      </div>

      <select
        v-if="canEdit"
        :value="item.status"
        class="status-select"
        :class="statusClass(item.status)"
        :data-testid="`mdc-action-item-status-${item.id}`"
        @change="onStatusChange(item.id, ($event.target as HTMLSelectElement).value as MdcActionItemStatus)"
      >
        <option v-for="(label, key) in ACTION_ITEM_STATUS_LABELS" :key="key" :value="key">
          {{ label }}
        </option>
      </select>

      <span v-else class="status-badge" :class="statusClass(item.status)">
        {{ ACTION_ITEM_STATUS_LABELS[item.status] }}
      </span>
    </div>

    <div v-if="canEdit" class="add-row">
      <button
        class="btn-ghost"
        data-testid="mdc-action-item-add-button"
        @click="$emit('add')"
      >
        + アクションアイテムを追加
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { MdcActionItemDto, MdcActionItemStatus } from '~/types/mdcDecision'
import { ACTION_ITEM_STATUS_LABELS } from '~/types/mdcDecision'

const props = defineProps<{
  items: MdcActionItemDto[]
  canEdit: boolean
}>()

const emit = defineEmits<{
  add: []
  statusChange: [id: string, status: MdcActionItemStatus]
}>()

const today = new Date().toISOString().slice(0, 10)

function isOverdue(item: MdcActionItemDto): boolean {
  if (!item.dueDate) return false
  if (item.status === 'Done' || item.status === 'Cancelled') return false
  return item.dueDate < today
}

const overdueCount = computed(() => props.items.filter(isOverdue).length)

const sortedItems = computed(() => {
  return [...props.items].sort((a, b) => {
    if (isOverdue(a) && !isOverdue(b)) return -1
    if (!isOverdue(a) && isOverdue(b)) return 1
    if (a.dueDate && b.dueDate) return a.dueDate.localeCompare(b.dueDate)
    if (a.dueDate && !b.dueDate) return -1
    if (!a.dueDate && b.dueDate) return 1
    return 0
  })
})

function statusClass(status: MdcActionItemStatus): string {
  const map: Record<MdcActionItemStatus, string> = {
    NotStarted: 'status-not-started',
    InProgress: 'status-in-progress',
    Done: 'status-done',
    Cancelled: 'status-cancelled',
  }
  return map[status]
}

function onStatusChange(id: string, status: MdcActionItemStatus) {
  emit('statusChange', id, status)
}
</script>

<style scoped>
.overdue-banner {
  background: #fef2f2;
  color: #dc2626;
  font-size: 0.8rem;
  font-weight: 700;
  padding: 0.35rem 0.75rem;
  border-radius: var(--radius-sm);
  margin-bottom: 0.75rem;
  border: 1px solid #fca5a5;
}

.ai-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.6rem 0;
  border-bottom: 1px solid var(--surface-muted);
}
.ai-row:last-of-type { border-bottom: none; }

.ai-row.overdue { background: #fff5f5; border-radius: var(--radius-sm); padding: 0.6rem 0.5rem; }

.ai-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  min-width: 0;
}

.ai-desc {
  font-size: 0.875rem;
  color: var(--text);
  word-break: break-word;
}

.ai-meta {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.ai-assignee, .ai-due {
  font-size: 0.75rem;
  color: var(--text-muted);
}

.due-overdue {
  color: #dc2626;
  font-weight: 600;
}

.status-select {
  font-size: 0.78rem;
  padding: 0.25rem 0.5rem;
  border-radius: 999px;
  border: 1px solid var(--border-strong);
  background: var(--surface);
  cursor: pointer;
  outline: none;
  flex-shrink: 0;
}

.status-badge {
  font-size: 0.72rem;
  font-weight: 600;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  flex-shrink: 0;
}

.status-not-started { color: #6b7280; border-color: #d1d5db; background: #f9fafb; }
.status-in-progress { color: #1d4ed8; border-color: #93c5fd; background: #eff6ff; }
.status-done { color: #15803d; border-color: #86efac; background: #f0fdf4; }
.status-cancelled { color: #9ca3af; border-color: #e5e7eb; background: #f9fafb; text-decoration: line-through; }

.add-row {
  padding: 0.5rem 0 0;
}

.empty-note {
  font-size: 0.875rem;
  color: var(--text-muted);
  padding: 0.5rem 0;
}
</style>
