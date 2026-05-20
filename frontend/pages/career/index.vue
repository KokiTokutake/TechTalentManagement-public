<template>
  <div class="page-wrap">
    <header class="page-header">
      <h1>キャリアプラン</h1>
      <p class="page-subtitle">{{ auth.currentUser?.displayName }} さんのキャリアプラン</p>
    </header>

    <!-- Key person banner -->
    <div v-if="keyPersonStore.myStatus?.isActive" class="kp-banner">
      <span class="kp-badge">⭐ 重点管理人物</span>
      <span v-if="keyPersonStore.myStatus.reason" class="kp-reason">{{ keyPersonStore.myStatus.reason }}</span>
    </div>

    <div v-if="store.error" class="error-box" role="alert">{{ store.error }}</div>
    <div v-if="store.loading" class="loading">読み込み中...</div>

    <template v-else>
      <!-- Year selector -->
      <section class="section" v-if="store.years.length > 0">
        <div class="year-row">
          <label class="label">年度を選択</label>
          <select v-model.number="selectedYear" class="select year-select" @change="onYearChange">
            <option v-for="y in store.years" :key="y" :value="y">{{ y }}年度</option>
          </select>
        </div>
      </section>

      <!-- Will -->
      <section class="section">
        <div class="card">
          <h2 class="section-title">Will — キャリアプラン</h2>
          <p class="section-desc">中長期目標・今期の注力テーマを記録します</p>

          <template v-if="isCurrentYear && !planSaved">
            <form class="plan-form" @submit.prevent="submitCreate">
              <div class="form-group">
                <label class="label">中長期目標（3〜5年後のなりたい姿）</label>
                <textarea v-model="form.longTermGoal" class="textarea" rows="3" maxlength="1000" required
                  placeholder="例：テックリードとして組織の技術戦略を牽引する" />
              </div>
              <div class="form-group">
                <label class="label">中期目標（1〜2年で達成したいこと）</label>
                <textarea v-model="form.mediumTermGoal" class="textarea" rows="3" maxlength="1000" required
                  placeholder="例：設計スキルを高め、アーキテクチャレビューを担当できるようになる" />
              </div>
              <div class="form-group">
                <label class="label">今期の注力テーマ</label>
                <textarea v-model="form.currentFocus" class="textarea" rows="2" maxlength="1000" required
                  placeholder="例：クリーンアーキテクチャの実践と社内勉強会の開催" />
              </div>
              <div class="form-footer">
                <button class="btn-primary" type="submit" :disabled="saving">
                  {{ saving ? '保存中…' : 'キャリアプランを作成する' }}
                </button>
              </div>
            </form>
          </template>

          <template v-else-if="currentPlan">
            <div v-if="!editing" class="plan-view">
              <div class="plan-item">
                <span class="plan-label">中長期目標</span>
                <p class="plan-text">{{ currentPlan.longTermGoal }}</p>
              </div>
              <div class="plan-item">
                <span class="plan-label">中期目標</span>
                <p class="plan-text">{{ currentPlan.mediumTermGoal }}</p>
              </div>
              <div class="plan-item">
                <span class="plan-label">今期の注力テーマ</span>
                <p class="plan-text">{{ currentPlan.currentFocus }}</p>
              </div>
              <div class="plan-footer" v-if="isCurrentYear">
                <button class="btn-secondary" @click="startEdit">編集する</button>
              </div>
              <p v-else class="readonly-note">過去のキャリアプランは読み取り専用です</p>
            </div>

            <form v-else class="plan-form" @submit.prevent="submitUpdate">
              <div class="form-group">
                <label class="label">中長期目標</label>
                <textarea v-model="form.longTermGoal" class="textarea" rows="3" maxlength="1000" required />
              </div>
              <div class="form-group">
                <label class="label">中期目標</label>
                <textarea v-model="form.mediumTermGoal" class="textarea" rows="3" maxlength="1000" required />
              </div>
              <div class="form-group">
                <label class="label">今期の注力テーマ</label>
                <textarea v-model="form.currentFocus" class="textarea" rows="2" maxlength="1000" required />
              </div>
              <div class="form-footer">
                <button class="btn-ghost" type="button" @click="editing = false">キャンセル</button>
                <button class="btn-primary" type="submit" :disabled="saving">
                  {{ saving ? '保存中…' : '保存する' }}
                </button>
              </div>
            </form>
          </template>

          <template v-else>
            <p class="empty-note">{{ selectedYear }}年度のキャリアプランはまだ作成されていません。</p>
          </template>

          <p v-if="saveError" class="error-inline">{{ saveError }}</p>
        </div>
      </section>

      <!-- Can -->
      <section class="section">
        <div class="card">
          <h2 class="section-title">Can — 承認済みスキル</h2>
          <template v-if="isCurrentYear && store.integratedView">
            <p v-if="store.integratedView.can.length === 0" class="empty-note">承認済みスキルがありません</p>
            <ul v-else class="skill-list">
              <li v-for="skill in store.integratedView.can" :key="skill.id" class="skill-item">
                <span class="skill-category">{{ skill.categoryName }}</span>
                <span class="skill-name">{{ skill.skillItemName }}</span>
                <span class="level-pill">Lv.{{ skill.level }}</span>
              </li>
            </ul>
          </template>
          <p v-else class="empty-note">今期のデータのみ表示されます</p>
        </div>
      </section>

      <!-- Must -->
      <section class="section">
        <div class="card">
          <h2 class="section-title">Must — 今期の評価目標</h2>
          <template v-if="isCurrentYear && store.integratedView">
            <p v-if="store.integratedView.must.length === 0" class="empty-note">今期目標が未設定です</p>
            <ul v-else class="goal-list">
              <li v-for="goal in store.integratedView.must" :key="goal.id" class="goal-item">
                <span class="goal-cat-badge" :class="goal.category.toLowerCase()">{{ goal.category }}</span>
                <span class="goal-title">{{ goal.title }}</span>
              </li>
            </ul>
          </template>
          <p v-else class="empty-note">今期のデータのみ表示されます</p>
        </div>
      </section>

      <!-- Dev plan (key person only) -->
      <section class="section" v-if="keyPersonStore.myStatus?.isActive">
        <div class="card">
          <h2 class="section-title">育成方針</h2>
          <p class="section-desc">直属上長が登録した育成方針です</p>
          <div v-if="devPlanStore.loading" class="empty-note">読み込み中...</div>
          <template v-else-if="devPlanStore.plans.length > 0">
            <div class="dev-plan">
              <div class="dev-plan-meta">
                <span class="version-pill">v{{ devPlanStore.plans[0].version }}</span>
                <span v-if="devPlanStore.plans[0].dueDate" class="due-pill">
                  期限 {{ devPlanStore.plans[0].dueDate.slice(0, 10) }}
                </span>
              </div>
              <div class="plan-item">
                <span class="plan-label">課題</span>
                <p class="plan-text">{{ devPlanStore.plans[0].challenge }}</p>
              </div>
              <div class="plan-item">
                <span class="plan-label">目標</span>
                <p class="plan-text">{{ devPlanStore.plans[0].goal }}</p>
              </div>
              <div class="plan-item">
                <span class="plan-label">サポート施策</span>
                <p class="plan-text">{{ devPlanStore.plans[0].supportAction }}</p>
              </div>
            </div>
          </template>
          <p v-else class="empty-note">育成方針はまだ登録されていません。</p>
        </div>
      </section>

      <!-- Comments -->
      <section class="section" v-if="currentPlan">
        <div class="card">
          <h2 class="section-title">上長からのコメント</h2>
          <p v-if="store.comments.length === 0" class="empty-note">コメントはまだありません</p>
          <ul v-else class="comment-list">
            <li v-for="c in store.comments" :key="c.id" class="comment-item">
              <div class="comment-meta">{{ c.authorName }} · {{ formatDate(c.createdAt) }}</div>
              <p class="comment-content">{{ c.content }}</p>
            </li>
          </ul>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '~/stores/auth'
