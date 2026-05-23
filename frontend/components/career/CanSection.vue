<template>
  <section class="can-section">
    <!-- タブ切替 -->
    <div class="tab-bar" role="tablist">
      <button
        role="tab"
        :aria-selected="activeTab === 'strength'"
        class="tab-btn"
        :class="{ active: activeTab === 'strength' }"
        @click="activeTab = 'strength'"
      >
        強み（活かしたいCan）
        <span class="tab-count">{{ localStrengths.length }}/5</span>
      </button>
      <button
        role="tab"
        :aria-selected="activeTab === 'challenge'"
        class="tab-btn"
        :class="{ active: activeTab === 'challenge' }"
        @click="activeTab = 'challenge'"
      >
        課題（伸ばしたいCan）
        <span class="tab-count">{{ localChallenges.length }}/5</span>
      </button>
    </div>

    <!-- 強みタブ -->
    <div v-show="activeTab === 'strength'" class="tab-panel">
      <div class="entry-list">
        <div v-for="(entry, i) in localStrengths" :key="i" class="entry-row">
          <div class="entry-main">
            <textarea
              v-model="entry.content"
              class="entry-textarea"
              rows="2"
              maxlength="2000"
              :placeholder="`強み ${i + 1}`"
              :data-testid="`can-strength-${i}`"
              :disabled="readonly"
            />
            <span v-if="entry.linkedSkillName" class="skill-badge">🔗 {{ entry.linkedSkillName }}</span>
          </div>
          <button v-if="!readonly" type="button" class="entry-remove" @click="removeStrength(i)" aria-label="削除">✕</button>
        </div>
      </div>
      <p v-if="strengthLimitReached" class="limit-message" data-testid="can-limit-message">
        強みは最大5件まで入力できます
      </p>
      <div v-if="!readonly" class="entry-actions">
        <button
          type="button"
          class="entry-add"
          :disabled="strengthLimitReached"
          data-testid="can-strength-add-button"
          @click="addStrength"
        >
          ＋ 追加
        </button>
        <button
          type="button"
          class="skill-select-btn"
          :disabled="strengthLimitReached"
          data-testid="can-skill-select-button"
          @click="openSkillPicker('strength')"
        >
          🔗 スキルから選択
        </button>
      </div>
    </div>

    <!-- 課題タブ -->
    <div v-show="activeTab === 'challenge'" class="tab-panel">
      <div class="entry-list">
        <div v-for="(entry, i) in localChallenges" :key="i" class="entry-row">
          <div class="entry-main">
            <textarea
              v-model="entry.content"
              class="entry-textarea"
              rows="2"
              maxlength="2000"
              :placeholder="`課題 ${i + 1}`"
              :data-testid="`can-challenge-${i}`"
              :disabled="readonly"
            />
            <span v-if="entry.linkedSkillName" class="skill-badge">🔗 {{ entry.linkedSkillName }}</span>
          </div>
          <button v-if="!readonly" type="button" class="entry-remove" @click="removeChallenge(i)" aria-label="削除">✕</button>
        </div>
      </div>
      <p v-if="challengeLimitReached" class="limit-message" data-testid="can-limit-message">
        課題は最大5件まで入力できます
      </p>
      <div v-if="!readonly" class="entry-actions">
        <button
          type="button"
          class="entry-add"
          :disabled="challengeLimitReached"
          data-testid="can-challenge-add-button"
          @click="addChallenge"
        >
          ＋ 追加
        </button>
        <button
          type="button"
          class="skill-select-btn"
          :disabled="challengeLimitReached"
          data-testid="can-skill-select-button"
          @click="openSkillPicker('challenge')"
        >
          🔗 スキルから選択
        </button>
      </div>
    </div>

    <!-- スキル選択モーダル -->
    <div v-if="pickerOpen" class="modal-overlay" @click.self="pickerOpen = false">
      <div class="modal">
        <div class="modal-header">
          <h3>スキルから選択</h3>
          <button type="button" class="modal-close" @click="pickerOpen = false">✕</button>
        </div>
        <div v-if="skillsLoading" class="modal-body">読み込み中...</div>
        <div v-else class="modal-body">
          <div v-for="cat in skillCategories" :key="cat.id" class="skill-cat">
            <p class="skill-cat-name">{{ cat.name }}</p>
            <div class="skill-items">
              <button
                v-for="item in cat.items"
                :key="item.id"
                type="button"
                class="skill-chip"
                @click="selectSkill(item)"
              >
                {{ item.name }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import type { SaveWcmEntryRequest } from '~/types/wcm'

interface SkillItem { id: string; name: string }
interface SkillCategory { id: string; name: string; items: SkillItem[] }

const props = defineProps<{
  strengths: SaveWcmEntryRequest[]
  challenges: SaveWcmEntryRequest[]
  readonly?: boolean
}>()

const emit = defineEmits<{
  (e: 'update:strengths' | 'update:challenges', v: SaveWcmEntryRequest[]): void
}>()

const config = useRuntimeConfig()
const base = config.public.apiBaseUrl

const activeTab = ref<'strength' | 'challenge'>('strength')
const pickerOpen = ref(false)
const pickerTarget = ref<'strength' | 'challenge'>('strength')
const skillCategories = ref<SkillCategory[]>([])
const skillsLoading = ref(false)

const localStrengths = computed({
  get: () => props.strengths,
  set: (v) => emit('update:strengths', v),
})

const localChallenges = computed({
  get: () => props.challenges,
  set: (v) => emit('update:challenges', v),
})

const strengthLimitReached = computed(() => localStrengths.value.length >= 5)
const challengeLimitReached = computed(() => localChallenges.value.length >= 5)

function addStrength() {
  if (strengthLimitReached.value) return
  emit('update:strengths', [...localStrengths.value, { id: null, content: '' }])
}

function removeStrength(idx: number) {
  const next = [...localStrengths.value]
  next.splice(idx, 1)
  emit('update:strengths', next)
}

function addChallenge() {
  if (challengeLimitReached.value) return
  emit('update:challenges', [...localChallenges.value, { id: null, content: '' }])
}

function removeChallenge(idx: number) {
  const next = [...localChallenges.value]
  next.splice(idx, 1)
  emit('update:challenges', next)
}

async function openSkillPicker(target: 'strength' | 'challenge') {
  pickerTarget.value = target
  pickerOpen.value = true
  if (skillCategories.value.length > 0) return
  skillsLoading.value = true
  try {
    const cats = await $fetch<Array<{ id: string; name: string }>>(`${base}/api/skill-categories`)
    const withItems = await Promise.all(
      cats.map(async (c) => {
        const items = await $fetch<SkillItem[]>(`${base}/api/skill-categories/${c.id}/items`)
        return { ...c, items }
      }),
    )
    skillCategories.value = withItems
  }
  finally {
    skillsLoading.value = false
  }
}

function selectSkill(item: SkillItem) {
  const entry: SaveWcmEntryRequest = { id: null, content: item.name, linkedSkillItemId: item.id }
  if (pickerTarget.value === 'strength') {
    if (!strengthLimitReached.value)
      emit('update:strengths', [...localStrengths.value, entry])
  }
  else {
    if (!challengeLimitReached.value)
      emit('update:challenges', [...localChallenges.value, entry])
  }
  pickerOpen.value = false
}
</script>

<style scoped>
.can-section { display: flex; flex-direction: column; gap: 0.75rem; }
.tab-bar { display: flex; gap: 0.5rem; border-bottom: 2px solid var(--border); padding-bottom: 0; }
.tab-btn {
  padding: 0.5rem 1rem;
  font-size: 0.875rem;
  font-weight: 500;
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  margin-bottom: -2px;
  cursor: pointer;
  color: var(--text-secondary);
  display: flex;
  align-items: center;
  gap: 0.4rem;
}
.tab-btn.active { color: var(--accent); border-bottom-color: var(--accent); }
.tab-count { font-size: 0.7rem; background: var(--surface-muted); padding: 0.1rem 0.4rem; border-radius: 9999px; }
.tab-panel { display: flex; flex-direction: column; gap: 0.5rem; padding-top: 0.5rem; }
.entry-list { display: flex; flex-direction: column; gap: 0.5rem; }
.entry-row { display: flex; gap: 0.5rem; align-items: flex-start; }
.entry-main { flex: 1; display: flex; flex-direction: column; gap: 0.25rem; }
.entry-textarea {
  width: 100%;
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--border-strong);
  border-radius: 0.375rem;
  font-size: 0.875rem;
  resize: vertical;
  font-family: inherit;
}
.entry-textarea:disabled { background: var(--surface-muted); color: var(--text-secondary); }
.skill-badge { font-size: 0.75rem; color: var(--accent); }
.entry-remove { background: none; border: none; color: var(--text-secondary); cursor: pointer; font-size: 0.9rem; padding: 0.25rem; margin-top: 0.35rem; }
.entry-remove:hover { color: var(--danger); }
.limit-message { font-size: 0.8rem; color: var(--danger); margin: 0; }
.entry-actions { display: flex; gap: 0.5rem; flex-wrap: wrap; }
.entry-add {
  background: none;
  border: 1px dashed var(--border-strong);
  border-radius: 0.375rem;
  padding: 0.35rem 0.75rem;
  font-size: 0.8rem;
  color: var(--text-secondary);
  cursor: pointer;
}
.entry-add:hover:not(:disabled) { border-color: var(--accent); color: var(--accent); }
.entry-add:disabled { opacity: 0.4; cursor: not-allowed; }
.skill-select-btn {
  background: none;
  border: 1px solid var(--border-strong);
  border-radius: 0.375rem;
  padding: 0.35rem 0.75rem;
  font-size: 0.8rem;
  color: var(--text-secondary);
  cursor: pointer;
}
.skill-select-btn:hover:not(:disabled) { border-color: var(--accent); color: var(--accent); }
.skill-select-btn:disabled { opacity: 0.4; cursor: not-allowed; }
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.4); z-index: 50; display: flex; align-items: center; justify-content: center; padding: 1rem; }
.modal { background: var(--surface); border-radius: 0.5rem; width: 100%; max-width: 480px; max-height: 70vh; display: flex; flex-direction: column; }
.modal-header { display: flex; align-items: center; justify-content: space-between; padding: 1rem 1.25rem; border-bottom: 1px solid var(--border); }
.modal-header h3 { margin: 0; font-size: 1rem; }
.modal-close { background: none; border: none; font-size: 1rem; cursor: pointer; color: var(--text-secondary); }
.modal-body { padding: 1rem 1.25rem; overflow-y: auto; display: flex; flex-direction: column; gap: 1rem; }
.skill-cat-name { font-size: 0.75rem; font-weight: 600; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.05em; margin: 0 0 0.4rem; }
.skill-items { display: flex; flex-wrap: wrap; gap: 0.4rem; }
.skill-chip {
  padding: 0.3rem 0.75rem;
  border: 1px solid var(--border-strong);
  border-radius: 9999px;
  font-size: 0.8rem;
  background: var(--surface-muted);
  cursor: pointer;
}
.skill-chip:hover { border-color: var(--accent); color: var(--accent); background: #eff6ff; }
</style>
