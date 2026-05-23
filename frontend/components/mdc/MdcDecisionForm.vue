<template>
  <div class="modal-backdrop" @click.self="$emit('cancel')">
    <div class="modal" role="dialog" aria-label="決定事項を追加">
      <h2 class="modal-title">{{ isEdit ? '決定事項を編集' : '決定事項を追加' }}</h2>

      <form @submit.prevent="submit">
        <div class="field">
          <label for="decision-type">決定タイプ <span class="required">*</span></label>
          <select
            id="decision-type"
            v-model="form.decisionType"
            data-testid="mdc-decision-type-select"
            required
          >
            <option v-for="(label, key) in DECISION_TYPE_LABELS" :key="key" :value="key">
              {{ label }}
            </option>
          </select>
        </div>

        <div class="field">
          <label for="decided-at">決定日 <span class="required">*</span></label>
          <input
            id="decided-at"
            v-model="form.decidedAt"
            type="date"
            required
          />
        </div>

        <div class="field">
          <label for="content">決定内容 <span class="required">*</span></label>
          <textarea
            id="content"
            v-model="form.content"
            data-testid="mdc-decision-content-input"
            rows="4"
            maxlength="2000"
            placeholder="決定内容を記入してください"
            required
          />
        </div>

        <div class="field">
          <label>対象タレント</label>
          <div class="talent-select-wrap" data-testid="mdc-decision-talent-select">
            <label
              v-for="user in talents"
              :key="user.id"
              class="talent-checkbox"
            >
              <input
                v-model="form.talentIds"
                type="checkbox"
                :value="user.id"
              />
              {{ user.displayName }}
            </label>
            <p v-if="talents.length === 0" class="empty-note">タレントが見つかりません</p>
          </div>
        </div>

        <p v-if="errorMsg" class="error-msg" role="alert">{{ errorMsg }}</p>

        <div class="actions">
          <button type="button" class="btn-secondary" @click="$emit('cancel')">キャンセル</button>
          <button
            type="submit"
            class="btn-primary"
            data-testid="mdc-decision-save-button"
            :disabled="submitting"
          >
            {{ submitting ? '保存中…' : '保存' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { AppUser } from '~/types/user'
import type { MdcDecisionDto, DecisionType, CreateDecisionRequest } from '~/types/mdcDecision'
import { DECISION_TYPE_LABELS } from '~/types/mdcDecision'

const props = defineProps<{
  initial?: MdcDecisionDto
  talents: AppUser[]
}>()

const emit = defineEmits<{
  saved: [data: CreateDecisionRequest]
  cancel: []
}>()

const isEdit = computed(() => !!props.initial)

const today = new Date().toISOString().slice(0, 10)

const form = reactive({
  decisionType: (props.initial?.decisionType ?? 'Other') as DecisionType,
  content: props.initial?.content ?? '',
  decidedAt: props.initial?.decidedAt ?? today,
  talentIds: props.initial?.talents.map(t => t.talentId) ?? [] as string[],
})

const submitting = ref(false)
const errorMsg = ref('')

async function submit() {
  if (!form.content.trim()) {
    errorMsg.value = '決定内容を入力してください。'
    return
  }
  submitting.value = true
  errorMsg.value = ''
  try {
    emit('saved', {
      decisionType: form.decisionType,
      content: form.content.trim(),
      decidedAt: form.decidedAt,
      talentIds: form.talentIds,
    })
  }
  finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 50;
}

.modal {
  background: var(--surface);
  border-radius: var(--radius-lg);
  padding: 2rem;
  width: min(520px, 90vw);
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: var(--shadow-lg);
}

.modal-title {
  margin: 0 0 1.5rem;
  font-size: 1.2rem;
  font-weight: 700;
  font-family: var(--font-heading);
  color: var(--text);
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  margin-bottom: 1rem;
}

label {
  font-size: 0.82rem;
  font-weight: 600;
  color: var(--text-secondary);
}

.required { color: var(--danger); }

input[type='date'],
select,
textarea {
  border: 1px solid var(--border-strong);
  border-radius: var(--radius-sm);
  padding: 0.5rem 0.75rem;
  font-size: 0.9rem;
  font-family: var(--font-body);
  color: var(--text);
  background: var(--surface);
  outline: none;
}

input:focus,
select:focus,
textarea:focus {
  border-color: var(--accent);
  box-shadow: 0 0 0 3px rgba(192, 86, 33, 0.12);
}

.talent-select-wrap {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
  max-height: 160px;
  overflow-y: auto;
  border: 1px solid var(--border-strong);
  border-radius: var(--radius-sm);
  padding: 0.5rem 0.75rem;
}

.talent-checkbox {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  font-weight: 400;
  color: var(--text);
  cursor: pointer;
}

.talent-checkbox input { cursor: pointer; }

.empty-note { font-size: 0.875rem; color: var(--text-muted); }

.error-msg {
  color: var(--danger);
  font-size: 0.875rem;
  margin: 0 0 0.75rem;
}

.actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  margin-top: 1.5rem;
}
</style>
