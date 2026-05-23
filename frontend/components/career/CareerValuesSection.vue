<template>
  <section class="values-section" data-testid="career-values-section">
    <div class="field-group">
      <label class="field-label">仕事で大切にしていること</label>
      <textarea
        v-model="localValues.importantThings"
        class="field-textarea"
        rows="3"
        maxlength="1000"
        placeholder="例: チームの信頼関係、高品質なコード、ユーザー価値..."
        data-testid="values-important-things"
        :disabled="readonly"
      />
    </div>
    <div class="field-group">
      <label class="field-label">やる気が上がる状況</label>
      <textarea
        v-model="localValues.motivationSituations"
        class="field-textarea"
        rows="3"
        maxlength="1000"
        placeholder="例: 難しい課題をチームで解決したとき、自分の設計が本番で動いたとき..."
        data-testid="values-motivation-situations"
        :disabled="readonly"
      />
    </div>
    <div class="field-group">
      <label class="field-label">避けたい状況</label>
      <textarea
        v-model="localValues.avoidSituations"
        class="field-textarea"
        rows="3"
        maxlength="1000"
        placeholder="例: 技術的負債を放置せざるを得ない状況、成長機会のない単純作業..."
        data-testid="values-avoid-situations"
        :disabled="readonly"
      />
    </div>
    <div v-if="!readonly" class="save-row">
      <p v-if="saveError" class="save-error">{{ saveError }}</p>
      <p v-if="saveSuccess" class="save-success" data-testid="wcm-save-success">価値観を保存しました</p>
      <button
        type="button"
        class="btn-primary"
        :disabled="saving"
        data-testid="values-save-button"
        @click="$emit('save')"
      >
        {{ saving ? '保存中…' : '価値観を保存' }}
      </button>
    </div>
  </section>
</template>

<script setup lang="ts">
const props = defineProps<{
  modelValue: { importantThings: string | null; motivationSituations: string | null; avoidSituations: string | null }
  saving?: boolean
  saveError?: string | null
  saveSuccess?: boolean
  readonly?: boolean
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', v: typeof props.modelValue): void
  (e: 'save'): void
}>()

const localValues = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})
</script>

<style scoped>
.values-section { display: flex; flex-direction: column; gap: 1rem; }
.field-group { display: flex; flex-direction: column; gap: 0.35rem; }
.field-label { font-size: 0.8rem; font-weight: 600; color: var(--text-secondary); }
.field-textarea {
  width: 100%;
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--border-strong);
  border-radius: 0.375rem;
  font-size: 0.875rem;
  resize: vertical;
  font-family: inherit;
  line-height: 1.5;
}
.field-textarea:disabled { background: var(--surface-muted); color: var(--text-secondary); }
.save-row { display: flex; align-items: center; justify-content: flex-end; gap: 1rem; flex-wrap: wrap; }
.save-error { font-size: 0.8rem; color: var(--danger); margin: 0; }
.save-success { font-size: 0.8rem; color: #15803d; margin: 0; }
</style>
