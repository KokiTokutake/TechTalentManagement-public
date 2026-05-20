<template>
  <div class="modal-backdrop" @click.self="$emit('cancel')">
    <div class="modal" role="dialog" :aria-label="isEdit ? '目標を編集' : '目標を追加'">
      <h2 class="modal-title">{{ isEdit ? '目標を編集' : '目標を追加' }}</h2>

      <form @submit.prevent="submit">
        <div class="field">
          <label for="category">カテゴリ</label>
          <select id="category" v-model="form.category" :disabled="isEdit" required>
            <option value="Will">Will（やりたいこと）</option>
            <option value="Can">Can（できること）</option>
            <option value="Must">Must（やるべきこと）</option>
          </select>
        </div>

        <div class="field">
          <label for="title">目標タイトル <span class="required">*</span></label>
          <input
            id="title"
            v-model="form.title"
            type="text"
            maxlength="200"
            placeholder="例: チームリードとしてスプリント運営を担当する"
            required
          />
        </div>

        <div class="field">
          <label for="description">詳細・背景</label>
          <textarea
            id="description"
            v-model="form.description"
            rows="4"
            maxlength="2000"
            placeholder="目標の背景や具体的な達成指標を記載（任意）"
          />
        </div>

        <div class="field">
          <label for="displayOrder">表示順</label>
          <input
            id="displayOrder"
            v-model.number="form.displayOrder"
            type="number"
            min="1"
            max="99"
          />
        </div>

        <p v-if="errorMsg" class="error-msg" role="alert">{{ errorMsg }}</p>

        <div class="actions">
          <button type="button" class="btn-secondary" @click="$emit('cancel')">キャンセル</button>
          <button type="submit" class="btn-primary" :disabled="submitting">
            {{ submitting ? '保存中…' : '保存' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Goal, WillCanMust } from '~/types/goal'

const props = defineProps<{
  initial?: Goal
  defaultCategory?: WillCanMust
}>()

const emit = defineEmits<{
  saved: [{ category: WillCanMust; title: string; description: string | null; displayOrder: number }]
  cancel: []
}>()

const isEdit = computed(() => !!props.initial)

const form = reactive({
  category: (props.initial?.category ?? props.defaultCategory ?? 'Will') as WillCanMust,
  title: props.initial?.title ?? '',
  description: props.initial?.description ?? '',
  displayOrder: props.initial?.displayOrder ?? 1,
})

const submitting = ref(false)
const errorMsg = ref('')

async function submit() {
  if (!form.title.trim()) {
    errorMsg.value = '目標タイトルを入力してください。'
    return
  }
  submitting.value = true
  errorMsg.value = ''
  try {
    emit('saved', {
      category: form.category,
      title: form.title.trim(),
      description: form.description?.trim() || null,
      displayOrder: form.displayOrder,
    })
  } finally {
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

input[type='text'],
input[type='number'],
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
