<template>
  <main class="container">
    <header class="page-header">
      <h1>自己評価</h1>
    </header>

    <div v-if="store.loading" class="loading">読み込み中…</div>
    <p v-else-if="store.error" class="error" role="alert">{{ store.error }}</p>

    <template v-else>
      <div v-if="store.isSubmitted" class="submitted-banner" role="status">
        自己評価は提出済みです。内容の変更はできません。
      </div>

      <div v-if="store.formItems.length === 0" class="empty">
        この考課期間に登録された目標がありません。先に目標を登録してください。
      </div>

      <div v-else class="eval-list">
        <SelfEvaluationItem
          v-for="item in store.formItems"
          :key="item.goalId"
          :item="item"
          :disabled="store.isSubmitted"
          @update:score="score => onScoreChange(item.goalId, score)"
          @update:comment="comment => onCommentChange(item.goalId, comment)"
        />
      </div>

      <footer v-if="!store.isSubmitted && store.formItems.length > 0" class="action-footer">
        <span class="save-status">
          <template v-if="store.saveStatus === 'saving'">保存中…</template>
          <template v-else-if="store.saveStatus === 'saved'">✓ 下書き保存済み</template>
          <template v-else-if="store.saveStatus === 'error'">⚠ 保存に失敗しました</template>
        </span>

        <p v-if="store.submitError" class="submit-error" role="alert">{{ store.submitError }}</p>

        <button
          class="btn-submit"
          :disabled="submitting"
          @click="handleSubmit"
        >
          {{ submitting ? '提出中…' : '自己評価を提出する' }}
        </button>
      </footer>
    </template>
  </main>
</template>

<script setup lang="ts">
const route = useRoute()
const periodId = route.params.periodId as string
const auth = useAuthStore()
const userId = computed(() => auth.currentUser!.id)

const store = useSelfEvaluationStore()
const goalsApi = useGoalsApi()
const submitting = ref(false)

onMounted(async () => {
  const goals = await goalsApi.fetchGoals(periodId, userId.value)
  await store.load(periodId, userId.value, goals)
})

function onScoreChange(goalId: string, score: number) {
  const item = store.formItems.find(i => i.goalId === goalId)
  if (item) {
    item.achievementScore = score
    store.scheduleAutoSave(periodId, userId.value)
  }
}

function onCommentChange(goalId: string, comment: string) {
  const item = store.formItems.find(i => i.goalId === goalId)
  if (item) {
    item.comment = comment
    store.scheduleAutoSave(periodId, userId.value)
  }
}

async function handleSubmit() {
  submitting.value = true
  await store.submitEvaluation(periodId, userId.value)
  submitting.value = false
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

.submitted-banner {
  background: #f0fdf4;
  border: 1px solid #86efac;
  border-radius: 8px;
  padding: 0.75rem 1rem;
  color: #166534;
  font-weight: 600;
  margin-bottom: 1.5rem;
}

.empty {
  color: #9ca3af;
  font-size: 0.875rem;
  padding: 2rem;
  border: 1px dashed #e5e7eb;
  border-radius: 8px;
  text-align: center;
}

.eval-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
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

.submit-error {
  color: #ef4444;
  font-size: 0.85rem;
  margin: 0;
}

.btn-submit {
  background: #16a34a;
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 0.65rem 1.75rem;
  font-weight: 700;
  font-size: 1rem;
  cursor: pointer;
  transition: background 0.15s;
}

.btn-submit:hover:not(:disabled) {
  background: #15803d;
}

.btn-submit:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
