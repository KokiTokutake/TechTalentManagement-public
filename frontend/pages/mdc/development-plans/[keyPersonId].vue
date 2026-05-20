<template>
  <div class="page-wrap">
    <header class="page-header">
      <NuxtLink to="/mdc/key-persons" class="back-link">← 重点管理人物一覧</NuxtLink>
      <h1>育成方針</h1>
      <p class="subtitle">重点管理人物 ID: {{ keyPersonId }}</p>
    </header>

    <div v-if="store.loading" class="loading">読み込み中…</div>

    <div v-else-if="store.error" class="error-box">{{ store.error }}</div>

    <template v-else>
      <section v-if="canWrite" class="card">
        <h2 class="section-title">新しい育成方針を登録</h2>

        <div v-if="submitError" class="error-box form-error">{{ submitError }}</div>

        <form class="plan-form" @submit.prevent="submitPlan">
          <div class="form-group">
            <label class="form-label" for="challenge-input">課題</label>
            <textarea
              id="challenge-input"
              v-model="form.challenge"
              class="form-textarea"
              rows="3"
              placeholder="現状の課題・克服すべきポイント"
            />
          </div>

          <div class="form-group">
            <label class="form-label" for="goal-input">目標</label>
            <textarea
              id="goal-input"
              v-model="form.goal"
              class="form-textarea"
              rows="3"
              placeholder="達成すべき目標・期待する姿"
            />
          </div>

          <div class="form-group">
            <label class="form-label" for="support-input">支援アクション</label>
            <textarea
              id="support-input"
              v-model="form.supportAction"
              class="form-textarea"
              rows="3"
              placeholder="会社・上長として行う支援・育成アクション"
            />
          </div>

          <div class="form-group form-group-inline">
            <label class="form-label" for="due-date-input">期限（任意）</label>
            <input
              id="due-date-input"
              v-model="form.dueDate"
              type="date"
              class="form-input"
            />
          </div>

          <p v-if="isAllEmpty" class="hint-text">課題・目標・支援アクションのいずれかを入力してください。</p>

          <button type="submit" class="btn-primary" :disabled="submitting || isAllEmpty">
            {{ submitting ? '保存中…' : '育成方針を保存' }}
          </button>
        </form>
      </section>

      <section class="card">
        <h2 class="section-title">
          育成方針履歴
          <span class="version-count">{{ store.plans.length }} 件</span>
        </h2>

        <div v-if="store.plans.length === 0" class="empty-note">
          まだ育成方針が登録されていません。
        </div>

        <div v-else class="plan-list">
          <article
            v-for="plan in store.plans"
            :key="plan.id"
            class="plan-card"
            :class="{ 'plan-card--latest': plan.version === latestVersion }"
          >
            <div class="plan-card-header">
              <span class="version-badge" :class="plan.version === latestVersion ? 'version-badge--latest' : ''">
                v{{ plan.version }}{{ plan.version === latestVersion ? '（最新）' : '' }}
              </span>
              <span class="plan-date">{{ formatDate(plan.createdAt) }}</span>
              <span v-if="plan.dueDate" class="due-date-badge">期限: {{ plan.dueDate }}</span>
            </div>

            <dl class="plan-body">
              <template v-if="plan.challenge">
                <dt class="plan-dt">課題</dt>
                <dd class="plan-dd">{{ plan.challenge }}</dd>
              </template>
              <template v-if="plan.goal">
                <dt class="plan-dt">目標</dt>
                <dd class="plan-dd">{{ plan.goal }}</dd>
              </template>
              <template v-if="plan.supportAction">
                <dt class="plan-dt">支援アクション</dt>
                <dd class="plan-dd">{{ plan.supportAction }}</dd>
              </template>
            </dl>
          </article>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
const route = useRoute()
const auth = useAuthStore()
const store = useDevelopmentPlanStore()

const keyPersonId = route.params.keyPersonId as string

const canWrite = computed(() => auth.currentUser?.role === 'Manager')

