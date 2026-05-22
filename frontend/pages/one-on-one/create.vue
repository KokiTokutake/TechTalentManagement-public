<template>
  <div class="page-wrap">
    <header class="page-header">
      <div>
        <h1>1on1 面談記録 — 新規作成</h1>
        <p v-if="talentName" class="subtitle">{{ talentName }} との面談を記録します</p>
      </div>
      <NuxtLink :to="backLink" class="btn btn--secondary btn--sm">← 戻る</NuxtLink>
    </header>

    <!-- テンプレート選択 -->
    <div v-if="store.templates.length > 0" class="template-bar">
      <label class="template-label">テンプレートを選択:</label>
      <div class="template-buttons">
        <button
          v-for="tpl in store.templates"
          :key="tpl.id"
          type="button"
          class="template-btn"
          :class="{ 'template-btn--active': selectedTemplateId === tpl.id }"
          @click="applyTemplate(tpl)"
        >
          {{ tpl.name }}
        </button>
        <button
          v-if="selectedTemplateId"
          type="button"
          class="template-btn template-btn--clear"
          @click="clearTemplate"
        >
          クリア
        </button>
      </div>
    </div>

    <!-- フォーム -->
    <form class="form-card" @submit.prevent="handleSave">
      <!-- 基本情報 -->
      <div class="form-section">
        <h2 class="form-section-title">基本情報</h2>
        <div class="form-row">
          <div class="form-group form-group--required">
            <label class="form-label" for="conducted-at">実施日 <span class="required">*</span></label>
            <input
              id="conducted-at"
              v-model="form.conductedAt"
              type="date"
              class="form-input"
              data-testid="meeting-conducted-at-input"
            />
            <span v-if="validationError" class="field-error">{{ validationError }}</span>
          </div>
          <div class="form-group">
            <label class="form-label" for="format-select">形式</label>
            <select
              id="format-select"
              v-model="form.format"
              class="form-select"
              data-testid="meeting-format-select"
            >
              <option value="InPerson">対面</option>
              <option value="Online">オンライン</option>
              <option value="Hybrid">ハイブリッド</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Will-Can-Must メモ -->
      <div class="form-section">
        <h2 class="form-section-title">面談メモ</h2>
        <div class="form-group">
          <label class="form-label form-label--will" for="will-input">Will（やりたいこと）</label>
          <textarea
            id="will-input"
            v-model="form.willTopics"
            class="form-textarea"
            rows="3"
            :placeholder="willPlaceholder"
            data-testid="meeting-will-topics-input"
          />
        </div>
        <div class="form-group">
          <label class="form-label form-label--can" for="can-input">Can（できること）</label>
          <textarea
            id="can-input"
            v-model="form.canTopics"
            class="form-textarea"
            rows="3"
            :placeholder="canPlaceholder"
            data-testid="meeting-can-topics-input"
          />
        </div>
        <div class="form-group">
          <label class="form-label form-label--must" for="must-input">Must（やるべきこと）</label>
          <textarea
            id="must-input"
            v-model="form.mustTopics"
            class="form-textarea"
            rows="3"
            :placeholder="mustPlaceholder"
            data-testid="meeting-must-topics-input"
          />
        </div>
        <div class="form-group">
          <label class="form-label" for="situation-input">近況・コンテキスト</label>
          <textarea
            id="situation-input"
            v-model="form.situationNote"
            class="form-textarea"
            rows="2"
            placeholder="タレントの近況・気になっていること"
            data-testid="meeting-situation-input"
          />
        </div>
        <div class="form-group">
          <label class="form-label" for="free-text-input">自由記述・その他</label>
          <textarea
            id="free-text-input"
            v-model="form.freeText"
            class="form-textarea"
            rows="2"
            :placeholder="freeTextPlaceholder || '自由に記述してください'"
            data-testid="meeting-free-text-input"
          />
        </div>
      </div>

      <!-- 宿題（アクションアイテム） -->
      <div class="form-section">
        <h2 class="form-section-title">宿題・アクションアイテム</h2>
        <div class="action-item-input-row">
          <input
            v-model="newActionItem"
            type="text"
            class="form-input"
            placeholder="宿題を入力してください"
            data-testid="meeting-action-item-input"
            @keydown.enter.prevent="addActionItem"
          />
          <button
            type="button"
            class="btn btn--secondary"
            :disabled="!newActionItem.trim()"
            data-testid="meeting-action-item-add-button"
            @click="addActionItem"
          >
            宿題を追加
          </button>
        </div>
        <ul v-if="actionItems.length > 0" class="action-item-list">
          <li
            v-for="(item, idx) in actionItems"
            :key="idx"
            class="action-item"
          >
            <span class="action-item-bullet">•</span>
            <span class="action-item-text">{{ item }}</span>
            <button
              type="button"
              class="action-item-remove"
              @click="actionItems.splice(idx, 1)"
            >
              ×
            </button>
          </li>
        </ul>
      </div>

      <!-- 保存アクション -->
      <div class="form-actions">
        <NuxtLink :to="backLink" class="btn btn--secondary">キャンセル</NuxtLink>
        <button
          type="submit"
          class="btn btn--primary"
          :disabled="store.loading"
          data-testid="meeting-save-button"
        >
          {{ store.loading ? '保存中…' : '保存する' }}
        </button>
      </div>

      <!-- エラー -->
      <div v-if="store.error" class="alert alert--error">{{ store.error }}</div>

      <!-- 成功 -->
      <div v-if="saved" class="alert alert--success" data-testid="meeting-saved-message">
        面談記録を保存しました。
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import type { MeetingFormat, OneOnOneTemplate } from '~/types/oneOnOne'

