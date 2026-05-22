<template>
  <div class="page-wrap">
    <header class="page-header">
      <NuxtLink to="/mdc/nine-box" class="back-link">← 9ボックスマップ</NuxtLink>
      <h1>9ボックス評価入力</h1>
      <p class="subtitle">{{ talentName }} の評価を入力します</p>
    </header>

    <!-- 評価期間外の警告 -->
    <div v-if="isPeriodClosed" class="alert alert--warning" data-testid="nine-box-period-closed-message">
      評価期間が終了しています。新しい評価を入力できません。
    </div>

    <!-- 読み込み中 -->
    <div v-else-if="store.loading && !form.performanceLevel" class="loading">読み込み中…</div>

    <!-- エラー -->
    <div v-else-if="store.error" class="alert alert--error">{{ store.error }}</div>

    <form v-else class="form-card" @submit.prevent="handleSave">
      <!-- パフォーマンス評価 -->
      <div class="form-group">
        <label class="form-label" for="performance-select">
          パフォーマンス評価 <span class="required">*</span>
        </label>
        <select
          id="performance-select"
          v-model="form.performanceLevel"
          class="form-select"
          :disabled="isPeriodClosed"
          required
          data-testid="nine-box-performance-select"
        >
          <option value="">選択してください</option>
          <option value="High">High（高い）</option>
          <option value="Mid">Mid（標準）</option>
          <option value="Low">Low（要改善）</option>
        </select>
      </div>

      <!-- ポテンシャル評価 -->
      <div class="form-group">
        <label class="form-label" for="potential-select">
          ポテンシャル評価 <span class="required">*</span>
        </label>
        <select
          id="potential-select"
          v-model="form.potentialLevel"
          class="form-select"
          :disabled="isPeriodClosed"
          required
          data-testid="nine-box-potential-select"
        >
          <option value="">選択してください</option>
          <option value="High">High（高い）</option>
          <option value="Mid">Mid（標準）</option>
          <option value="Low">Low（要改善）</option>
        </select>
      </div>

      <!-- セルプレビュー -->
      <div v-if="form.performanceLevel && form.potentialLevel" class="cell-preview">
        <span class="cell-preview__label">配置セル:</span>
        <span
          class="cell-preview__badge"
          :style="{ backgroundColor: store.getCellColor(form.performanceLevel, form.potentialLevel) }"
        >
          {{ store.getCellLabel(form.performanceLevel, form.potentialLevel) }}
        </span>
        <span v-if="isHighPotential" class="cell-preview__hp-badge">重点管理候補</span>
      </div>

      <!-- パフォーマンスコメント -->
      <div class="form-group">
        <label class="form-label" for="performance-comment">
          パフォーマンスコメント
        </label>
        <textarea
          id="performance-comment"
          v-model="form.performanceRationale"
          class="form-textarea"
          :disabled="isPeriodClosed"
          rows="4"
          placeholder="業績・成果の根拠・具体的な事実を記入してください"
          maxlength="1000"
          data-testid="nine-box-performance-comment"
        />
        <span class="char-count">{{ form.performanceRationale?.length ?? 0 }} / 1000</span>
      </div>

      <!-- ポテンシャルコメント -->
      <div class="form-group">
        <label class="form-label" for="potential-comment">
          ポテンシャルコメント
        </label>
        <textarea
          id="potential-comment"
          v-model="form.potentialRationale"
          class="form-textarea"
          :disabled="isPeriodClosed"
          rows="4"
          placeholder="成長可能性・学習意欲・将来性の根拠を記入してください"
          maxlength="1000"
          data-testid="nine-box-potential-comment"
        />
        <span class="char-count">{{ form.potentialRationale?.length ?? 0 }} / 1000</span>
      </div>

      <!-- 保存ボタン -->
      <div class="form-actions">
        <NuxtLink to="/mdc/nine-box" class="btn btn--secondary">キャンセル</NuxtLink>
        <button
          type="submit"
          class="btn btn--primary"
          :disabled="isPeriodClosed || store.loading || !form.performanceLevel || !form.potentialLevel"
          data-testid="nine-box-save-button"
        >
          {{ store.loading ? '保存中…' : (isUpdate ? '更新する' : '保存する') }}
        </button>
      </div>

      <!-- 成功メッセージ -->
      <div v-if="saved" class="alert alert--success" data-testid="nine-box-saved-message">
        評価を{{ isUpdate ? '更新' : '保存' }}しました。
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import type { NineBoxLevel } from '~/types/nineBox'

const route = useRoute()
const auth = useAuthStore()
const store = useNineBoxStore()

const talentId = computed(() => route.query.talentId as string)
const periodId = computed(() => route.query.periodId as string)
const talentName = computed(() => route.query.talentName as string ?? 'タレント')

