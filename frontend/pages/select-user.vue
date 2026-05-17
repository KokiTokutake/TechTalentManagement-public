<template>
  <main class="container">
    <div class="card">
      <h1>TeckTalentManagement</h1>
      <p class="subtitle">ユーザーと考課期間を選択してください</p>

      <div v-if="loading" class="loading">読み込み中…</div>
      <p v-else-if="error" class="error" role="alert">{{ error }}</p>

      <template v-else>
        <section class="section">
          <h2>ユーザー</h2>
          <div class="user-list">
            <button
              v-for="user in users"
              :key="user.id"
              class="user-btn"
              :class="{ selected: selected?.id === user.id }"
              @click="selected = user"
            >
              <span class="role-badge" :class="`role-${user.role.toLowerCase()}`">
                {{ roleLabel(user.role) }}
              </span>
              <span class="name">{{ user.displayName }}</span>
              <span class="email">{{ user.email }}</span>
            </button>
          </div>
        </section>

        <section class="section">
          <h2>考課期間</h2>
          <div class="period-list">
            <button
              v-for="period in periods"
              :key="period.id"
              class="period-btn"
              :class="{ selected: selectedPeriod?.id === period.id }"
              @click="selectedPeriod = period"
            >
              <span class="period-name">{{ period.name }}</span>
              <span class="period-range">目標: {{ period.goalStartDate }} 〜 {{ period.goalEndDate }}</span>
            </button>
          </div>
        </section>

        <button
          class="btn-start"
          :disabled="!selected || !selectedPeriod"
          @click="start"
        >
          この設定で開始する
        </button>
      </template>
    </div>
  </main>
</template>

<script setup lang="ts">
import type { AppUser, EvaluationPeriod } from '~/types/user'

definePageMeta({ middleware: [] }) // auth middleware を無効化

const config = useRuntimeConfig()
const base = config.public.apiBaseUrl

const auth = useAuthStore()

const users = ref<AppUser[]>([])
const periods = ref<EvaluationPeriod[]>([])
const selected = ref<AppUser | null>(null)
const selectedPeriod = ref<EvaluationPeriod | null>(null)
const loading = ref(true)
const error = ref('')

onMounted(async () => {
  try {
    const [u, p] = await Promise.all([
      $fetch<AppUser[]>(`${base}/api/users`),
      $fetch<EvaluationPeriod[]>(`${base}/api/periods`),
    ])
    users.value = u
    periods.value = p
    if (p.length > 0) selectedPeriod.value = p[0]
  } catch {
    error.value = 'サーバーに接続できません。バックエンドが起動しているか確認してください。'
  } finally {
    loading.value = false
  }
})

function roleLabel(role: string) {
  const map: Record<string, string> = {
    Talent: 'タレント', Manager: '上長', Senior: '管理職', Executive: '幹部長', Admin: '管理者',
  }
  return map[role] ?? role
}

async function start() {
  if (!selected.value || !selectedPeriod.value) return
  auth.selectUser(selected.value)
  auth.selectPeriod(selectedPeriod.value)
  await navigateTo('/')
}
</script>

<style scoped>
.container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f3f4f6;
  font-family: sans-serif;
  padding: 2rem 1rem;
}

.card {
  background: #fff;
  border-radius: 12px;
  padding: 2.5rem;
  width: min(540px, 100%);
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.1);
}

h1 {
  font-size: 1.5rem;
  font-weight: 800;
  margin: 0 0 0.25rem;
  color: #111827;
}

.subtitle {
  color: #6b7280;
  font-size: 0.9rem;
  margin: 0 0 2rem;
}

.loading { color: #6b7280; text-align: center; padding: 2rem 0; }

.error {
  color: #ef4444;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 6px;
  padding: 0.75rem 1rem;
}

.section {
  margin-bottom: 1.5rem;
}

.section h2 {
  font-size: 0.85rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #9ca3af;
  margin: 0 0 0.6rem;
}

.user-list, .period-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.user-btn, .period-btn {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  border: 2px solid #e5e7eb;
  border-radius: 8px;
  background: #fff;
  cursor: pointer;
  text-align: left;
  transition: all 0.12s;
  width: 100%;
}

.user-btn:hover, .period-btn:hover {
  border-color: #2563eb;
  background: #eff6ff;
}

.user-btn.selected, .period-btn.selected {
  border-color: #2563eb;
  background: #eff6ff;
}

.role-badge {
  font-size: 0.72rem;
  font-weight: 700;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
  flex-shrink: 0;
}

.role-talent   { background: #dbeafe; color: #1d4ed8; }
.role-manager  { background: #d1fae5; color: #065f46; }
.role-senior   { background: #fef3c7; color: #92400e; }
.role-executive { background: #fee2e2; color: #991b1b; }
.role-admin    { background: #f3f4f6; color: #374151; }

.name {
  font-weight: 600;
  font-size: 0.95rem;
  color: #111827;
  flex: 1;
}

.email {
  font-size: 0.8rem;
  color: #9ca3af;
}

.period-name {
  font-weight: 600;
  font-size: 0.95rem;
  color: #111827;
  flex: 1;
}

.period-range {
  font-size: 0.78rem;
  color: #9ca3af;
}

.btn-start {
  width: 100%;
  background: #2563eb;
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 0.875rem;
  font-size: 1rem;
  font-weight: 700;
  cursor: pointer;
  margin-top: 0.5rem;
  transition: background 0.15s;
}

.btn-start:hover:not(:disabled) { background: #1d4ed8; }
.btn-start:disabled { opacity: 0.4; cursor: not-allowed; }
</style>
