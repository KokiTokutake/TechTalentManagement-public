<template>
  <div class="modal-backdrop" @click.self="$emit('cancel')">
    <div class="modal" role="dialog" aria-label="アクションアイテムを追加">
      <h2 class="modal-title">アクションアイテムを追加</h2>

      <form @submit.prevent="submit">
        <div class="field">
          <label for="ai-description">内容 <span class="required">*</span></label>
          <textarea
            id="ai-description"
            v-model="form.description"
            data-testid="mdc-action-item-description-input"
            rows="3"
            maxlength="500"
            placeholder="実施すべき内容を記入してください"
            required
          />
        </div>

        <div class="field">
          <label for="ai-assignee">担当者 <span class="required">*</span></label>
          <select
            id="ai-assignee"
            v-model="form.assigneeId"
            data-testid="mdc-action-item-assignee-select"
            required
          >
            <option value="" disabled>担当者を選択してください</option>
            <option v-for="user in users" :key="user.id" :value="user.id">
              {{ user.displayName }}
            </option>
          </select>
        </div>

        <div class="field">
          <label for="ai-due-date">期限</label>
          <input
            id="ai-due-date"
            v-model="form.dueDate"
            type="date"
            data-testid="mdc-action-item-due-date-input"
          />
        </div>

        <p v-if="errorMsg" class="error-msg" role="alert">{{ errorMsg }}</p>

        <div class="actions">
          <button type="button" class="btn-secondary" @click="$emit('cancel')">キャンセル</button>
          <button
            type="submit"
            class="btn-primary"
            data-testid="mdc-action-item-save-button"
            :disabled="submitting"
          >
            {{ submitting ? '保存中…' : '追加' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { AppUser } from '~/types/user'
import type { CreateActionItemRequest } from '~/types/mdcDecision'

defineProps<{ users: AppUser[] }>()

const emit = defineEmits<{
  saved: [data: CreateActionItemRequest]
  cancel: []
}>()

const form = reactive({
  description: '',
  assigneeId: '',
  dueDate: '',
})

const submitting = ref(false)
const errorMsg = ref('')

async function submit() {
  if (!form.description.trim()) {
    errorMsg.value = '内容を入力してください。'
    return
  }
  if (!form.assigneeId) {
    errorMsg.value = '担当者を選択してください。'
    return
  }
  submitting.value = true
  errorMsg.value = ''
  try {
    emit('saved', {
      description: form.description.trim(),
      assigneeId: form.assigneeId,
      dueDate: form.dueDate || null,
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
  width: min(480px, 90vw);
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
