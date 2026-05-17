<template>
  <main class="container">
    <header class="page-header">
      <h1>上長評価</h1>
      <!-- T-002（認証）実装まで managerId / periodId / talentUserId はクエリパラメータで受け取る暫定仕様 -->
      <p v-if="missingParams" class="warn">
        URL に ?managerId=...&amp;periodId=...&amp;talentUserId=... を付けてアクセスしてください（認証未実装）
      </p>
    </header>

    <div v-if="store.loading" class="loading">読み込み中…</div>
    <p v-else-if="store.error" class="error" role="alert">{{ store.error }}</p>

    <template v-else>
      <div v-if="store.isConfirmed" class="confirmed-banner" role="status">
        上長評価は確定済みです。内容の変更はできません。
      </div>

      <p v-if="!store.selfEvaluation?.isSubmitted" class="warn">
        タレントがまだ自己評価を提出していません。
      </p>

      <!-- タレントの自己評価サマリー -->
      <section v-if="store.selfEvaluation && store.goals.length > 0" class="section">
        <h2 class="section-title">タレントの自己評価</h2>
        <div class="goal-eval-list">
          <div
            v-for="goal in store.goals"
            :key="goal.id"
            class="goal-eval-row"
          >
            <div class="goal-row-header">
              <span class="category-badge" :class="`badge-${goal.category.toLowerCase()}`">
                {{ goal.category }}
              </span>
              <span class="goal-title">{{ goal.title }}</span>
            </div>
            <div v-if="selfItemFor(goal.id)" class="talent-scores">
              <span class="score-chip">達成度 {{ selfItemFor(goal.id)!.achievementScore }}</span>
              <span v-if="selfItemFor(goal.id)!.comment" class="talent-comment">
                {{ selfItemFor(goal.id)!.comment }}
              </span>
              <span v-else class="no-comment">コメントなし</span>
            </div>
            <div v-else class="talent-scores no-input">未入力</div>
          </div>
        </div>
      </section>

      <!-- 上長評価入力 -->
      <section class="section">
        <h2 class="section-title">上長評価入力</h2>

        <div class="field">
          <label class="label">総合評価スコア <span class="required">*</span></label>
          <div class="score-buttons">
            <button
              v-for="n in 5"
              :key="n"
              type="button"
              class="score-btn"
              :class="{ active: store.overallScore === n }"
              :disabled="store.isConfirmed || missingParams"
              :aria-label="`総合評価 ${n}`"
              @click="onScoreChange(n)"
            >
              {{ n }}
            </button>
          </div>
          <span v-if="store.overallScore === 0" class="unscored">未入力</span>
        </div>

        <div class="field">
          <label for="feedback" class="label">フィードバックコメント</label>
          <textarea
            id="feedback"
            v-model="feedbackBuffer"
            :disabled="store.isConfirmed || missingParams"
            rows="6"
            maxlength="2000"
            placeholder="タレントへのフィードバック・コメントを記入（任意）"
            @input="onCommentChange"
          />
        </div>
      </section>

      <footer v-if="!store.isConfirmed && !missingParams" class="action-footer">
        <span class="save-status">
          <template v-if="store.saveStatus === 'saving'">保存中…</template>
          <template v-else-if="store.saveStatus === 'saved'">✓ 下書き保存済み</template>
          <template v-else-if="store.saveStatus === 'error'">⚠ 保存に失敗しました</template>
        </span>

        <p v-if="store.confirmError" class="confirm-error" role="alert">{{ store.confirmError }}</p>

        <button
          class="btn-confirm"
          :disabled="confirming || !store.selfEvaluation?.isSubmitted"
          @click="handleConfirm"
        >
          {{ confirming ? '確定中…' : '上長評価を確定する' }}
        </button>
      </footer>
    </template>
  </main>
</template>

<script setup lang="ts">
import type { SelfEvaluationItem } from '~/types/selfEvaluation'

const route = useRoute()
const selfEvaluationId = route.params.selfEvaluationId as string
const managerId = route.query.managerId as string | undefined
const periodId = route.query.periodId as string | undefined
const talentUserId = route.query.talentUserId as string | undefined

const missingParams = computed(() => !managerId || !periodId || !talentUserId)

const store = useManagerEvaluationStore()
const confirming = ref(false)
const feedbackBuffer = ref('')

onMounted(async () => {
  if (missingParams.value) return
  await store.load(selfEvaluationId, managerId!, periodId!, talentUserId!)
  feedbackBuffer.value = store.feedbackComment
})

function selfItemFor(goalId: string): SelfEvaluationItem | undefined {
  return store.selfEvaluation?.items.find(i => i.goalId === goalId)
}

function onScoreChange(score: number) {
  store.overallScore = score
  store.scheduleAutoSave(selfEvaluationId, managerId!)
}

