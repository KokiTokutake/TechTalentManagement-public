<template>
  <section class="must-section">
    <!-- 上長の期待ロール（読み取り専用） -->
    <div v-if="managerExpectedRole" class="manager-context" data-testid="must-manager-context">
      <p class="manager-context-label">上長からの期待（読み取り専用）</p>
      <p class="manager-context-text">{{ managerExpectedRole }}</p>
    </div>

    <!-- Must 入力リスト -->
    <div class="entry-list">
      <div v-for="(entry, i) in localMust" :key="i" class="entry-row">
        <textarea
          v-model="entry.content"
          class="entry-textarea"
          rows="2"
          maxlength="2000"
          :placeholder="`Must ${i + 1}`"
          :data-testid="`must-${i}`"
          :disabled="readonly"
        />
        <button v-if="!readonly" type="button" class="entry-remove" @click="removeEntry(i)" aria-label="削除">✕</button>
      </div>
    </div>
    <button v-if="!readonly" type="button" class="entry-add" @click="addEntry">＋ 追加</button>
  </section>
</template>

<script setup lang="ts">
import type { SaveWcmEntryRequest } from '~/types/wcm'

const props = defineProps<{
  modelValue: SaveWcmEntryRequest[]
  managerExpectedRole?: string | null
  readonly?: boolean
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', v: SaveWcmEntryRequest[]): void
}>()

const localMust = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

function addEntry() {
  localMust.value = [...localMust.value, { id: null, content: '' }]
}

function removeEntry(idx: number) {
  const next = [...localMust.value]
  next.splice(idx, 1)
  localMust.value = next
}

onMounted(() => {
  if (localMust.value.length === 0) addEntry()
})
</script>

<style scoped>
.must-section { display: flex; flex-direction: column; gap: 0.75rem; }
.manager-context {
  padding: 0.75rem 1rem;
  background: #fefce8;
  border: 1px solid #fde047;
  border-radius: 0.5rem;
}
.manager-context-label { font-size: 0.7rem; font-weight: 700; color: #854d0e; text-transform: uppercase; letter-spacing: 0.05em; margin: 0 0 0.35rem; }
.manager-context-text { font-size: 0.875rem; color: #713f12; margin: 0; white-space: pre-wrap; }
.entry-list { display: flex; flex-direction: column; gap: 0.5rem; }
.entry-row { display: flex; gap: 0.5rem; align-items: flex-start; }
.entry-textarea {
  flex: 1;
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--border-strong);
  border-radius: 0.375rem;
  font-size: 0.875rem;
  resize: vertical;
  font-family: inherit;
}
.entry-textarea:disabled { background: var(--surface-muted); color: var(--text-secondary); }
.entry-remove { background: none; border: none; color: var(--text-secondary); cursor: pointer; font-size: 0.9rem; padding: 0.25rem; margin-top: 0.35rem; }
.entry-remove:hover { color: var(--danger); }
.entry-add {
  align-self: flex-start;
  background: none;
  border: 1px dashed var(--border-strong);
  border-radius: 0.375rem;
  padding: 0.35rem 0.75rem;
  font-size: 0.8rem;
  color: var(--text-secondary);
  cursor: pointer;
}
.entry-add:hover { border-color: var(--accent); color: var(--accent); }
</style>
