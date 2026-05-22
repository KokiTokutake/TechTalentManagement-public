<template>
  <div class="page-wrap">
    <header class="page-header">
      <NuxtLink to="/" class="back-link">← ホームへ戻る</NuxtLink>
      <h1>キャリアプラン</h1>
      <p class="subtitle" v-if="store.integratedView?.will">
        {{ store.integratedView.will.userId }} のキャリアプラン
      </p>
    </header>

    <div v-if="store.error" class="error-box" role="alert">{{ store.error }}</div>
    <div v-if="store.loading" class="loading">読み込み中...</div>

    <template v-else>
      <!-- Will -->
      <section class="section">
        <h2>Will（キャリアプラン）</h2>
        <template v-if="store.integratedView?.will">
          <div class="plan-view">
            <div class="plan-item">
              <span class="plan-label">中長期目標</span>
              <p class="plan-text">{{ store.integratedView.will.longTermGoal }}</p>
            </div>
            <div class="plan-item">
              <span class="plan-label">中期目標</span>
              <p class="plan-text">{{ store.integratedView.will.mediumTermGoal }}</p>
            </div>
            <div class="plan-item">
              <span class="plan-label">今期の注力テーマ</span>
              <p class="plan-text">{{ store.integratedView.will.currentFocus }}</p>
            </div>
          </div>
        </template>
        <p v-else class="empty-note">今期のキャリアプランはまだ作成されていません。</p>
      </section>

      <!-- Can -->
      <section class="section">
        <h2>Can（承認済みスキル）</h2>
        <p v-if="store.integratedView?.can.length === 0" class="empty-note">承認済みスキルがありません</p>
        <ul v-else-if="store.integratedView" class="skill-list">
          <li v-for="skill in store.integratedView.can" :key="skill.id" class="skill-item">
            <span class="skill-category">{{ skill.categoryName }}</span>
            <span class="skill-name">{{ skill.skillItemName }}</span>
            <span class="skill-level">Lv.{{ skill.level }}</span>
          </li>
        </ul>
      </section>

      <!-- Must -->
      <section class="section">
        <h2>Must（今期の評価目標）</h2>
        <p v-if="store.integratedView?.must.length === 0" class="empty-note">今期目標が未設定です</p>
        <ul v-else-if="store.integratedView" class="goal-list">
          <li v-for="goal in store.integratedView.must" :key="goal.id" class="goal-item">
            <span class="badge" :class="goal.category.toLowerCase()">{{ goal.category }}</span>
            <span class="goal-title">{{ goal.title }}</span>
          </li>
        </ul>
      </section>

      <!-- 面談サマリー（委員会共有分） -->
      <section class="section" data-testid="meeting-summary-section">
        <h2>1on1 面談サマリー</h2>
        <div v-if="oneOnOneStore.summary" class="meeting-summary">
          <div class="summary-meta">
            <span class="summary-stat">
              最終面談:
              <strong data-testid="meeting-summary-last-date">
                {{ oneOnOneStore.summary.lastMeetingAt ? oneOnOneStore.summary.lastMeetingAt.replace(/-/g, '/') : '—' }}
              </strong>
            </span>
            <span class="summary-stat">
              合計:
              <strong>{{ oneOnOneStore.summary.totalMeetingCount }}件</strong>
            </span>
          </div>
          <div v-if="oneOnOneStore.summary.sharedMeetings.length > 0">
            <p class="summary-label">委員会共有の面談（最大3件）</p>
            <ul class="shared-meeting-list" data-testid="meeting-summary-shared-list">
              <li
                v-for="m in oneOnOneStore.summary.sharedMeetings"
                :key="m.id"
                class="shared-meeting-item"
                :data-testid="`meeting-summary-item-${m.id}`"
              >
                <span class="shared-meeting-date">{{ m.conductedAt.replace(/-/g, '/') }}</span>
                <span class="shared-meeting-notes">{{ m.notesSummary }}</span>
              </li>
            </ul>
          </div>
          <p v-else class="empty-note">委員会に共有された面談記録はありません。</p>
        </div>
        <p v-else class="empty-note">面談サマリーを読み込み中…</p>
      </section>

      <!-- 上長コメント -->
      <section class="section" v-if="store.integratedView?.will">
        <h2>コメント</h2>
        <ul v-if="store.comments.length > 0" class="comment-list">
          <li v-for="c in store.comments" :key="c.id" class="comment-item">
            <div class="comment-meta">{{ c.authorName }} · {{ formatDate(c.createdAt) }}</div>
            <p class="comment-content">{{ c.content }}</p>
          </li>
        </ul>
        <p v-else class="empty-note">コメントはまだありません</p>

        <!-- 直属上長のみコメント投稿フォームを表示 -->
        <form v-if="canComment" class="comment-form" @submit.prevent="submitComment">
          <div class="form-group">
            <label class="label">コメントを追加</label>
            <textarea v-model="commentText" class="textarea" rows="3" maxlength="1000"
              placeholder="タレントへのフィードバックを入力してください" required />
          </div>
          <p v-if="commentError" class="error-inline">{{ commentError }}</p>
          <div class="form-footer">
            <button class="btn-primary" type="submit" :disabled="commentSaving">
              {{ commentSaving ? '送信中…' : 'コメントを送信' }}
            </button>
          </div>
        </form>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '~/stores/auth'
