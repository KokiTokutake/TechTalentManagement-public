<template>
  <div class="checkin-panel">
    <h3 class="panel-title">進捗チェックイン</h3>

    <div class="checkin-form">
      <div v-if="kpi" class="form-row">
        <label class="form-label">現在値（目標: {{ kpi.targetValue }} {{ kpi.unit }}）</label>
        <input
          v-model.number="form.currentValue"
          type="number"
          class="form-input"
          :placeholder="`0 〜 ${kpi.targetValue}`"
          data-testid="checkin-current-value-input"
        />
      </div>
      <div class="form-row">
        <label class="form-label">コメント</label>
        <textarea
          v-model="form.comment"
          class="form-textarea"
          rows="2"
          placeholder="進捗の状況や気づきを入力"
          data-testid="checkin-comment-input"
        />
      </div>
      <p v-if="errorMsg" class="error-text" role="alert">{{ errorMsg }}</p>
      <button
        class="btn-save"
        :disabled="saving"
        data-testid="checkin-save-button"
        @click="handleSave"
      >
        {{ saving ? '保存中…' : '記録する' }}
      </button>
    </div>

    <div v-if="checkins.length > 0" class="history">
      <h4 class="history-title">チェックイン履歴</h4>
      <ul class="history-list" data-testid="checkin-history-list">
        <li
          v-for="c in checkins"
          :key="c.id"
          class="history-item"
          data-testid="checkin-history-item"
        >
          <div class="history-header">
            <span class="history-date">{{ formatDate(c.checkedInAt) }}</span>
            <GoalProgressBadge
              v-if="c.progressPercent !== null"
              :progress-percent="c.progressPercent"
              data-testid="checkin-progress-percent"
            />
          </div>
          <div v-if="c.currentValue !== null && kpi" class="history-value">
            {{ c.currentValue }} / {{ kpi.targetValue }} {{ kpi.unit }}
          </div>
          <p v-if="c.comment" class="history-comment">{{ c.comment }}</p>
        </li>
      </ul>
    </div>
    <p v-else class="no-history">チェックイン記録はまだありません。</p>
  </div>
</template>

<script setup lang="ts">
import type { GoalKpiDto, GoalCheckinDto } from '~/types/goalKpi'
import GoalProgressBadge from '~/components/goals/GoalProgressBadge.vue'

const props = defineProps<{
  goalId: string
  kpi: GoalKpiDto | null
  checkins: GoalCheckinDto[]
}>()

const emit = defineEmits<{ saved: [] }>()

const store = useGoalKpiStore()

const form = reactive({
  currentValue: null as number | null,
  comment: '',
})

const saving = ref(false)
const errorMsg = ref<string | null>(null)

async function handleSave() {
  saving.value = true
  errorMsg.value = null
  try {
    await store.addCheckin(props.goalId, {
      currentValue: props.kpi ? (form.currentValue ?? null) : null,
      comment: form.comment || null,
    })
    form.currentValue = null
    form.comment = ''
    emit('saved')
  } catch (e: any) {
    errorMsg.value = e?.data?.error ?? 'チェックインの保存に失敗しました。'
  } finally {
    saving.value = false
  }
}

function formatDate(isoStr: string) {
  return new Date(isoStr).toLocaleString('ja-JP', {
    year: 'numeric', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit',
  })
}
</script>

<style scoped>
.checkin-panel {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 1rem;
  margin-top: 0.75rem;
}
.panel-title {
  font-size: 0.9rem;
  font-weight: 700;
  margin: 0 0 0.75rem;
}
.checkin-form { margin-bottom: 1rem; }
.form-row { margin-bottom: 0.6rem; }
.form-label {
  display: block;
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--text-secondary);
  margin-bottom: 0.3rem;
}
.form-input, .form-textarea {
  width: 100%;
  padding: 0.4rem 0.6rem;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  font-size: 0.875rem;
  box-sizing: border-box;
  resize: vertical;
}
.error-text { color: var(--danger-text); font-size: 0.8rem; margin: 0.25rem 0; }
.btn-save {
  background: var(--accent);
  color: #fff;
  border: none;
  border-radius: var(--radius-sm);
  padding: 0.4rem 1.2rem;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
}
.btn-save:disabled { opacity: 0.5; cursor: not-allowed; }
.history-title {
  font-size: 0.875rem;
  font-weight: 700;
  color: var(--text-secondary);
  margin: 0 0 0.5rem;
}
.history-list { list-style: none; padding: 0; margin: 0; }
.history-item {
  border-bottom: 1px solid var(--border);
  padding: 0.6rem 0;
}
.history-item:last-child { border-bottom: none; }
.history-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.2rem;
}
.history-date { font-size: 0.8rem; color: var(--text-muted); }
.history-value { font-size: 0.875rem; font-weight: 600; color: var(--text); }
.history-comment { font-size: 0.85rem; color: var(--text-muted); margin: 0.2rem 0 0; white-space: pre-wrap; }
.no-history { font-size: 0.875rem; color: var(--text-muted); }
</style>