const form = ref({ challenge: '', goal: '', supportAction: '', dueDate: '' })
const submitting = ref(false)
const submitError = ref<string | null>(null)

const isAllEmpty = computed(() =>
  !form.value.challenge.trim() && !form.value.goal.trim() && !form.value.supportAction.trim(),
)

const latestVersion = computed(() =>
  store.plans.length > 0 ? Math.max(...store.plans.map(p => p.version)) : 0,
)

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('ja-JP', {
    year: 'numeric', month: '2-digit', day: '2-digit',
  })
}

async function submitPlan() {
  if (!auth.currentUser || isAllEmpty.value) return
  submitting.value = true
  submitError.value = null
  try {
    await store.addPlan(auth.currentUser.id, keyPersonId, {
      challenge: form.value.challenge,
      goal: form.value.goal,
      supportAction: form.value.supportAction,
      dueDate: form.value.dueDate || null,
    })
    form.value = { challenge: '', goal: '', supportAction: '', dueDate: '' }
  }
  catch (e: unknown) {
    const err = e as { data?: { error?: string }, status?: number }
    if (err?.status === 403) {
      submitError.value = '直属上長のみ育成方針を登録できます。'
    }
    else if (err?.status === 409) {
      submitError.value = '解除済みの重点管理人物には育成方針を追加できません。'
    }
    else if (err?.status === 400) {
      submitError.value = err?.data?.error ?? '課題・目標・支援アクションのいずれかを入力してください。'
    }
    else {
      submitError.value = '保存に失敗しました。もう一度お試しください。'
    }
  }
  finally {
    submitting.value = false
  }
}

onMounted(async () => {
  if (auth.currentUser) {
    await store.fetchList(auth.currentUser.id, keyPersonId)
  }
})
</script>

<style scoped>
.page-header {
  margin-bottom: 2rem;
}

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

.section-title {
  font-size: 1rem;
  font-weight: 700;
  color: var(--text);
  margin: 0 0 1.25rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.version-count {
  font-size: 0.8rem;
  font-weight: 400;
  color: var(--text-muted);
}

.form-error { margin-bottom: 1rem; }

.plan-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.form-group-inline {
  flex-direction: row;
  align-items: center;
  gap: 1rem;
}

.form-label {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-secondary);
  white-space: nowrap;
}

.form-textarea,
.form-input {
  border: 1px solid var(--border-strong);
  border-radius: 8px;
  padding: 0.6rem 0.75rem;
  font-size: 0.875rem;
  color: var(--text);
  background: var(--surface);
  transition: border-color 0.12s;
}

.form-textarea:focus,
.form-input:focus {
  outline: none;
  border-color: var(--accent);
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}

.form-textarea { resize: vertical; }

.hint-text {
  font-size: 0.8rem;
  color: var(--text-muted);
  margin: 0;
}

.plan-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.plan-card {
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 1.25rem;
  background: var(--surface-muted);
}

.plan-card--latest {
  border-color: #93c5fd;
  background: #eff6ff;
}

.plan-card-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}

.version-badge {
  display: inline-block;
  background: var(--border);
  color: var(--text-secondary);
  font-size: 0.75rem;
  font-weight: 700;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
}

.version-badge--latest {
  background: var(--accent);
  color: var(--surface);
}

.plan-date {
  font-size: 0.8rem;
  color: var(--text-secondary);
}

.due-date-badge {
  display: inline-block;
  background: #fef3c7;
  color: #92400e;
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
}

.plan-body {
  display: grid;
  grid-template-columns: max-content 1fr;
  gap: 0.5rem 1rem;
  margin: 0;
}

.plan-dt {
  font-size: 0.8rem;
  font-weight: 700;
  color: var(--text-secondary);
  white-space: nowrap;
  padding-top: 0.1rem;
}

.plan-dd {
  font-size: 0.875rem;
  color: var(--text);
  margin: 0;
  white-space: pre-wrap;
  word-break: break-word;
}
</style>
