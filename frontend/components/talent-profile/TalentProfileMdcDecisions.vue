<template>
  <section class="card" data-testid="talent-profile-mdc-decisions-section">
    <h2 class="section-title">MDC決定履歴</h2>

    <div v-if="loading" class="loading-note">読み込み中…</div>

    <div v-else-if="decisions.length === 0" class="empty-note">
      MDC決定事項はありません
    </div>

    <div v-else class="decisions-list">
      <div
        v-for="decision in decisions"
        :key="decision.id"
        class="decision-row"
      >
        <div class="decision-row-header">
          <span class="type-badge" :class="typeClass(decision.decisionType)">
            {{ DECISION_TYPE_LABELS[decision.decisionType] }}
          </span>
          <span class="decided-at">{{ decision.decidedAt }}</span>
        </div>
        <p class="decision-content">{{ decision.content }}</p>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import type { MdcDecisionDto, DecisionType } from '~/types/mdcDecision'
import { DECISION_TYPE_LABELS } from '~/types/mdcDecision'

defineProps<{
  decisions: MdcDecisionDto[]
  loading: boolean
}>()

function typeClass(type: DecisionType): string {
  const map: Record<DecisionType, string> = {
    KeyPersonAssign: 'type-assign',
    KeyPersonRemove: 'type-remove',
    DevelopmentPolicyChange: 'type-policy',
    TransferConsideration: 'type-transfer',
    Other: 'type-other',
  }
  return map[type]
}
</script>

<style scoped>
.section-title {
  font-size: 1rem;
  font-weight: 700;
  color: var(--text);
  margin: 0 0 1rem;
}

.loading-note, .empty-note {
  font-size: 0.875rem;
  color: var(--text-muted);
}

.decisions-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.decision-row {
  padding: 0.75rem 0;
  border-bottom: 1px solid var(--surface-muted);
}
.decision-row:last-child { border-bottom: none; }

.decision-row-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.4rem;
  flex-wrap: wrap;
}

.decided-at {
  font-size: 0.75rem;
  color: var(--text-muted);
  margin-left: auto;
}

.type-badge {
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
  margin: 0;
  white-space: pre-wrap;
  word-break: break-word;
}
</style>
