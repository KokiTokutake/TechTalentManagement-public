<template>
  <main class="container">
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
  </main>
</template>

<script setup lang="ts">
import { useAuthStore } from '~/stores/auth'
import { useCareerPlanStore } from '~/stores/careerPlan'

const route = useRoute()
const auth = useAuthStore()
const store = useCareerPlanStore()

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
  await store.fetchIntegratedView(targetUserId, auth.currentUser.id)
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
.plan-label { font-size: 0.75rem; font-weight: 600; color: #6b7280; text-transform: uppercase; letter-spacing: 0.05em; }
.plan-text { margin-top: 0.25rem; white-space: pre-wrap; }
.empty-note { color: #9ca3af; font-size: 0.875rem; }
.skill-list { list-style: none; padding: 0; display: flex; flex-direction: column; gap: 0.5rem; }
.skill-item { display: flex; align-items: center; gap: 0.75rem; padding: 0.5rem 0.75rem; background: #f9fafb; border-radius: 0.375rem; }
.skill-category { font-size: 0.75rem; color: #6b7280; }
.skill-name { font-weight: 500; flex: 1; }
.skill-level { font-size: 0.75rem; color: #3b82f6; font-weight: 600; }
.goal-list { list-style: none; padding: 0; display: flex; flex-direction: column; gap: 0.5rem; }
.goal-item { display: flex; align-items: center; gap: 0.75rem; padding: 0.5rem 0.75rem; background: #f9fafb; border-radius: 0.375rem; }
.badge { font-size: 0.7rem; font-weight: 600; padding: 0.15rem 0.5rem; border-radius: 9999px; }
.badge.must { background: #fee2e2; color: #dc2626; }
.badge.will { background: #dbeafe; color: #2563eb; }
.badge.can { background: #dcfce7; color: #16a34a; }
.goal-title { flex: 1; }
.comment-list { list-style: none; padding: 0; display: flex; flex-direction: column; gap: 0.75rem; }
.comment-item { padding: 0.75rem; background: #f9fafb; border-radius: 0.375rem; border-left: 3px solid #d1d5db; }
.comment-meta { font-size: 0.75rem; color: #6b7280; margin-bottom: 0.25rem; }
.comment-content { margin: 0; white-space: pre-wrap; }
.comment-form { margin-top: 1.25rem; display: flex; flex-direction: column; gap: 0.75rem; }
.textarea { width: 100%; padding: 0.5rem 0.75rem; border: 1px solid #d1d5db; border-radius: 0.375rem; font-size: 0.875rem; resize: vertical; }
.error-inline { color: #dc2626; font-size: 0.875rem; }
.form-footer { display: flex; justify-content: flex-end; }
</style>