const route = useRoute()
const auth = useAuthStore()
const store = useOneOnOneStore()

const talentId = computed(() => route.query.talentId as string)
const talentName = computed(() => route.query.talentName as string ?? '')
const backLink = computed(() =>
  talentId.value ? `/one-on-one?talentId=${talentId.value}&talentName=${talentName.value}` : '/one-on-one',
)

const form = reactive({
  conductedAt: new Date().toISOString().slice(0, 10),
  format: 'InPerson' as MeetingFormat,
  willTopics: '',
  canTopics: '',
  mustTopics: '',
  situationNote: '',
  freeText: '',
})

const newActionItem = ref('')
const actionItems = ref<string[]>([])
const saved = ref(false)
const validationError = ref('')
const selectedTemplateId = ref<string | null>(null)
const selectedTemplate = ref<OneOnOneTemplate | null>(null)

const willPlaceholder = computed(() => selectedTemplate.value?.willPlaceholder ?? 'Will軸のトピック・やりたいことを記入')
const canPlaceholder = computed(() => selectedTemplate.value?.canPlaceholder ?? 'Can軸のトピック・できることを記入')
const mustPlaceholder = computed(() => selectedTemplate.value?.mustPlaceholder ?? 'Must軸のトピック・やるべきことを記入')
const freeTextPlaceholder = computed(() => selectedTemplate.value?.freeTextPlaceholder ?? null)

onMounted(async () => {
  if (!auth.currentUser) {
    await navigateTo('/')
    return
  }
  if (!talentId.value) {
    await navigateTo('/one-on-one')
    return
  }
  await store.loadTemplates()
})

function applyTemplate(tpl: OneOnOneTemplate) {
  selectedTemplateId.value = tpl.id
  selectedTemplate.value = tpl
}

function clearTemplate() {
  selectedTemplateId.value = null
  selectedTemplate.value = null
}

function addActionItem() {
  const text = newActionItem.value.trim()
  if (!text) return
  actionItems.value.push(text)
  newActionItem.value = ''
}

async function handleSave() {
  validationError.value = ''
  if (!form.conductedAt) {
    validationError.value = '実施日は必須です。'
    return
  }
  if (!auth.currentUser) return

  try {
    const meeting = await store.createMeeting(
      {
        talentId: talentId.value,
        conductedAt: form.conductedAt,
        format: form.format,
        willTopics: form.willTopics || null,
        canTopics: form.canTopics || null,
        mustTopics: form.mustTopics || null,
        situationNote: form.situationNote || null,
        freeText: form.freeText || null,
        shareWithMdc: false,
      },
      auth.currentUser.id,
    )

    // 宿題を逐次追加
    for (const desc of actionItems.value) {
      await store.addActionItem(meeting.id, desc, auth.currentUser.id)
    }

    saved.value = true
    actionItems.value = []
  } catch {
    // store.error に設定済み
  }
}
</script>

