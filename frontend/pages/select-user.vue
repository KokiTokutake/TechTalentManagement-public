<template>
  <div class="login-shell">
    <!-- Left branding panel -->
    <div class="login-brand">
      <div class="brand-content">
        <div class="brand-logo">
          <span class="brand-mark">TTM</span>
        </div>
        <h1 class="brand-title">TeckTalent<br>Management</h1>
        <p class="brand-desc">
          ITエンジニアのキャリア成長を<br>組織全体で支援するシステム
        </p>
        <div class="brand-features">
          <div class="brand-feature">Will · Can · Must フレームワーク</div>
          <div class="brand-feature">スキルマップ & アセスメント</div>
          <div class="brand-feature">人材開発委員会サポート</div>
        </div>
      </div>
    </div>

    <!-- Right login panel -->
    <div class="login-panel">
      <div class="login-card">
        <div class="login-header">
          <h2 class="login-title">ログイン</h2>
          <p class="login-sub">ユーザーと考課期間を選択してください</p>
        </div>

        <div v-if="loading" class="loading-state">
          <span class="loading-dot" />
          <span class="loading-dot" />
          <span class="loading-dot" />
        </div>

        <p v-else-if="error" class="error-box" role="alert">{{ error }}</p>

        <template v-else>
          <section class="login-section">
            <h3 class="login-section-label">ユーザー</h3>
            <div class="user-list">
              <button
                v-for="user in users"
                :key="user.id"
                class="user-btn"
                :class="{ 'user-btn-selected': selected?.id === user.id }"
                @click="selected = user"
              >
                <span class="role-badge" :class="`role-${user.role.toLowerCase()}`">
                  {{ roleLabel(user.role) }}
                </span>
                <span class="user-name">{{ user.displayName }}</span>
                <span class="user-email">{{ user.email }}</span>
                <span v-if="selected?.id === user.id" class="user-check">✓</span>
              </button>
            </div>
          </section>

          <section class="login-section">
            <h3 class="login-section-label">考課期間</h3>
            <div class="period-list">
              <button
                v-for="period in periods"
                :key="period.id"
                class="period-btn"
                :class="{ 'period-btn-selected': selectedPeriod?.id === period.id }"
                @click="selectedPeriod = period"
              >
                <div class="period-info">
                  <span class="period-name">{{ period.name }}</span>
                  <span class="period-range">{{ period.goalStartDate }} 〜 {{ period.goalEndDate }}</span>
                </div>
                <span v-if="selectedPeriod?.id === period.id" class="period-check">✓</span>
              </button>
            </div>
          </section>

          <button
            class="btn-start"
            :disabled="!selected || !selectedPeriod"
            @click="start"
          >
            開始する
          </button>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { AppUser, EvaluationPeriod } from '~/types/user'

definePageMeta({ middleware: [], layout: false })

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
  }
  catch {
    error.value = 'サーバーに接続できません。バックエンドが起動しているか確認してください。'
  }
  finally {
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
.login-shell {
  display: flex;
  min-height: 100dvh;
  background: var(--canvas);
}

/* Left branding panel */
.login-brand {
  width: 380px;
  min-width: 380px;
  background: var(--nav-bg);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 3rem 2.5rem;
}

.brand-content {
  max-width: 280px;
}

.brand-logo {
  margin-bottom: 1.5rem;
}

.brand-mark {
  font-family: var(--font-mono);
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--accent);
  background: rgba(192, 86, 33, 0.15);
  padding: 0.35rem 0.6rem;
  border-radius: var(--radius-sm);
  letter-spacing: 0.06em;
}

.brand-title {
  font-family: var(--font-heading);
  font-size: 1.8rem;
  font-weight: 700;
  color: var(--nav-text);
  line-height: 1.25;
  margin-bottom: 1rem;
}

.brand-desc {
  font-size: 0.9rem;
  color: var(--nav-muted);
  line-height: 1.7;
  margin-bottom: 2rem;
}

.brand-features {
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
}

.brand-feature {
  font-size: 0.8rem;
  color: var(--nav-muted);
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.brand-feature::before {
  content: '';
  display: inline-block;
  width: 4px;
  height: 4px;
  background: var(--accent);
  border-radius: 50%;
  flex-shrink: 0;
}

/* Right panel */
.login-panel {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  overflow-y: auto;
}

.login-card {
  width: min(520px, 100%);
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-xl);
  padding: 2.5rem;
  box-shadow: var(--shadow-lg);
}

.login-header {
  margin-bottom: 2rem;
}

.login-title {
  font-family: var(--font-heading);
  font-size: 1.4rem;
  font-weight: 700;
  color: var(--text);
  margin-bottom: 0.3rem;
}

.login-sub {
  font-size: 0.875rem;
  color: var(--text-muted);
}

/* Loading dots */
.loading-state {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 3rem;
}

.loading-dot {
  width: 8px;
  height: 8px;
  background: var(--accent);
  border-radius: 50%;
  opacity: 0.4;
}

.loading-dot:nth-child(2) { opacity: 0.6; }
.loading-dot:nth-child(3) { opacity: 0.3; }

/* Login sections */
.login-section {
  margin-bottom: 1.75rem;
}

.login-section-label {
  font-size: 0.72rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: var(--text-muted);
  margin-bottom: 0.6rem;
}

/* User list */
.user-list, .period-list {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
  max-height: 240px;
  overflow-y: auto;
}

.user-btn, .period-btn {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  padding: 0.7rem 0.875rem;
  border: 1.5px solid var(--border);
  border-radius: var(--radius);
  background: var(--surface);
  cursor: pointer;
  text-align: left;
  width: 100%;
  font-family: var(--font-body);
  transition: border-color 0.1s, background 0.1s;
}

.user-btn:hover, .period-btn:hover {
  border-color: var(--accent);
  background: var(--accent-light);
}

.user-btn-selected, .period-btn-selected {
  border-color: var(--accent) !important;
  background: var(--accent-light) !important;
}

.user-name {
  font-weight: 600;
  font-size: 0.9rem;
  color: var(--text);
  flex: 1;
}

.user-email {
  font-size: 0.78rem;
  color: var(--text-muted);
  font-family: var(--font-mono);
}

.user-check, .period-check {
  font-size: 0.8rem;
  color: var(--accent);
  font-weight: 700;
}

.period-info {
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
  flex: 1;
}

.period-name {
  font-weight: 600;
  font-size: 0.9rem;
  color: var(--text);
}

.period-range {
  font-size: 0.75rem;
  color: var(--text-muted);
  font-family: var(--font-mono);
}

/* Start button */
.btn-start {
  width: 100%;
  background: var(--accent);
  color: #fff;
  border: none;
  border-radius: var(--radius);
  padding: 0.875rem;
  font-size: 1rem;
  font-weight: 700;
  cursor: pointer;
  font-family: var(--font-body);
  margin-top: 0.5rem;
  transition: background 0.12s;
}

.btn-start:hover:not(:disabled) { background: var(--accent-hover); }
.btn-start:disabled { opacity: 0.4; cursor: not-allowed; }

/* Small screen: stack layout */
@media (max-width: 700px) {
  .login-shell { flex-direction: column; }
  .login-brand { width: 100%; min-width: 0; padding: 2rem 1.5rem; }
}
</style>
