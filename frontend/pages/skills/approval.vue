<template>
  <main class="container">
    <header class="page-header">
      <NuxtLink to="/" class="back-link">← ホームへ戻る</NuxtLink>
      <h1>スキル承認</h1>
      <p class="subtitle">部下からの申請中スキルを確認・審査します</p>
    </header>

    <div v-if="store.error" class="error-box" role="alert">{{ store.error }}</div>

    <div v-if="store.loading" class="loading">読み込み中…</div>

    <div v-else-if="!store.subordinateSkills.length" class="empty-state">
      <p>現在、承認待ちのスキル申請はありません。</p>
    </div>

    <div v-for="group in store.subordinateSkills" :key="group.user.id" class="user-group">
      <h2 class="user-name">{{ group.user.displayName }}</h2>

      <div
        v-for="skill in group.skills"
        :key="skill.id"
        class="skill-card"
      >
        <div class="skill-header">
          <div class="skill-meta">
            <span class="category-tag">{{ skill.categoryName }}</span>
            <span class="skill-name">{{ skill.skillItemName }}</span>
            <span class="level-badge">Lv.{{ skill.level }} - {{ levelLabel(skill.level) }}</span>
          </div>
          <div class="skill-actions">
            <button class="btn-approve" @click="handleApprove(skill.id)">承認</button>
            <button class="btn-reject" @click="openRejectDialog(skill)">差し戻し</button>
          </div>
        </div>
        <p v-if="skill.comment" class="skill-comment">{{ skill.comment }}</p>
        <p class="skill-date">申請日: {{ formatDate(skill.updatedAt) }}</p>
      </div>
    </div>

    <!-- 差し戻しダイアログ -->
    <div v-if="rejectTarget" class="overlay" @click.self="rejectTarget = null">
      <div class="dialog">
        <h3>差し戻し</h3>
        <p class="dialog-desc">「{{ rejectTarget.skillItemName }}」を差し戻します。</p>
        <label class="label">差し戻し理由（必須）</label>
        <textarea
          v-model="rejectReason"
          class="textarea"
          rows="3"
          placeholder="タレントに伝える理由を入力してください"
        ></textarea>
        <div class="dialog-actions">
          <button class="btn-reject-confirm" :disabled="!rejectReason.trim()" @click="execReject">
            差し戻す
          </button>
          <button class="btn-ghost" @click="rejectTarget = null">キャンセル</button>
        </div>
      </div>
    </div>
  </main>
</template>

<script setup lang="ts">
import type { UserSkill } from '~/types/skill'

const auth = useAuthStore()
const store = useSkillApprovalStore()
const config = useRuntimeConfig()

const managerId = computed(() => auth.currentUser?.id ?? '')

const rejectTarget = ref<UserSkill | null>(null)
const rejectReason = ref('')

onMounted(async () => {
  // 部下のIDを取得してから申請中スキルをフェッチ
  const users = await $fetch<{ id: string, managerId: string | null }[]>(
    `${config.public.apiBaseUrl}/api/users`,
  )
  const subordinateIds = users
    .filter(u => u.managerId === managerId.value)
    .map(u => u.id)

  if (subordinateIds.length) {
    await store.fetchPendingSkills(managerId.value, subordinateIds)
  }
})

async function handleApprove(userSkillId: string) {
  store.error = null
  try {
    await store.approveSkill(managerId.value, userSkillId)
  }
  catch (e: unknown) {
    store.error = extractError(e) ?? '承認に失敗しました。'
  }
}

function openRejectDialog(skill: UserSkill) {
  rejectTarget.value = skill
  rejectReason.value = ''
}

async function execReject() {
  if (!rejectTarget.value || !rejectReason.value.trim()) return
  store.error = null
  try {
    await store.rejectSkill(managerId.value, rejectTarget.value.id, rejectReason.value.trim())
    rejectTarget.value = null
  }
  catch (e: unknown) {
    store.error = extractError(e) ?? '差し戻しに失敗しました。'
    rejectTarget.value = null
  }
}

function levelLabel(n: number) {
  return ['知識あり', '学習中', '実務可', '指導可', 'エキスパート'][n - 1] ?? ''
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('ja-JP')
}

