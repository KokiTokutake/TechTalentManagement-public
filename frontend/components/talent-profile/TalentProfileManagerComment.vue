<template>
  <section class="card" data-testid="talent-profile-manager-comment-section">
    <h2 class="section-title">育成コメント（上長記入）</h2>
    <div class="comment-form">
      <div class="field">
        <label class="field-label">現状の課題</label>
        <textarea
          v-model="form.currentChallenges"
          class="field-textarea"
          rows="3"
          placeholder="現状の課題を記入してください"
          data-testid="talent-profile-challenges-input"
        />
      </div>
      <div class="field">
        <label class="field-label">育成方針</label>
        <textarea
          v-model="form.developmentPolicy"
          class="field-textarea"
          rows="3"
          placeholder="育成方針を記入してください"
          data-testid="talent-profile-policy-input"
        />
      </div>
      <div class="field">
        <label class="field-label">期待する役割</label>
        <textarea
          v-model="form.expectedRole"
          class="field-textarea"
          rows="2"
          placeholder="期待する役割を記入してください"
          data-testid="talent-profile-expected-role-input"
        />
      </div>
      <div class="comment-actions">
        <button
          v-if="canCopyPrev"
          class="btn-ghost"
          data-testid="talent-profile-copy-prev-button"
          :disabled="saving"
          @click="$emit('copy-prev')"
        >
          前期からコピー
        </button>
        <button
          class="btn-primary"
          data-testid="talent-profile-comment-save-button"
          :disabled="saving"
          @click="handleSave"
        >
          {{ saving ? '保存中...' : '保存' }}
        </button>
      </div>
      <p v-if="savedAt" class="saved-note">
        最終保存: {{ formatDate(savedAt) }}
      </p>
    </div>
  </section>
</template>

<script setup lang="ts">
import { reactive, watch } from 'vue'
import type { ManagerComment, UpsertManagerCommentRequest } from '~/types/talentProfile'

const props = defineProps<{
  managerComment: ManagerComment | null
  saving: boolean
  canCopyPrev: boolean
}>()

const emit = defineEmits<{
  (e: 'save', data: UpsertManagerCommentRequest): void
  (e: 'copy-prev'): void
}>()

const form = reactive<UpsertManagerCommentRequest>({
  currentChallenges: props.managerComment?.currentChallenges ?? null,
  developmentPolicy: props.managerComment?.developmentPolicy ?? null,
  expectedRole: props.managerComment?.expectedRole ?? null,
})

const savedAt = computed(() => props.managerComment?.updatedAt ?? null)

watch(() => props.managerComment, (comment) => {
  form.currentChallenges = comment?.currentChallenges ?? null
  form.developmentPolicy = comment?.developmentPolicy ?? null
  form.expectedRole = comment?.expectedRole ?? null
})

function handleSave() {
  emit('save', { ...form })
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleString('ja-JP')
}
</script>

<style scoped>
.comment-form { display: flex; flex-direction: column; gap: 1rem; }
.field { display: flex; flex-direction: column; gap: 0.375rem; }
.field-label { font-size: 0.875rem; color: #374151; font-weight: 500; }
.field-textarea { border: 1px solid #d1d5db; border-radius: 0.375rem; padding: 0.5rem 0.75rem; font-size: 0.875rem; resize: vertical; font-family: inherit; }
.field-textarea:focus { outline: none; border-color: #3b82f6; box-shadow: 0 0 0 3px rgba(59,130,246,0.1); }
.comment-actions { display: flex; justify-content: flex-end; gap: 0.75rem; }
.saved-note { font-size: 0.75rem; color: #9ca3af; text-align: right; margin: 0; }
@media print { .comment-form { display: none; } }
</style>