import { useCareerPlanStore } from '~/stores/careerPlan'
import { useKeyPersonStore } from '~/stores/keyPerson'
import { useDevelopmentPlanStore } from '~/stores/developmentPlan'
import type { CareerPlan } from '~/types/careerPlan'

const auth = useAuthStore()
const store = useCareerPlanStore()
const keyPersonStore = useKeyPersonStore()
const devPlanStore = useDevelopmentPlanStore()

const currentYear = new Date().getFullYear()
const selectedYear = ref(currentYear)
const editing = ref(false)
const saving = ref(false)
const saveError = ref<string | null>(null)
const planSaved = ref(false)

const form = reactive({ longTermGoal: '', mediumTermGoal: '', currentFocus: '' })

const isCurrentYear = computed(() => selectedYear.value === currentYear)
const currentPlan = computed<CareerPlan | null>(() => {
  if (isCurrentYear.value) return store.integratedView?.will ?? null
  return store.selectedPlan
})

onMounted(async () => {
  if (!auth.currentUser) return
  await Promise.all([
    store.fetchIntegratedView(auth.currentUser.id, auth.currentUser.id),
    store.fetchYears(auth.currentUser.id, auth.currentUser.id),
    keyPersonStore.fetchMyStatus(auth.currentUser.id),
  ])
  if (store.integratedView?.will) {
    planSaved.value = true
    await store.fetchComments(store.integratedView.will.id, auth.currentUser.id)
  }
  if (keyPersonStore.myStatus?.isActive && keyPersonStore.myStatus.id) {
    await devPlanStore.fetchList(auth.currentUser.id, keyPersonStore.myStatus.id)
  }
})

async function onYearChange() {
  if (!auth.currentUser || isCurrentYear.value) return
  await store.fetchPlanByYear(auth.currentUser.id, auth.currentUser.id, selectedYear.value)
}

function startEdit() {
  if (!currentPlan.value) return
  form.longTermGoal = currentPlan.value.longTermGoal
  form.mediumTermGoal = currentPlan.value.mediumTermGoal
  form.currentFocus = currentPlan.value.currentFocus
  editing.value = true
}

