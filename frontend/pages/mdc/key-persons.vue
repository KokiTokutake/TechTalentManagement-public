<template>
  <main class="container">
    <header class="page-header">
      <NuxtLink to="/" class="back-link">← ホーム</NuxtLink>
      <h1>重点管理人物管理</h1>
      <p class="subtitle">重点管理対象のタレントを指定・管理する</p>
    </header>

    <div v-if="store.loading" class="loading">読み込み中…</div>

    <div v-else-if="store.error" class="error-box">{{ store.error }}</div>

    <template v-else>
      <section class="card">
        <h2 class="section-title">重点管理人物一覧</h2>

        <div v-if="store.keyPersons.length === 0" class="empty-state">
          重点管理人物は登録されていません。
        </div>

        <table v-else class="kp-table">
          <thead>
            <tr>
              <th>氏名</th>
              <th>指定理由</th>
              <th>指定日時</th>
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
                <span v-else class="cell-empty">—</span>
              </td>
              <td class="date-cell">{{ formatDate(kp.designatedAt) }}</td>
              <td class="action-cell">
                <NuxtLink :to="`/mdc/development-plans/${kp.id}`" class="btn-plans">育成方針</NuxtLink>
                <button
                  class="btn-revoke"
                  :disabled="revoking === kp.id"
                  @click="confirmRevoke(kp)"
                >
                  解除
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </section>

      <section class="card">
        <h2 class="section-title">重点管理人物を指定</h2>

        <div v-if="designateError" class="error-box form-error">{{ designateError }}</div>

        <form class="designate-form" @submit.prevent="submitDesignate">
          <div class="form-group">
            <label class="form-label" for="talent-select">対象タレント <span class="required">*</span></label>
            <select id="talent-select" v-model="form.talentId" class="form-select" required>
              <option value="" disabled>選択してください</option>
              <option
                v-for="user in talentUsers"
                :key="user.id"
                :value="user.id"
                :disabled="isAlreadyDesignated(user.id)"
              >
                {{ user.displayName }}{{ isAlreadyDesignated(user.id) ? '（登録済み）' : '' }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label class="form-label" for="reason-input">指定理由（任意）</label>
            <textarea
              id="reason-input"
              v-model="form.reason"
              class="form-textarea"
              rows="3"
              placeholder="指定の背景や理由を入力してください"
            />
          </div>

          <button type="submit" class="btn-primary" :disabled="submitting || !form.talentId">
            {{ submitting ? '登録中…' : '指定する' }}
          </button>
        </form>
      </section>
    </template>

    <div v-if="revokeTarget" class="modal-overlay" @click.self="revokeTarget = null">
      <div class="modal">
        <h3 class="modal-title">指定解除の確認</h3>
        <p class="modal-body">
          <strong>{{ talentName(revokeTarget.talentId) }}</strong> の重点管理人物指定を解除しますか？
        </p>
        <div class="modal-actions">
          <button class="btn-secondary" @click="revokeTarget = null">キャンセル</button>
          <button class="btn-danger" :disabled="revoking === revokeTarget.id" @click="executeRevoke">
            {{ revoking === revokeTarget.id ? '解除中…' : '解除する' }}
          </button>
        </div>
      </div>
    </div>
  </main>
</template>

<script setup lang="ts">
import type { KeyPerson } from '~/types/keyPerson'

const auth = useAuthStore()
const store = useKeyPersonStore()

const allowedRoles = ['Senior', 'Executive', 'Admin']
if (!auth.currentUser || !allowedRoles.includes(auth.currentUser.role)) {
  await navigateTo('/')
}

const form = ref({ talentId: '', reason: '' })
const submitting = ref(false)
const designateError = ref<string | null>(null)
const revokeTarget = ref<KeyPerson | null>(null)
const revoking = ref<string | null>(null)

const talentUsers = computed(() =>
  store.allUsers.filter(u => u.role === 'Talent' || u.role === 'Manager'),
)

function isAlreadyDesignated(talentId: string) {
  return store.keyPersons.some(kp => kp.talentId === talentId)
}

function talentName(talentId: string) {
  return store.allUsers.find(u => u.id === talentId)?.displayName ?? talentId
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('ja-JP', {
    year: 'numeric', month: '2-digit', day: '2-digit',
  })
}

async function submitDesignate() {
  if (!auth.currentUser || !form.value.talentId) return
  submitting.value = true
  designateError.value = null
  try {
    await store.designate(auth.currentUser.id, {
      talentId: form.value.talentId,
      reason: form.value.reason || null,
    })
    form.value = { talentId: '', reason: '' }
  }
  catch (e: unknown) {
    const err = e as { data?: { error?: string }, status?: number }
    if (err?.status === 409) {
      designateError.value = 'このタレントはすでに重点管理人物に指定されています。'
    }
    else {
      designateError.value = '指定に失敗しました。もう一度お試しください。'
    }
  }
  finally {
    submitting.value = false
  }
}

function confirmRevoke(kp: KeyPerson) {
  revokeTarget.value = kp
}

async function executeRevoke() {
  if (!auth.currentUser || !revokeTarget.value) return
  revoking.value = revokeTarget.value.id
  try {
    await store.revoke(auth.currentUser.id, revokeTarget.value.id)
    revokeTarget.value = null
  }
  catch {
    // keep modal open so user sees failure
  }
  finally {
    revoking.value = null
  }
}

onMounted(async () => {
  if (auth.currentUser) {
    await Promise.all([
      store.fetchList(auth.currentUser.id),
      store.fetchAllUsers(),
    ])
  }
})
</script>

<style scoped>
.container {
  max-width: 880px;
  margin: 0 auto;
  padding: 2rem 1rem;
  font-family: sans-serif;
}

.page-header {
  margin-bottom: 2rem;
}

.back-link {
  display: inline-block;
  margin-bottom: 0.75rem;
  font-size: 0.85rem;
  color: #6b7280;
  text-decoration: none;
}

.back-link:hover { color: #2563eb; }

h1 {
  font-size: 1.5rem;
  font-weight: 800;
  margin: 0 0 0.25rem;
  color: #111827;
}

.subtitle {
  font-size: 0.875rem;
  color: #6b7280;
  margin: 0;
}

.loading {
  text-align: center;
  color: #9ca3af;
  padding: 3rem 0;
}

.error-box {
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 8px;
  padding: 1rem 1.25rem;
  color: #dc2626;
  font-size: 0.875rem;
}

.card {
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 1.5rem;
  margin-bottom: 1.5rem;
}

.section-title {
  font-size: 1rem;
  font-weight: 700;
  color: #111827;
  margin: 0 0 1.25rem;
}

.empty-state {
  color: #9ca3af;
  font-size: 0.875rem;
  padding: 1.5rem 0;
  text-align: center;
}

.kp-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.kp-table th {
  text-align: left;
  padding: 0.65rem 1rem;
  background: #f9fafb;
  border-bottom: 2px solid #e5e7eb;
  font-weight: 600;
  color: #374151;
  white-space: nowrap;
}

.kp-table td {
  padding: 0.75rem 1rem;
  border-bottom: 1px solid #f3f4f6;
  vertical-align: middle;
  color: #111827;
}

.kp-table tr:last-child td { border-bottom: none; }

.name-cell { font-weight: 600; white-space: nowrap; }
.date-cell { white-space: nowrap; color: #6b7280; font-size: 0.8rem; }
.action-cell { text-align: right; display: flex; align-items: center; justify-content: flex-end; gap: 0.5rem; }

.talent-link {
  font-weight: 600;
  color: #2563eb;
  text-decoration: none;
}

.talent-link:hover { text-decoration: underline; }

.btn-plans {
  border: 1px solid #93c5fd;
  color: #1d4ed8;
  background: #eff6ff;
  border-radius: 6px;
  padding: 0.3rem 0.75rem;
  font-size: 0.8rem;
  text-decoration: none;
  cursor: pointer;
  transition: all 0.12s;
  white-space: nowrap;
}

.btn-plans:hover { background: #dbeafe; }

.reason-text {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.cell-empty {
  color: #9ca3af;
  font-size: 0.8rem;
}

.btn-revoke {
  background: none;
  border: 1px solid #fca5a5;
  color: #dc2626;
  border-radius: 6px;
  padding: 0.3rem 0.75rem;
  font-size: 0.8rem;
  cursor: pointer;
  transition: all 0.12s;
}

.btn-revoke:hover:not(:disabled) {
  background: #fef2f2;
}

.btn-revoke:disabled { opacity: 0.4; cursor: not-allowed; }

.designate-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.form-error { margin-bottom: 0; }

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.form-label {
  font-size: 0.875rem;
  font-weight: 600;
  color: #374151;
}

.required {
  color: #dc2626;
}

.form-select,
.form-textarea {
  border: 1px solid #d1d5db;
  border-radius: 8px;
  padding: 0.6rem 0.75rem;
  font-size: 0.875rem;
  color: #111827;
  background: #fff;
  transition: border-color 0.12s;
}

.form-select:focus,
.form-textarea:focus {
  outline: none;
  border-color: #2563eb;
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}

.form-textarea { resize: vertical; }

.btn-primary {
  align-self: flex-start;
  background: #2563eb;
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 0.65rem 1.5rem;
  font-size: 0.9rem;
  font-weight: 700;
  cursor: pointer;
  transition: background 0.15s;
}

.btn-primary:hover:not(:disabled) { background: #1d4ed8; }
.btn-primary:disabled { opacity: 0.4; cursor: not-allowed; }

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}

.modal {
  background: #fff;
  border-radius: 12px;
  padding: 2rem;
  width: min(420px, 90vw);
  box-shadow: 0 8px 40px rgba(0, 0, 0, 0.2);
}

.modal-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: #111827;
  margin: 0 0 0.75rem;
}

.modal-body {
  font-size: 0.9rem;
  color: #374151;
  margin: 0 0 1.5rem;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
}

.btn-secondary {
  background: #fff;
  border: 1px solid #d1d5db;
  color: #374151;
  border-radius: 8px;
  padding: 0.6rem 1.25rem;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.12s;
}

.btn-secondary:hover { background: #f9fafb; }

.btn-danger {
  background: #dc2626;
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 0.6rem 1.25rem;
  font-size: 0.875rem;
  font-weight: 700;
  cursor: pointer;
  transition: background 0.15s;
}

.btn-danger:hover:not(:disabled) { background: #b91c1c; }
.btn-danger:disabled { opacity: 0.4; cursor: not-allowed; }
</style>
