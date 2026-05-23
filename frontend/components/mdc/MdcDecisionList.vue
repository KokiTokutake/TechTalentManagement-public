<template>
  <section class="card" data-testid="mdc-decision-list">
    <div class="section-header">
      <h2 class="section-title">
        決定事項
        <span class="count-badge">{{ decisions.length }} 件</span>
      </h2>
      <button
        v-if="canEdit"
        class="btn-primary btn-sm"
        data-testid="mdc-decision-add-button"
        @click="showDecisionForm = true"
      >
        + 追加
      </button>
    </div>

    <div v-if="decisions.length === 0" class="empty-note">決定事項なし</div>

    <div v-for="decision in decisions" :key="decision.id" class="decision-card">
      <div class="decision-header">
        <span class="decision-type-badge" :class="typeClass(decision.decisionType)">
          {{ DECISION_TYPE_LABELS[decision.decisionType] }}
        </span>
        <span class="decision-date">{{ decision.decidedAt }}</span>
        <button
          v-if="canEdit"
          class="btn-ghost btn-xs edit-btn"
          @click="openEdit(decision)"
        >
          編集
        </button>
      </div>

      <p class="decision-content">{{ decision.content }}</p>

      <div v-if="decision.talents.length > 0" class="talent-chips">
        <span v-for="t in decision.talents" :key="t.talentId" class="chip">
          {{ t.talentName }}
        </span>
      </div>

      <div class="action-items-wrap">
        <h3 class="ai-title">アクションアイテム</h3>
        <MdcActionItemList
          :items="decision.actionItems"
          :can-edit="canEdit"
          @add="openAddActionItem(decision.id)"
          @status-change="onStatusChange"
        />
      </div>
    </div>

    <MdcDecisionForm
      v-if="showDecisionForm"
      :initial="editingDecision ?? undefined"
      :talents="talents"
      @saved="onDecisionSaved"
      @cancel="closeDecisionForm"
    />

    <MdcActionItemForm
      v-if="showActionItemForm"
      :users="allUsers"
      @saved="onActionItemSaved"
      @cancel="showActionItemForm = false"
    />
  </section>
</template>

<script setup lang="ts">
import type { AppUser } from '~/types/user'
import type { MdcDecisionDto, MdcActionItemStatus, CreateDecisionRequest, CreateActionItemRequest } from '~/types/mdcDecision'
import { DECISION_TYPE_LABELS } from '~/types/mdcDecision'
import MdcDecisionForm from './MdcDecisionForm.vue'
import MdcActionItemList from './MdcActionItemList.vue'
import MdcActionItemForm from './MdcActionItemForm.vue'

const props = defineProps<{
  decisions: MdcDecisionDto[]
  sessionId: string
  canEdit: boolean
  allUsers: AppUser[]
}>()

const emit = defineEmits<{
  decisionCreate: [sessionId: string, data: CreateDecisionRequest]
  decisionUpdate: [id: string, data: CreateDecisionRequest]
  actionItemCreate: [decisionId: string, data: CreateActionItemRequest]
  statusChange: [id: string, status: MdcActionItemStatus]
}>()

const talents = computed(() =>
  props.allUsers.filter(u => u.role === 'Talent'),
)

const showDecisionForm = ref(false)
const editingDecision = ref<MdcDecisionDto | null>(null)
const showActionItemForm = ref(false)
const activeDecisionId = ref<string>('')

function openEdit(decision: MdcDecisionDto) {
  editingDecision.value = decision
  showDecisionForm.value = true
}

function closeDecisionForm() {
  showDecisionForm.value = false
  editingDecision.value = null
}

function onDecisionSaved(data: CreateDecisionRequest) {
  if (editingDecision.value) {
    emit('decisionUpdate', editingDecision.value.id, data)
  }
  else {
    emit('decisionCreate', props.sessionId, data)
  }
  closeDecisionForm()
}

function openAddActionItem(decisionId: string) {
  activeDecisionId.value = decisionId
  showActionItemForm.value = true
}

function onActionItemSaved(data: CreateActionItemRequest) {
  emit('actionItemCreate', activeDecisionId.value, data)
  showActionItemForm.value = false
}

function onStatusChange(id: string, status: MdcActionItemStatus) {
  emit('statusChange', id, status)
}

function typeClass(type: string): string {
  const map: Record<string, string> = {
    KeyPersonAssign: 'type-assign',
    KeyPersonRemove: 'type-remove',
    DevelopmentPolicyChange: 'type-policy',
    TransferConsideration: 'type-transfer',
    Other: 'type-other',
  }
  return map[type] ?? 'type-other'
}
</script>

<style scoped>
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

.empty-note {
  font-size: 0.875rem;
  color: var(--text-muted);
}

.decision-card {
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  padding: 1rem;
  margin-bottom: 1rem;
  background: var(--surface);
}
.decision-card:last-child { margin-bottom: 0; }

.decision-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
  flex-wrap: wrap;
}

.decision-date {
  font-size: 0.75rem;
  color: var(--text-muted);
  margin-left: auto;
}

.edit-btn { margin-left: 0.25rem; }

.decision-type-badge {
  font-size: 0.72rem;
  font-weight: 700;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
}

.type-assign { background: #fef3c7; color: #92400e; }
.type-remove { background: #fee2e2; color: #991b1b; }
.type-policy { background: #ede9fe; color: #5b21b6; }
.type-transfer { background: #e0f2fe; color: #0369a1; }
.type-other { background: #f3f4f6; color: #374151; }

.decision-content {
  font-size: 0.875rem;
  color: var(--text);
  line-height: 1.6;
  margin: 0 0 0.75rem;
  white-space: pre-wrap;
  word-break: break-word;
}

.talent-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
  margin-bottom: 0.75rem;
}

.chip {
  font-size: 0.72rem;
  padding: 0.2rem 0.6rem;
  background: var(--surface-muted);
  border-radius: 999px;
  color: var(--text-secondary);
}

.action-items-wrap {
  border-top: 1px solid var(--surface-muted);
  padding-top: 0.75rem;
}

.ai-title {
  font-size: 0.8rem;
  font-weight: 700;
  color: var(--text-secondary);
  margin: 0 0 0.5rem;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.btn-sm { font-size: 0.82rem; padding: 0.3rem 0.75rem; }
.btn-xs { font-size: 0.75rem; padding: 0.15rem 0.5rem; }
</style>
