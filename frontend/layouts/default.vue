<template>
  <div class="app-shell">
    <!-- Sidebar -->
    <aside class="sidebar">
      <!-- Logo -->
      <div class="sidebar-logo">
        <span class="logo-mark">TTM</span>
        <div class="logo-text">
          <span class="logo-name">TeckTalent</span>
          <span class="logo-sub">Management</span>
        </div>
      </div>

      <!-- User -->
      <div v-if="auth.currentUser" class="sidebar-user">
        <div class="sidebar-user-row">
          <span class="role-badge" :class="`role-${auth.currentUser.role.toLowerCase()}`">
            {{ roleLabel(auth.currentUser.role) }}
          </span>
        </div>
        <div class="sidebar-user-name">{{ auth.currentUser.displayName }}</div>
        <div v-if="auth.currentPeriod" class="sidebar-period">
          <span class="sidebar-period-label">考課期間</span>
          <span class="sidebar-period-name">{{ auth.currentPeriod.name }}</span>
        </div>
        <div v-else class="sidebar-period-empty">期間未選択</div>
      </div>

      <div class="sidebar-divider" />

      <!-- Navigation -->
      <nav class="sidebar-nav">
        <!-- My page -->
        <div class="nav-section">
          <span class="nav-section-label">マイページ</span>
          <NuxtLink to="/" class="nav-item" exact-active-class="nav-item-active">
            ホーム
          </NuxtLink>
          <NuxtLink
            v-if="auth.currentPeriod"
            :to="`/periods/${auth.currentPeriod.id}/goals`"
            class="nav-item"
            active-class="nav-item-active"
          >
            目標設定
          </NuxtLink>
          <NuxtLink
            v-if="auth.currentPeriod"
            :to="`/periods/${auth.currentPeriod.id}/self-evaluation`"
            class="nav-item"
            active-class="nav-item-active"
          >
            自己評価
          </NuxtLink>
          <NuxtLink to="/career" class="nav-item" active-class="nav-item-active">
            キャリアプラン
          </NuxtLink>
          <NuxtLink to="/skills/my" class="nav-item" active-class="nav-item-active">
            スキル管理
          </NuxtLink>
        </div>

        <!-- Team (Manager+) -->
        <div v-if="isManagerPlus" class="nav-section">
          <span class="nav-section-label">チーム管理</span>
          <NuxtLink to="/one-on-one" class="nav-item" active-class="nav-item-active">
            1on1面談
          </NuxtLink>
          <NuxtLink to="/skills/approval" class="nav-item" active-class="nav-item-active">
            スキル承認
          </NuxtLink>
          <NuxtLink
            v-if="isSeniorPlus"
            to="/skills/map"
            class="nav-item"
            active-class="nav-item-active"
          >
            組織スキルマップ
          </NuxtLink>
          <NuxtLink
            v-if="isSeniorPlus"
            to="/career/org-overview"
            class="nav-item"
            active-class="nav-item-active"
          >
            組織キャリア俯瞰
          </NuxtLink>
        </div>

        <!-- MDC (Manager+) -->
        <div v-if="isManagerPlus" class="nav-section">
          <span class="nav-section-label">人材開発委員会</span>
          <NuxtLink to="/mdc/nine-box" class="nav-item" active-class="nav-item-active">
            9ボックスマップ
          </NuxtLink>
          <NuxtLink
            v-if="isSeniorPlus"
            to="/mdc/key-persons"
            class="nav-item"
            active-class="nav-item-active"
          >
            重点管理人物
          </NuxtLink>
          <NuxtLink
            v-if="isSeniorPlus"
            to="/mdc/nine-box/calibration"
            class="nav-item"
            active-class="nav-item-active"
          >
            キャリブレーション
          </NuxtLink>
          <NuxtLink to="/mdc/sessions" class="nav-item" active-class="nav-item-active">
            委員会セッション
          </NuxtLink>
          <NuxtLink
            v-if="isSeniorPlus"
            to="/mdc/summary"
            class="nav-item"
            active-class="nav-item-active"
          >
            委員会サマリー
          </NuxtLink>
        </div>

        <!-- Admin -->
        <div v-if="isAdmin" class="nav-section">
          <span class="nav-section-label">管理</span>
          <NuxtLink to="/admin/skill-master" class="nav-item" active-class="nav-item-active">
            スキルマスター管理
          </NuxtLink>
        </div>
      </nav>

      <div class="sidebar-spacer" />

      <!-- Footer action -->
      <div class="sidebar-footer">
        <button class="sidebar-logout-btn" @click="handleLogout">ユーザー切替</button>
      </div>
    </aside>

    <!-- Main content area -->
    <div class="main-area">
      <header class="topbar">
        <div class="topbar-spacer" />
        <NotificationBell />
      </header>
      <main class="main-content">
        <NuxtPage />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
const auth = useAuthStore()

