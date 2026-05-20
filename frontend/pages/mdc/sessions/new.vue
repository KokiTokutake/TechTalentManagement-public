<template>
  <div class="page-wrap">
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
  </div>
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
.page-header { margin-bottom: 2rem; }

h1 {
  font-size: 1.5rem;
  font-weight: 800;
  margin: 0;
  color: var(--text);
  font-family: var(--font-heading);
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
  color: var(--text-secondary);
}

.required { color: var(--danger); }

.form-input,
.form-textarea {
  border: 1px solid var(--border-strong);
  border-radius: 8px;
  padding: 0.6rem 0.75rem;
  font-size: 0.875rem;
  color: var(--text);
  background: var(--surface);
  transition: border-color 0.12s;
}
.form-input:focus,
.form-textarea:focus {
  outline: none;
  border-color: var(--accent);
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}
.form-textarea { resize: vertical; }

.checkbox-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  max-height: 220px;
  overflow-y: auto;
  border: 1px solid var(--border);
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
.role-admin    { background: var(--surface-muted); color: var(--text-secondary); }

.checkbox-name { color: var(--text); }

.hint-text {
  font-size: 0.8rem;
  color: var(--text-muted);
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding-top: 0.5rem;
}
</style>