async function submitCreate() {
  if (!auth.currentUser) return
  saving.value = true; saveError.value = null
  try {
    const plan = await store.createPlan(auth.currentUser.id, auth.currentUser.id, { ...form })
    planSaved.value = true
    if (!store.years.includes(currentYear)) store.years.unshift(currentYear)
    await store.fetchComments(plan.id, auth.currentUser.id)
  }
  catch (e: unknown) {
    const err = e as { data?: { error?: string } }
    saveError.value = err?.data?.error ?? 'キャリアプランの作成に失敗しました。'
  }
  finally { saving.value = false }
}

async function submitUpdate() {
  if (!auth.currentUser || !currentPlan.value) return
  saving.value = true; saveError.value = null
  try {
    await store.updatePlan(currentPlan.value.id, auth.currentUser.id, { ...form })
    editing.value = false
  }
  catch (e: unknown) {
    const err = e as { data?: { error?: string } }
    saveError.value = err?.data?.error ?? '保存に失敗しました。'
  }
  finally { saving.value = false }
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleString('ja-JP', {
    year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit',
  })
}
</script>

<style scoped>
/* Key person banner */
.kp-banner {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  background: var(--warning-bg);
  border: 1px solid var(--warning-border);
  border-radius: var(--radius);
  margin-bottom: 1.5rem;
}
.kp-badge { font-size: 0.8rem; font-weight: 700; color: var(--warning-text); white-space: nowrap; }
.kp-reason { font-size: 0.85rem; color: var(--warning-text); }

/* Year selector */
.year-row { display: flex; align-items: center; gap: 0.75rem; }
.year-select { width: auto; min-width: 130px; }

/* Section desc */
.section-desc { font-size: 0.875rem; color: var(--text-muted); margin-bottom: 1rem; }

/* Plan form */
.plan-form { display: flex; flex-direction: column; gap: 1rem; }
.form-group { display: flex; flex-direction: column; gap: 0.3rem; }
.textarea { resize: vertical; }
.form-footer { display: flex; gap: 0.75rem; justify-content: flex-end; margin-top: 0.25rem; }

/* Plan view */
.plan-view { display: flex; flex-direction: column; gap: 1rem; }
.plan-item { }
.plan-label { font-size: 0.72rem; font-weight: 600; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.06em; }
.plan-text { margin-top: 0.3rem; white-space: pre-wrap; font-size: 0.9rem; color: var(--text); }
.plan-footer { margin-top: 0.5rem; }
.readonly-note { font-size: 0.8rem; color: var(--text-muted); margin-top: 0.5rem; }

/* Skills */
.skill-list { list-style: none; padding: 0; display: flex; flex-direction: column; gap: 0.4rem; }
.skill-item { display: flex; align-items: center; gap: 0.75rem; padding: 0.5rem 0.75rem; background: var(--surface-muted); border-radius: var(--radius-sm); }
.skill-category { font-size: 0.75rem; color: var(--text-muted); }
.skill-name { font-weight: 500; flex: 1; font-size: 0.9rem; }
.level-pill { font-size: 0.72rem; font-family: var(--font-mono); color: var(--accent); font-weight: 700; }

/* Goals */
.goal-list { list-style: none; padding: 0; display: flex; flex-direction: column; gap: 0.4rem; }
.goal-item { display: flex; align-items: center; gap: 0.75rem; padding: 0.5rem 0.75rem; background: var(--surface-muted); border-radius: var(--radius-sm); }
.goal-cat-badge { font-size: 0.7rem; font-weight: 600; padding: 0.15rem 0.5rem; border-radius: 999px; }
.goal-cat-badge.must { background: var(--danger-bg); color: var(--danger-text); }
.goal-cat-badge.will { background: #dbeafe; color: #1d4ed8; }
.goal-cat-badge.can  { background: var(--success-bg); color: var(--success-text); }
.goal-title { flex: 1; font-size: 0.9rem; }

/* Dev plan */
.dev-plan { display: flex; flex-direction: column; gap: 0.75rem; }
.dev-plan-meta { display: flex; align-items: center; gap: 0.5rem; margin-bottom: 0.25rem; }
.version-pill { font-size: 0.7rem; font-weight: 700; background: #dbeafe; color: #1d4ed8; padding: 0.15rem 0.5rem; border-radius: 999px; font-family: var(--font-mono); }
.due-pill { font-size: 0.7rem; font-weight: 600; background: var(--warning-bg); color: var(--warning-text); padding: 0.15rem 0.5rem; border-radius: 999px; font-family: var(--font-mono); }

/* Comments */
.comment-list { list-style: none; padding: 0; display: flex; flex-direction: column; gap: 0.75rem; }
.comment-item { padding: 0.75rem; background: var(--surface-muted); border-radius: var(--radius-sm); border-left: 3px solid var(--border-strong); }
.comment-meta { font-size: 0.75rem; color: var(--text-muted); margin-bottom: 0.25rem; }
.comment-content { white-space: pre-wrap; font-size: 0.875rem; }

/* Misc */
.error-inline { color: var(--danger); font-size: 0.875rem; margin-top: 0.5rem; }
</style>
