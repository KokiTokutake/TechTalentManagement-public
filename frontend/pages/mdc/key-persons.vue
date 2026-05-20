<template>
  <div class="page-wrap-wide">
    <header class="page-header">
      <h1>重点管理人物</h1>
      <p class="page-subtitle">重点管理対象のタレントを指定・管理する</p>
    </header>

    <div v-if="store.loading" class="loading">読み込み中…</div>
    <div v-else-if="store.error" class="error-box">{{ store.error }}</div>

    <template v-else>
      <!-- List card -->
      <div class="card">
        <h2 class="section-title">重点管理人物一覧</h2>

        <div v-if="store.keyPersons.length === 0" class="empty-note">
          重点管理人物は登録されていません。
        </div>

        <table v-else class="data-table">
          <thead>
            <tr>
              <th>氏名</th>
              <th>指定理由</th>
              <th>指定日</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="kp in store.keyPersons" :key="kp.id">
              <td class="name-cell">
                <NuxtLink :to="`/mdc/development-plans/${kp.id}`" class="talent-link">
                  {{ talentName(kp.talentId) }}
                </NuxtLink>
              </td>
              <td>
                <span v-if="kp.reason" class="reason-text">{{ kp.reason }}</span>
                <span v-else class="text-muted-sm">—</span>
              </td>
              <td class="date-cell">{{ formatDate(kp.designatedAt) }}</td>
              <td class="action-cell">
                <NuxtLink :to="`/mdc/development-plans/${kp.id}`" class="btn-plans">育成方針</NuxtLink>
                <button class="btn-revoke" :disabled="revoking === kp.id" @click="confirmRevoke(kp)">
                  解除
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Designate form card -->
      <div class="card">
        <h2 class="section-title">重点管理人物を指定</h2>

        <div v-if="designateError" class="error-box">{{ designateError }}</div>

        <form class="designate-form" @submit.prevent="submitDesignate">
          <div class="form-group">
            <label class="label" for="talent-select">対象タレント <span class="required">*</span></label>
            <select id="talent-select" v-model="form.talentId" class="select" required>
              <option value="" disabled>選択してください</option>
              <option v-for="user in talentUsers" :key="user.id" :value="user.id"
                :disabled="isAlreadyDesignated(user.id)">
                {{ user.displayName }}{{ isAlreadyDesignated(user.id) ? '（登録済み）' : '' }}
              </option>
            </select>
          </div>
          <div class="form-group">
            <label class="label" for="reason-input">指定理由（任意）</label>
            <textarea id="reason-input" v-model="form.reason" class="textarea" rows="3"
              placeholder="指定の背景や理由を入力してください" />
          </div>
          <button type="submit" class="btn-primary" :disabled="submitting || !form.talentId">
            {{ submitting ? '登録中…' : '指定する' }}
          </button>
        </form>
      </div>
    </template>

    <!-- Revoke confirm modal -->
    <div v-if="revokeTarget" class="modal-overlay" @click.self="revokeTarget = null">
      <div class="modal">
        <h3 class="modal-title">指定解除の確認</h3>
        <p class="modal-body">
          <strong>{{ talentName(revokeTarget.talentId) }}</strong> の重点管理人物指定を解除しますか？
        </p>
        <div class="modal-actions">
          <button class="btn-ghost" @click="revokeTarget = null">キャンセル</button>
          <button class="btn-danger" :disabled="revoking === revokeTarget.id" @click="executeRevoke">
            {{ revoking === revokeTarget.id ? '解除中…' : '解除する' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { KeyPerson } from '~/types/keyPerson'

const auth = useAuthStore()
const store = useKeyPersonStore()

const allowedRoles = ['Senior', 'Executive', 'Admin']
if (!auth.currentUser || !allowedRoles.includes(auth.currentUser.role)) await navigateTo('/')

const form = ref({ talentId: '', reason: '' })
const submitting = ref(false)
const designateError = ref<string | null>(null)
const revokeTarget = ref<KeyPerson | null>(null)
const revoking = ref<string | null>(null)

const talentUsers = computed(() => store.allUsers.filter(u => u.role === 'Talent' || u.role === 'Manager'))
function isAlreadyDesignated(talentId: string) { return store.keyPersons.some(kp => kp.talentId === talentId) }
function talentName(talentId: string) { return store.allUsers.find(u => u.id === talentId)?.displayName ?? talentId }
function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('ja-JP', { year: 'numeric', month: '2-digit', day: '2-digit' })
}

async function submitDesignate() {
  if (!auth.currentUser || !form.value.talentId) return
  submitting.value = true; designateError.value = null
  try {
    await store.designate(auth.currentUser.id, { talentId: form.value.talentId, reason: form.value.reason || null })
    form.value = { talentId: '', reason: '' }
  }
  catch (e: unknown) {
    const err = e as { data?: { error?: string }, status?: number }
    designateError.value = err?.status === 409
      ? 'このタレントはすでに重点管理人物に指定されています。'
      : '指定に失敗しました。もう一度お試しください。'
  }
  finally { submitting.value = false }
}

function confirmRevoke(kp: KeyPerson) { revokeTarget.value = kp }

async function executeRevoke() {
  if (!auth.currentUser || !revokeTarget.value) return
  revoking.value = revokeTarget.value.id
  try {
    await store.revoke(auth.currentUser.id, revokeTarget.value.id)
    revokeTarget.value = null
  }
  finally { revoking.value = null }
}

onMounted(async () => {
  if (auth.currentUser) {
    await Promise.all([store.fetchList(auth.currentUser.id), store.fetchAllUsers()])
  }
})
</script>

<style scoped>
.name-cell { font-weight: 600; white-space: nowrap; }
.date-cell { white-space: nowrap; color: var(--text-muted); font-size: 0.82rem; font-family: var(--font-mono); }
.action-cell { text-align: right; display: flex; align-items: center; justify-content: flex-end; gap: 0.5rem; }

.talent-link { font-weight: 600; color: var(--accent); text-decoration: none; }
.talent-link:hover { text-decoration: underline; }

.reason-text { display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; font-size: 0.875rem; }
.text-muted-sm { color: var(--text-muted); font-size: 0.82rem; }

.btn-plans {
  border: 1px solid var(--border-strong);
  color: var(--text-secondary);
  background: var(--surface-muted);
  border-radius: var(--radius-sm);
  padding: 0.3rem 0.75rem;
  font-size: 0.8rem;
  text-decoration: none;
  cursor: pointer;
  white-space: nowrap;
  font-family: var(--font-body);
}
.btn-plans:hover { background: var(--border); color: var(--text); }

.btn-revoke {
  background: none;
  border: 1px solid var(--danger-border);
  color: var(--danger);
  border-radius: var(--radius-sm);
  padding: 0.3rem 0.75rem;
  font-size: 0.8rem;
  cursor: pointer;
  font-family: var(--font-body);
}
.btn-revoke:hover:not(:disabled) { background: var(--danger-bg); }
.btn-revoke:disabled { opacity: 0.4; cursor: not-allowed; }

.designate-form { display: flex; flex-direction: column; gap: 1.25rem; }
.form-group { display: flex; flex-direction: column; gap: 0.4rem; }
.textarea { resize: vertical; }

.modal-title { font-family: var(--font-heading); font-size: 1.1rem; font-weight: 700; color: var(--text); margin-bottom: 0.75rem; }
.modal-body { font-size: 0.9rem; color: var(--text-secondary); margin-bottom: 1.5rem; }
.modal-actions { display: flex; justify-content: flex-end; gap: 0.75rem; }
</style>