function onCommentChange() {
  store.feedbackComment = feedbackBuffer.value
  store.scheduleAutoSave(selfEvaluationId, managerId!)
}

async function handleConfirm() {
  if (!managerId) return
  confirming.value = true
  await store.confirmEvaluation(selfEvaluationId, managerId)
  confirming.value = false
}
</script>

<style scoped>
.container {
  max-width: 720px;
  margin: 0 auto;
  padding: 2rem 1rem 6rem;
  font-family: sans-serif;
}

.page-header {
  margin-bottom: 2rem;
}

.page-header h1 {
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0 0 0.25rem;
}

.warn {
  color: #d97706;
  font-size: 0.875rem;
  background: #fffbeb;
  border: 1px solid #fcd34d;
  border-radius: 6px;
  padding: 0.5rem 0.75rem;
  margin: 0;
}

.loading {
  color: #6b7280;
  text-align: center;
  padding: 3rem 0;
}

.error {
  color: #ef4444;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 6px;
  padding: 0.75rem 1rem;
}

.confirmed-banner {
  background: #f0fdf4;
  border: 1px solid #86efac;
  border-radius: 8px;
  padding: 0.75rem 1rem;
  color: #166534;
  font-weight: 600;
  margin-bottom: 1.5rem;
}

.section {
  margin-bottom: 2rem;
}

.section-title {
  font-size: 1rem;
  font-weight: 700;
  color: #374151;
  margin: 0 0 0.75rem;
  padding-bottom: 0.5rem;
  border-bottom: 2px solid #e5e7eb;
}

.goal-eval-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.goal-eval-row {
  background: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 0.875rem 1rem;
}

.goal-row-header {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  margin-bottom: 0.5rem;
}

.goal-title {
  font-weight: 600;
  font-size: 0.9rem;
  color: #111827;
}

.category-badge {
  font-size: 0.72rem;
  font-weight: 700;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
  flex-shrink: 0;
}

.badge-will { background: #dbeafe; color: #1d4ed8; }
.badge-can  { background: #d1fae5; color: #065f46; }
.badge-must { background: #fee2e2; color: #991b1b; }

.talent-scores {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  font-size: 0.875rem;
  color: #374151;
}

.talent-scores.no-input {
  color: #9ca3af;
}

.score-chip {
  background: #eff6ff;
  color: #1d4ed8;
  border-radius: 999px;
  padding: 0.15rem 0.6rem;
  font-weight: 700;
  font-size: 0.8rem;
  flex-shrink: 0;
}

.talent-comment {
  color: #4b5563;
  white-space: pre-wrap;
}

.no-comment {
  color: #9ca3af;
  font-style: italic;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  margin-bottom: 1.25rem;
}

.label {
  font-size: 0.875rem;
  font-weight: 600;
  color: #374151;
}

.required { color: #ef4444; }

.score-buttons {
  display: flex;
  gap: 0.5rem;
}

.score-btn {
  width: 2.75rem;
  height: 2.75rem;
  border-radius: 8px;
  border: 2px solid #e5e7eb;
  background: #f9fafb;
  font-weight: 700;
  font-size: 1rem;
  cursor: pointer;
  color: #374151;
  transition: all 0.12s;
}

.score-btn:hover:not(:disabled) {
  border-color: #7c3aed;
  background: #f5f3ff;
  color: #7c3aed;
}

.score-btn.active {
  border-color: #7c3aed;
  background: #7c3aed;
  color: #fff;
}

.score-btn:disabled {
  cursor: not-allowed;
}

.unscored {
  font-size: 0.8rem;
  color: #ef4444;
}

textarea {
  border: 1px solid #d1d5db;
  border-radius: 6px;
  padding: 0.6rem 0.75rem;
  font-size: 0.95rem;
  resize: vertical;
  outline: none;
  transition: border-color 0.15s;
  font-family: inherit;
}

textarea:focus {
  border-color: #7c3aed;
  box-shadow: 0 0 0 3px rgba(124, 58, 237, 0.1);
}

textarea:disabled {
  background: #f3f4f6;
}

.action-footer {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  background: #fff;
  border-top: 1px solid #e5e7eb;
  padding: 1rem 1.5rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  justify-content: flex-end;
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.06);
}

.save-status {
  font-size: 0.85rem;
  color: #6b7280;
  flex: 1;
}

.confirm-error {
  color: #ef4444;
  font-size: 0.85rem;
  margin: 0;
}

.btn-confirm {
  background: #7c3aed;
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 0.65rem 1.75rem;
  font-weight: 700;
  font-size: 1rem;
  cursor: pointer;
  transition: background 0.15s;
}

.btn-confirm:hover:not(:disabled) {
  background: #6d28d9;
}

.btn-confirm:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