const form = reactive({
  performanceLevel: '' as NineBoxLevel | '',
  potentialLevel: '' as NineBoxLevel | '',
  performanceRationale: '',
  potentialRationale: '',
})

const isUpdate = ref(false)
const existingAssessmentId = ref<string | null>(null)
const saved = ref(false)

const isPeriodClosed = computed(() => {
  const period = auth.currentPeriod
  if (!period) return false
  return period.currentPhase === 'Closed'
})

const isHighPotential = computed(() => {
  if (!form.performanceLevel || !form.potentialLevel) return false
  const perf = form.performanceLevel as NineBoxLevel
  const pot = form.potentialLevel as NineBoxLevel
  return (perf === 'High' && (pot === 'High' || pot === 'Mid')) ||
    (perf === 'Mid' && pot === 'High')
})

onMounted(async () => {
  if (!talentId.value || !periodId.value) {
    await navigateTo('/mdc/nine-box')
    return
  }
  if (!auth.currentUser) {
    await navigateTo('/')
    return
  }

  await store.loadCellDefinitions()
  await store.loadCurrentAssessment(talentId.value, periodId.value)

  if (store.currentAssessment) {
    isUpdate.value = true
    existingAssessmentId.value = store.currentAssessment.id
    form.performanceLevel = store.currentAssessment.performanceLevel
    form.potentialLevel = store.currentAssessment.potentialLevel
    form.performanceRationale = store.currentAssessment.performanceRationale ?? ''
    form.potentialRationale = store.currentAssessment.potentialRationale ?? ''
  }
})

async function handleSave() {
  if (!auth.currentUser || !form.performanceLevel || !form.potentialLevel) return

  saved.value = false
  try {
    if (isUpdate.value && existingAssessmentId.value) {
      await store.updateAssessment(
        existingAssessmentId.value,
        {
          performanceLevel: form.performanceLevel as NineBoxLevel,
          potentialLevel: form.potentialLevel as NineBoxLevel,
          performanceRationale: form.performanceRationale || null,
          potentialRationale: form.potentialRationale || null,
        },
        auth.currentUser.id,
      )
    } else {
      await store.saveAssessment(
        {
          talentId: talentId.value,
          periodId: periodId.value,
          performanceLevel: form.performanceLevel as NineBoxLevel,
          potentialLevel: form.potentialLevel as NineBoxLevel,
          performanceRationale: form.performanceRationale || null,
          potentialRationale: form.potentialRationale || null,
        },
        auth.currentUser.id,
      )
      isUpdate.value = true
      existingAssessmentId.value = store.currentAssessment?.id ?? null
    }
    saved.value = true
  } catch {
    // store.error に設定済み
  }
}
</script>

<style scoped>
.page-header { margin-bottom: 2rem; }

h1 {
  font-size: 1.5rem;
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

.form-card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 2rem;
  max-width: 640px;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-label {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text);
}

.required {
  color: #ef4444;
  margin-left: 0.2rem;
}

.form-select,
.form-textarea {
  padding: 0.6rem 0.75rem;
  border: 1px solid var(--border);
  border-radius: 8px;
  background: var(--surface);
  color: var(--text);
  font-size: 0.9rem;
  transition: border-color 0.15s;
}

.form-select:focus,
.form-textarea:focus {
  outline: none;
  border-color: var(--accent);
}

.form-select:disabled,
.form-textarea:disabled {
  background: var(--surface-muted);
  color: var(--text-muted);
  cursor: not-allowed;
}

.form-textarea { resize: vertical; }

.char-count {
  font-size: 0.75rem;
  color: var(--text-muted);
  text-align: right;
}

.cell-preview {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  background: var(--surface-muted);
  border-radius: 8px;
}

.cell-preview__label {
  font-size: 0.8rem;
  color: var(--text-secondary);
}

.cell-preview__badge {
  font-size: 0.8rem;
  font-weight: 700;
  color: #fff;
  padding: 0.2rem 0.75rem;
  border-radius: 999px;
}

.cell-preview__hp-badge {
  font-size: 0.75rem;
  font-weight: 600;
  background: #fef3c7;
  color: #92400e;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding-top: 0.5rem;
}

.btn {
  padding: 0.55rem 1.25rem;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  border: none;
  transition: all 0.15s;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
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
.btn--secondary:hover { background: var(--border); }

.alert {
  padding: 0.75rem 1rem;
  border-radius: 8px;
  font-size: 0.875rem;
}

.alert--warning {
  background: #fef3c7;
  color: #92400e;
  border: 1px solid #fde68a;
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

.loading {
  text-align: center;
  padding: 3rem;
  color: var(--text-secondary);
}
</style>