function extractError(e: unknown): string | null {
  if (e && typeof e === 'object' && 'data' in e) {
    const data = (e as { data: unknown }).data
    if (data && typeof data === 'object' && 'error' in data) {
      return String((data as { error: unknown }).error)
    }
  }
  return null
}
</script>

<style scoped>
.container {
  max-width: 720px;
  margin: 0 auto;
  padding: 2rem 1rem;
  font-family: sans-serif;
}

.page-header { margin-bottom: 2rem; }

.back-link {
  font-size: 0.85rem;
  color: #6b7280;
  text-decoration: none;
  display: inline-block;
  margin-bottom: 0.5rem;
}
.back-link:hover { color: #2563eb; }

h1 { font-size: 1.4rem; font-weight: 800; margin: 0 0 0.25rem; color: #111827; }
.subtitle { font-size: 0.9rem; color: #9ca3af; margin: 0; }

.error-box {
  background: #fef2f2;
  border: 1px solid #fca5a5;
  color: #991b1b;
  padding: 0.75rem 1rem;
  border-radius: 8px;
  margin-bottom: 1rem;
  font-size: 0.9rem;
}

.loading, .empty-state { color: #9ca3af; font-size: 0.9rem; padding: 2rem 0; text-align: center; }

.user-group { margin-bottom: 2rem; }

.user-name {
  font-size: 1rem;
  font-weight: 700;
  color: #374151;
  padding: 0.5rem 0;
  border-bottom: 2px solid #e5e7eb;
  margin: 0 0 0.75rem;
}

.skill-card {
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 0.6rem;
  background: #fff;
}

.skill-header {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  justify-content: space-between;
  flex-wrap: wrap;
}

.skill-meta {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
  flex: 1;
}

.category-tag {
  font-size: 0.75rem;
  background: #f3f4f6;
  color: #6b7280;
  padding: 0.15rem 0.5rem;
  border-radius: 4px;
}

.skill-name { font-weight: 700; color: #111827; }

.level-badge {
  font-size: 0.78rem;
  font-weight: 700;
  background: #dbeafe;
  color: #1d4ed8;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
}

.skill-actions { display: flex; gap: 0.4rem; }

.skill-comment { font-size: 0.85rem; color: #6b7280; margin: 0.4rem 0 0; }
.skill-date { font-size: 0.78rem; color: #9ca3af; margin: 0.3rem 0 0; }

.btn-approve {
  background: #059669;
  color: #fff;
  border: none;
  border-radius: 6px;
  padding: 0.35rem 0.85rem;
  font-size: 0.85rem;
  cursor: pointer;
  white-space: nowrap;
}
.btn-approve:hover { background: #047857; }

.btn-reject {
  background: none;
  border: 1px solid #f87171;
  color: #ef4444;
  border-radius: 6px;
  padding: 0.35rem 0.85rem;
  font-size: 0.85rem;
  cursor: pointer;
  white-space: nowrap;
}
.btn-reject:hover { background: #fef2f2; }

.overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}

.dialog {
  background: #fff;
  border-radius: 12px;
  padding: 1.5rem;
  max-width: 400px;
  width: 100%;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.15);
}

.dialog h3 { font-size: 1rem; font-weight: 700; margin: 0 0 0.4rem; color: #111827; }
.dialog-desc { font-size: 0.9rem; color: #6b7280; margin: 0 0 1rem; }

.label { font-size: 0.8rem; font-weight: 600; color: #374151; display: block; margin-bottom: 0.3rem; }

.textarea {
  width: 100%;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  padding: 0.5rem 0.75rem;
  font-size: 0.9rem;
  resize: vertical;
  box-sizing: border-box;
  margin-bottom: 1rem;
}
.textarea:focus { outline: 2px solid #2563eb; }

.dialog-actions { display: flex; gap: 0.75rem; justify-content: flex-end; }

.btn-reject-confirm {
  background: #ef4444;
  color: #fff;
  border: none;
  border-radius: 6px;
  padding: 0.45rem 1rem;
  font-size: 0.9rem;
  cursor: pointer;
}
.btn-reject-confirm:hover:not(:disabled) { background: #dc2626; }
.btn-reject-confirm:disabled { opacity: 0.5; cursor: not-allowed; }

.btn-ghost {
  background: none;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  padding: 0.45rem 1rem;
  font-size: 0.9rem;
  color: #374151;
  cursor: pointer;
}
.btn-ghost:hover { background: #f3f4f6; }
</style>