<style scoped>
.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.25rem;
}

h1 {
  font-size: 1.4rem;
  font-weight: 800;
  margin: 0 0 0.25rem;
  color: var(--text);
  font-family: var(--font-heading);
}

.subtitle {
  font-size: 0.875rem;
  color: var(--text-secondary);
  margin: 0;
}

.template-bar {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin-bottom: 1.25rem;
  padding: 0.75rem 1rem;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;
}

.template-label {
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--text-secondary);
  white-space: nowrap;
}

.template-buttons {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.template-btn {
  padding: 0.3rem 0.75rem;
  border-radius: 999px;
  font-size: 0.8rem;
  font-weight: 600;
  border: 1px solid var(--border);
  background: var(--surface-muted);
  color: var(--text-secondary);
  cursor: pointer;
  transition: all 0.12s;
}

.template-btn:hover {
  border-color: var(--accent);
  color: var(--accent);
}

.template-btn--active {
  background: var(--accent);
  color: #fff;
  border-color: var(--accent);
}

.template-btn--clear {
  background: transparent;
  color: var(--text-muted);
  border-style: dashed;
}

.form-card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 1.75rem;
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.form-section {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.form-section-title {
  font-size: 0.8rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--text-muted);
  padding-bottom: 0.5rem;
  border-bottom: 1px solid var(--border);
  margin: 0;
}

.form-row {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  flex: 1;
  min-width: 180px;
}

.form-label {
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--text-secondary);
}

.form-label--will { color: #7c3aed; }
.form-label--can  { color: #0891b2; }
.form-label--must { color: #059669; }

.required { color: #ef4444; margin-left: 0.2rem; }

.form-input,
.form-select,
.form-textarea {
  padding: 0.55rem 0.75rem;
  border: 1px solid var(--border);
  border-radius: 8px;
  background: var(--surface);
  color: var(--text);
  font-size: 0.875rem;
  transition: border-color 0.15s;
}

.form-input:focus,
.form-select:focus,
.form-textarea:focus {
  outline: none;
  border-color: var(--accent);
}

.form-textarea { resize: vertical; }

.field-error {
  font-size: 0.75rem;
  color: #ef4444;
}

.action-item-input-row {
  display: flex;
  gap: 0.5rem;
}

.action-item-input-row .form-input { flex: 1; }

.action-item-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.action-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.4rem 0.6rem;
  background: var(--surface-muted);
  border-radius: 6px;
  font-size: 0.875rem;
}

.action-item-bullet { color: var(--accent); font-weight: 700; }

.action-item-text { flex: 1; color: var(--text-secondary); }

.action-item-remove {
  background: none;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  font-size: 1rem;
  padding: 0 0.2rem;
  line-height: 1;
}

.action-item-remove:hover { color: #ef4444; }

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
}

.btn {
  padding: 0.55rem 1.25rem;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  border: none;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  transition: all 0.15s;
}

.btn--primary {
  background: var(--accent);
  color: #fff;
}

.btn--primary:hover:not(:disabled) { opacity: 0.88; }
.btn--primary:disabled { opacity: 0.5; cursor: not-allowed; }

.btn--secondary {
  background: var(--surface-muted);
  color: var(--text-secondary);
}

.btn--secondary:hover:not(:disabled) { background: var(--border); }
.btn--secondary:disabled { opacity: 0.5; cursor: not-allowed; }

.btn--sm {
  padding: 0.35rem 0.85rem;
  font-size: 0.8rem;
}

.alert {
  padding: 0.75rem 1rem;
  border-radius: 8px;
  font-size: 0.875rem;
}

.alert--error {
  background: #fee2e2;
  color: #991b1b;
  border: 1px solid #fca5a5;
}

.alert--success {
  background: #d1fae5;
  color: #065f46;
  border: 1px solid #6ee7b7;
}
</style>