import { useCareerPlanStore } from '~/stores/careerPlan'
import { useOneOnOneStore } from '~/stores/oneOnOne'

const route = useRoute()
const auth = useAuthStore()
const store = useCareerPlanStore()
const oneOnOneStore = useOneOnOneStore()

const targetUserId = route.params.userId as string
const commentText = ref('')
const commentSaving = ref(false)
const commentError = ref<string | null>(null)

const canComment = computed(() => {
  const role = auth.currentUser?.role
  return role === 'Manager'
})

onMounted(async () => {
  if (!auth.currentUser) return
  store.$reset()
  await Promise.all([
    store.fetchIntegratedView(targetUserId, auth.currentUser.id),
    oneOnOneStore.fetchSummary(targetUserId),
  ])
  const planId = store.integratedView?.will?.id
  if (planId)
    await store.fetchComments(planId, auth.currentUser.id)
})

async function submitComment() {
  if (!auth.currentUser || !store.integratedView?.will) return
  commentSaving.value = true
  commentError.value = null
  try {
    await store.addComment(store.integratedView.will.id, auth.currentUser.id, commentText.value)
    commentText.value = ''
  }
  catch (e: unknown) {
    const err = e as { data?: { error?: string } }
    commentError.value = err?.data?.error ?? 'コメントの送信に失敗しました。'
  }
  finally {
    commentSaving.value = false
  }
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleString('ja-JP', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' })
}
</script>

<style scoped>
.plan-view { display: flex; flex-direction: column; gap: 1rem; }
.plan-item { }
.plan-label { font-size: 0.75rem; font-weight: 600; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.05em; }
.plan-text { margin-top: 0.25rem; white-space: pre-wrap; }
.skill-list { list-style: none; padding: 0; display: flex; flex-direction: column; gap: 0.5rem; }
.skill-item { display: flex; align-items: center; gap: 0.75rem; padding: 0.5rem 0.75rem; background: var(--surface-muted); border-radius: 0.375rem; }
.skill-category { font-size: 0.75rem; color: var(--text-secondary); }
.skill-name { font-weight: 500; flex: 1; }
.skill-level { font-size: 0.75rem; color: var(--accent); font-weight: 600; }
.goal-list { list-style: none; padding: 0; display: flex; flex-direction: column; gap: 0.5rem; }
.goal-item { display: flex; align-items: center; gap: 0.75rem; padding: 0.5rem 0.75rem; background: var(--surface-muted); border-radius: 0.375rem; }
.badge { font-size: 0.7rem; font-weight: 600; padding: 0.15rem 0.5rem; border-radius: 9999px; }
.badge.must { background: #fee2e2; color: var(--danger); }
.badge.will { background: #dbeafe; color: var(--accent); }
.badge.can { background: #dcfce7; color: #16a34a; }
.goal-title { flex: 1; }
.comment-list { list-style: none; padding: 0; display: flex; flex-direction: column; gap: 0.75rem; }
.comment-item { padding: 0.75rem; background: var(--surface-muted); border-radius: 0.375rem; border-left: 3px solid var(--border-strong); }
.comment-meta { font-size: 0.75rem; color: var(--text-secondary); margin-bottom: 0.25rem; }
.comment-content { margin: 0; white-space: pre-wrap; }
.comment-form { margin-top: 1.25rem; display: flex; flex-direction: column; gap: 0.75rem; }
.textarea { width: 100%; padding: 0.5rem 0.75rem; border: 1px solid var(--border-strong); border-radius: 0.375rem; font-size: 0.875rem; resize: vertical; }
.error-inline { color: var(--danger); font-size: 0.875rem; }
.form-footer { display: flex; justify-content: flex-end; }
.meeting-summary { display: flex; flex-direction: column; gap: 0.75rem; }
.summary-meta { display: flex; gap: 1.5rem; flex-wrap: wrap; }
.summary-stat { font-size: 0.875rem; color: var(--text-secondary); }
.summary-stat strong { color: var(--text); }
.summary-label { font-size: 0.75rem; font-weight: 600; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.05em; margin: 0 0 0.4rem; }
.shared-meeting-list { list-style: none; padding: 0; display: flex; flex-direction: column; gap: 0.4rem; }
.shared-meeting-item { display: flex; gap: 0.75rem; align-items: flex-start; padding: 0.5rem 0.75rem; background: var(--surface-muted); border-radius: 6px; border-left: 3px solid var(--accent); font-size: 0.85rem; }
.shared-meeting-date { font-family: var(--font-mono); font-weight: 600; color: var(--text); white-space: nowrap; flex-shrink: 0; }
.shared-meeting-notes { color: var(--text-secondary); line-height: 1.4; }
</style>
