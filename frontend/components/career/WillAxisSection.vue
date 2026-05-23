<template>
  <section class="wcm-subsection">
    <div v-if="!longTermFilled" class="guide-message" data-testid="will-guide-message">
      💡 まず長期のWillから考えてみましょう。3〜5年後のありたい姿を思い描くことで、短期の行動が明確になります。
    </div>

    <!-- 長期 Will -->
    <div class="axis-block">
      <div class="axis-header">
        <span class="axis-badge long">長期（3〜5年）</span>
        <p class="axis-hint">キャリアの到達イメージ・ありたい姿</p>
      </div>
      <div class="entry-list">
        <div
          v-for="(entry, i) in localWill.longTerm"
          :key="i"
          class="entry-row"
        >
          <textarea
            v-model="entry.content"
            class="entry-textarea"
            rows="2"
            maxlength="2000"
            :placeholder="`長期Will ${i + 1}`"
            :data-testid="`will-long-term-${i}`"
            :disabled="readonly"
          />
          <button v-if="!readonly" type="button" class="entry-remove" @click="removeEntry(localWill.longTerm, i)" aria-label="削除">✕</button>
        </div>
        <button v-if="!readonly" type="button" class="entry-add" @click="addEntry(localWill.longTerm)">＋ 追加</button>
      </div>
    </div>

    <!-- 中期 Will -->
    <div class="axis-block">
      <div class="axis-header">
        <span class="axis-badge mid">中期（1〜3年）</span>
        <p class="axis-hint">担いたい役割・ポジション・プロジェクト</p>
      </div>
      <div class="entry-list">
        <div
          v-for="(entry, i) in localWill.midTerm"
          :key="i"
          class="entry-row"
        >
          <textarea
            v-model="entry.content"
            class="entry-textarea"
            rows="2"
            maxlength="2000"
            :placeholder="`中期Will ${i + 1}`"
            :data-testid="`will-mid-term-${i}`"
            :disabled="readonly"
          />
          <button v-if="!readonly" type="button" class="entry-remove" @click="removeEntry(localWill.midTerm, i)" aria-label="削除">✕</button>
        </div>
        <button v-if="!readonly" type="button" class="entry-add" @click="addEntry(localWill.midTerm)">＋ 追加</button>
      </div>
    </div>

    <!-- 短期 Will -->
    <div class="axis-block">
      <div class="axis-header">
        <span class="axis-badge short">短期（6ヶ月）</span>
        <p class="axis-hint">直近の考課期間でやりたいこと・挑戦したいこと</p>
      </div>
      <div class="entry-list">
        <div
          v-for="(entry, i) in localWill.shortTerm"
          :key="i"
          class="entry-row"
        >
          <textarea
            v-model="entry.content"
            class="entry-textarea"
            rows="2"
            maxlength="2000"
            :placeholder="`短期Will ${i + 1}`"
            :data-testid="`will-short-term-${i}`"
            :disabled="readonly"
          />
          <button v-if="!readonly" type="button" class="entry-remove" @click="removeEntry(localWill.shortTerm, i)" aria-label="削除">✕</button>
        </div>
        <button v-if="!readonly" type="button" class="entry-add" @click="addEntry(localWill.shortTerm)">＋ 追加</button>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import type { WcmWillDto, SaveWcmEntryRequest } from '~/types/wcm'

const props = defineProps<{
  modelValue: { shortTerm: SaveWcmEntryRequest[], midTerm: SaveWcmEntryRequest[], longTerm: SaveWcmEntryRequest[] }
  initial?: WcmWillDto | null
  readonly?: boolean
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', v: typeof props.modelValue): void
}>()

const localWill = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

const longTermFilled = computed(() =>
  localWill.value.longTerm.some(e => e.content.trim().length > 0),
)

function addEntry(list: SaveWcmEntryRequest[]) {
  list.push({ id: null, content: '' })
}

function removeEntry(list: SaveWcmEntryRequest[], idx: number) {
  list.splice(idx, 1)
}

// 初期データが空の場合、入力行を1行ずつ用意する
onMounted(() => {
  if (localWill.value.longTerm.length === 0) addEntry(localWill.value.longTerm)
  if (localWill.value.midTerm.length === 0) addEntry(localWill.value.midTerm)
  if (localWill.value.shortTerm.length === 0) addEntry(localWill.value.shortTerm)
})
</script>

<style scoped>
.wcm-subsection { display: flex; flex-direction: column; gap: 1.5rem; }
.guide-message {
  padding: 0.75rem 1rem;
  background: #eff6ff;
  border: 1px solid #bfdbfe;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  color: #1d4ed8;
  line-height: 1.5;
}
.axis-block { display: flex; flex-direction: column; gap: 0.5rem; }
.axis-header { display: flex; align-items: baseline; gap: 0.75rem; flex-wrap: wrap; }
.axis-badge {
  font-size: 0.7rem;
  font-weight: 700;
  padding: 0.2rem 0.6rem;
  border-radius: 9999px;
  white-space: nowrap;
}
.axis-badge.long { background: #ede9fe; color: #6d28d9; }
.axis-badge.mid  { background: #dbeafe; color: #1d4ed8; }
.axis-badge.short { background: #dcfce7; color: #15803d; }
.axis-hint { font-size: 0.8rem; color: var(--text-secondary); margin: 0; }
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
  line-height: 1.5;
}
.entry-textarea:disabled { background: var(--surface-muted); color: var(--text-secondary); }
.entry-remove {
  margin-top: 0.35rem;
  background: none;
  border: none;
  color: var(--text-secondary);
  cursor: pointer;
  font-size: 0.9rem;
  padding: 0.25rem;
  line-height: 1;
}
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
