<template>
  <div class="level-def-form">
    <div v-for="lv in 7" :key="lv" class="level-row">
      <div class="level-badge" :class="`lv-${lv}`">Lv.{{ lv }}</div>
      <div class="level-inputs">
        <input
          v-model="form[lv].description"
          class="input"
          placeholder="レベルの説明（必須）"
          :disabled="saving"
          required
        />
        <textarea
          v-model="form[lv].examples"
          class="textarea"
          placeholder="行動例（任意）"
          rows="2"
          :disabled="saving"
        ></textarea>
      </div>
    </div>

    <div class="form-actions">
      <button
        class="btn-primary"
        type="button"
        :disabled="saving || !isValid"
        data-testid="skill-level-def-save-button"
        @click="handleSave"
      >
        {{ saving ? '保存中…' : 'レベル定義を保存' }}
      </button>
      <span v-if="saveStatus === 'saved'" class="save-ok">✓ 保存しました</span>
      <span v-else-if="saveStatus === 'error'" class="save-err">保存に失敗しました</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { SkillLevelDefinitions } from '~/types/skill'

const props = defineProps<{
  definitions: SkillLevelDefinitions | null
  saving: boolean
  saveStatus: 'idle' | 'saved' | 'error'
}>()

const emit = defineEmits<{
  (e: 'save', payload: Array<{ level: number; description: string; examples: string | null }>): void
}>()

interface LevelFormRow {
  description: string
  examples: string
}

const form = ref<Record<number, LevelFormRow>>({
  1: { description: '', examples: '' },
  2: { description: '', examples: '' },
  3: { description: '', examples: '' },
  4: { description: '', examples: '' },
  5: { description: '', examples: '' },
  6: { description: '', examples: '' },
  7: { description: '', examples: '' },
})

watch(() => props.definitions, (defs) => {
  if (!defs) return
  for (const d of defs.levelDefinitions) {
    if (d.level >= 1 && d.level <= 7) {
      form.value[d.level] = {
        description: d.description,
        examples: d.examples ?? '',
      }
    }
  }
}, { immediate: true })

const isValid = computed(() =>
  Object.values(form.value).every(r => r.description.trim().length > 0),
)

function handleSave() {
  const payload = Array.from({ length: 7 }, (_, i) => ({
    level: i + 1,
    description: form.value[i + 1].description.trim(),
    examples: form.value[i + 1].examples.trim() || null,
  }))
  emit('save', payload)
}
</script>

<style scoped>
.level-def-form { display: flex; flex-direction: column; gap: 0.75rem; }

.level-row {
  display: grid;
  grid-template-columns: 56px 1fr;
  gap: 0.75rem;
  align-items: start;
}

.level-badge {
  font-size: 0.75rem;
  font-weight: 800;
  padding: 0.3rem 0;
  border-radius: var(--radius-sm);
  text-align: center;
  line-height: 1;
}

.lv-1 { color: #166534; }
.lv-2 { color: #065f46; }
.lv-3 { color: #1d4ed8; }
.lv-4 { color: #3730a3; }
.lv-5 { color: #92400e; }
.lv-6 { color: #b45309; }
.lv-7 { color: #991b1b; }

.level-inputs { display: flex; flex-direction: column; gap: 0.35rem; }

.textarea {
  width: 100%;
  padding: 0.45rem 0.6rem;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  font-size: 0.85rem;
  font-family: inherit;
  resize: vertical;
  background: var(--surface);
  color: var(--text);
  transition: border-color 0.15s;
  box-sizing: border-box;
}

.textarea:focus { outline: none; border-color: var(--accent); }

.form-actions {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding-top: 0.5rem;
  border-top: 1px solid var(--border);
}

.save-ok { font-size: 0.85rem; color: var(--success-text, #166534); font-weight: 600; }
.save-err { font-size: 0.85rem; color: var(--danger-text); font-weight: 600; }
</style>