const role = computed(() => auth.currentUser?.role ?? '')
const isManagerPlus = computed(() => ['Manager', 'Senior', 'Executive', 'Admin'].includes(role.value))
const isSeniorPlus = computed(() => ['Senior', 'Executive', 'Admin'].includes(role.value))
const isAdmin = computed(() => role.value === 'Admin')

function roleLabel(r: string) {
  const map: Record<string, string> = {
    Talent: 'タレント', Manager: '上長', Senior: '管理職', Executive: '幹部長', Admin: '管理者',
  }
  return map[r] ?? r
}

async function handleLogout() {
  auth.logout()
  await navigateTo('/select-user')
}
</script>

<style scoped>
.app-shell {
  display: flex;
  height: 100dvh;
  overflow: hidden;
}

/* ── Sidebar ── */
.sidebar {
  width: 224px;
  min-width: 224px;
  background: var(--nav-bg);
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow-y: auto;
  border-right: 1px solid var(--nav-border);
}

.sidebar-logo {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  padding: 1.25rem 1rem 1rem;
  border-bottom: 1px solid var(--nav-border);
}

.logo-mark {
  font-family: var(--font-mono);
  font-size: 0.8rem;
  font-weight: 700;
  color: var(--accent);
  background: rgba(192, 86, 33, 0.15);
  padding: 0.3rem 0.45rem;
  border-radius: var(--radius-sm);
  letter-spacing: 0.05em;
}

.logo-text {
  display: flex;
  flex-direction: column;
  line-height: 1.1;
}

.logo-name {
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--nav-text);
  font-family: var(--font-heading);
}

.logo-sub {
  font-size: 0.65rem;
  color: var(--nav-muted);
  letter-spacing: 0.04em;
}

/* User section */
.sidebar-user {
  padding: 1rem;
}

.sidebar-user-row {
  margin-bottom: 0.4rem;
}

.sidebar-user-name {
  font-size: 0.9rem;
  font-weight: 600;
  color: var(--nav-text);
  margin-bottom: 0.5rem;
}

.sidebar-period {
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
  margin-top: 0.5rem;
}

.sidebar-period-label {
  font-size: 0.65rem;
  color: var(--nav-muted);
  text-transform: uppercase;
  letter-spacing: 0.06em;
}

.sidebar-period-name {
  font-size: 0.8rem;
  color: var(--nav-text);
  font-family: var(--font-mono);
}

.sidebar-period-empty {
  font-size: 0.78rem;
  color: var(--nav-muted);
  margin-top: 0.4rem;
  font-style: italic;
}

.sidebar-divider {
  height: 1px;
  background: var(--nav-border);
  margin: 0;
}

/* Navigation */
.sidebar-nav {
  flex: 1;
  padding: 0.75rem 0;
}

.nav-section {
  margin-bottom: 0.25rem;
  padding: 0 0.75rem;
}

.nav-section-label {
  display: block;
  font-size: 0.6rem;
  font-weight: 700;
  color: var(--nav-muted);
  text-transform: uppercase;
  letter-spacing: 0.1em;
  padding: 0.75rem 0.5rem 0.35rem;
}

.nav-item {
  display: block;
  padding: 0.45rem 0.65rem;
  font-size: 0.855rem;
  color: var(--nav-muted);
  text-decoration: none;
  border-radius: var(--radius-sm);
  margin-bottom: 1px;
  transition: color 0.12s, background 0.12s;
  font-family: var(--font-body);
}

.nav-item:hover {
  color: var(--nav-text);
  background: rgba(255, 255, 255, 0.05);
}

.nav-item-active, :deep(.nav-item-active) {
  color: #f07b3a;
  background: rgba(192, 86, 33, 0.14);
  font-weight: 600;
}

.sidebar-spacer { flex: 1; }

/* Footer */
.sidebar-footer {
  padding: 0.75rem;
  border-top: 1px solid var(--nav-border);
}

.sidebar-logout-btn {
  width: 100%;
  background: none;
  border: 1px solid var(--nav-border);
  color: var(--nav-muted);
  border-radius: var(--radius-sm);
  padding: 0.45rem;
  font-size: 0.8rem;
  cursor: pointer;
  font-family: var(--font-body);
  transition: color 0.12s, border-color 0.12s;
}

.sidebar-logout-btn:hover {
  color: var(--nav-text);
  border-color: var(--nav-muted);
}

/* ── Main area ── */
.main-area {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.topbar {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  padding: 0.5rem 1.25rem;
  height: 3rem;
  min-height: 3rem;
  background: var(--surface, #fff);
  border-bottom: 1px solid var(--border, #e5e7eb);
  flex-shrink: 0;
}

.topbar-spacer { flex: 1; }

.main-content {
  flex: 1;
  min-width: 0;
  overflow-y: auto;
  background: var(--canvas);
}
</style>
