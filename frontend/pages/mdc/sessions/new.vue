<template>
  <main class="container">
    <header class="page-header">
      <NuxtLink to="/mdc/sessions" class="back-link">← 委員会セッション一覧</NuxtLink>
      <h1>委員会セッションを作成</h1>
    </header>

    <div v-if="submitError" class="error-box">{{ submitError }}</div>

    <form class="session-form card" @submit.prevent="submit">
      <div class="form-group">
        <label class="form-label" for="held-on">開催日 <span class="required">*</span></label>
        <input
          id="held-on"
          v-model="form.heldOn"
          type="date"
          class="form-input"
          required
        />
      </div>

      <div class="form-group">
        <label class="form-label" for="minutes">議事録 <span class="required">*</span></label>
        <textarea
          id="minutes"
          v-model="form.minutes"
          class="form-textarea"
          rows="8"
          placeholder="委員会での議論内容・決定事項を記録してください"
          required
        />
      </div>

      <div class="form-group">
        <label class="form-label">参加者（複数選択可）</label>
        <div class="checkbox-list">
          <label
            v-for="user in allUsers"
            :key="user.id"
            class="checkbox-item"
          >
            <input
              type="checkbox"
              :value="user.id"
              v-model="form.participantIds"
              class="checkbox-input"
            />
            <span class="role-badge" :class="`role-${user.role.toLowerCase()}`">{{ roleLabel(user.role) }}</span>
            <span class="checkbox-name">{{ user.displayName }}</span>
          </label>
        </div>
      </div>

      <div class="form-group">
        <label class="form-label">対象重点管理人物（複数選択可）</label>
        <div v-if="keyPersonStore.keyPersons.length === 0" class="hint-text">
          重点管理人物が登録されていません。
        </div>
        <div v-else class="checkbox-list">
          <label
            v-for="kp in keyPersonStore.keyPersons"
            :key="kp.id"
            class="checkbox-item"
          >
            <input
              type="checkbox"
              :value="kp.id"
              v-model="form.keyPersonIds"
              class="checkbox-input"
            />
            <span class="checkbox-name">{{ talentName(kp.talentId) }}</span>
          </label>
        </div>
      </div>

      <div class="form-actions">
        <NuxtLink to="/mdc/sessions" class="btn-secondary">キャンセル</NuxtLink>
        <button type="submit" class="btn-primary" :disabled="submitting || !form.heldOn || !form.minutes.trim()">
          {{ submitting ? '作成中…' : 'セッションを作成' }}
        </button>
      </div>
    </form>
  </main>
</template>

<script setup lang="ts">
const auth = useAuthStore()
const store = useCommitteeSessionStore()
const keyPersonStore = useKeyPersonStore()

const allowedRoles = ['Senior', 'Executive', 'Admin']
if (!auth.currentUser || !allowedRoles.includes(auth.currentUser.role)) {
  await navigateTo('/')
}

const allUsers = ref<import('~/types/user').AppUser[]>([])
const form = ref({
  heldOn: '',
  minutes: '',
  participantIds: [] as string[],
  keyPersonIds: [] as string[],
})
const submitting = ref(false)
const submitError = ref<string | null>(null)

function roleLabel(role: string) {
  const map: Record<string, string> = {
    Talent: 'タレント', Manager: '上長', Senior: '管理職', Executive: '幹部長', Admin: '管理者',
  }
  return map[role] ?? role
}

function talentName(talentId: string) {
  return allUsers.value.find(u => u.id === talentId)?.displayName ?? talentId
}

async function submit() {
  if (!auth.currentUser || !form.value.heldOn || !form.value.minutes.trim()) return
  submitting.value = true
  submitError.value = null
  try {
    const session = await store.createSession(auth.currentUser.id, {
      heldOn: form.value.heldOn,
      minutes: form.value.minutes,
      participantIds: form.value.participantIds,
      keyPersonIds: form.value.keyPersonIds,
    })
    await navigateTo(`/mdc/sessions/${session.id}`)
  }
  catch (e: unknown) {
    const err = e as { status?: number, data?: { error?: string } }
    if (err?.status === 403) {
      submitError.value = '委員会セッションを作成する権限がありません。'
    }
    else {
      submitError.value = '作成に失敗しました。もう一度お試しください。'
    }
  }
  finally {
    submitting.value = false
  }
}

onMounted(async () => {
  if (auth.currentUser) {
    const config = useRuntimeConfig()
    const [users] = await Promise.all([
      $fetch<import('~/types/user').AppUser[]>(`${config.public.apiBaseUrl}/api/users`),
      keyPersonStore.fetchList(auth.currentUser.id),
    ])
    allUsers.value = users
    if (keyPersonStore.allUsers.length === 0) {
      keyPersonStore.allUsers = users
    }
  }
})
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
  margin: 0;
  color: #111827;
}

.error-box {
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 8px;
  padding: 1rem 1.25rem;
  color: #dc2626;
  font-size: 0.875rem;
  margin-bottom: 1.5rem;
}

.card {
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 1.75rem;
}

.session-form {
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
  color: #374151;
}

.required { color: #dc2626; }

.form-input,
.form-textarea {
  border: 1px solid #d1d5db;
  border-radius: 8px;
  padding: 0.6rem 0.75rem;
  font-size: 0.875rem;
  color: #111827;
  background: #fff;
  transition: border-color 0.12s;
}
.form-input:focus,
.form-textarea:focus {
  outline: none;
  border-color: #2563eb;
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}
.form-textarea { resize: vertical; }

.checkbox-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  max-height: 220px;
  overflow-y: auto;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 0.75rem;
}

.checkbox-item {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  cursor: pointer;
  font-size: 0.875rem;
}

.checkbox-input { cursor: pointer; flex-shrink: 0; }

.role-badge {
  font-size: 0.7rem;
  font-weight: 700;
  padding: 0.1rem 0.45rem;
  border-radius: 999px;
  flex-shrink: 0;
}

.role-talent   { background: #dbeafe; color: #1d4ed8; }
.role-manager  { background: #d1fae5; color: #065f46; }
.role-senior   { background: #fef3c7; color: #92400e; }
.role-executive { background: #fee2e2; color: #991b1b; }
.role-admin    { background: #f3f4f6; color: #374151; }

.checkbox-name { color: #111827; }

.hint-text {
  font-size: 0.8rem;
  color: #9ca3af;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding-top: 0.5rem;
}

.btn-secondary {
  display: inline-flex;
  align-items: center;
  background: #fff;
  border: 1px solid #d1d5db;
  color: #374151;
  border-radius: 8px;
  padding: 0.6rem 1.25rem;
  font-size: 0.875rem;
  font-weight: 600;
  text-decoration: none;
  cursor: pointer;
  transition: all 0.12s;
}
.btn-secondary:hover { background: #f9fafb; }

.btn-primary {
  background: #2563eb;
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 0.6rem 1.5rem;
  font-size: 0.875rem;
  font-weight: 700;
  cursor: pointer;
  transition: background 0.15s;
}
.btn-primary:hover:not(:disabled) { background: #1d4ed8; }
.btn-primary:disabled { opacity: 0.4; cursor: not-allowed; }
</style>
