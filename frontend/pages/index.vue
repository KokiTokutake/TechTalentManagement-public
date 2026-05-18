<template>
  <main class="container">
    <header class="page-header">
      <div class="header-top">
        <h1>TeckTalentManagement</h1>
        <button class="btn-logout" @click="handleLogout">ユーザー切替</button>
      </div>
      <div class="user-info">
        <span class="role-badge" :class="`role-${auth.currentUser!.role.toLowerCase()}`">
          {{ roleLabel(auth.currentUser!.role) }}
        </span>
        <span class="user-name">{{ auth.currentUser!.displayName }}</span>
        <span class="period-name">{{ auth.currentPeriod?.name ?? '期間未選択' }}</span>
      </div>
    </header>

    <div class="menu-grid">
      <NuxtLink
        v-if="auth.currentPeriod"
        :to="`/periods/${auth.currentPeriod.id}/goals`"
        class="menu-card"
      >
        <span class="menu-icon">🎯</span>
        <span class="menu-title">目標設定</span>
        <span class="menu-desc">Will / Can / Must の目標を登録・管理する</span>
      </NuxtLink>

      <NuxtLink
        v-if="auth.currentPeriod"
        :to="`/periods/${auth.currentPeriod.id}/self-evaluation`"
        class="menu-card"
      >
        <span class="menu-icon">📝</span>
        <span class="menu-title">自己評価</span>
        <span class="menu-desc">目標ごとの達成度を入力・提出する</span>
      </NuxtLink>

      <NuxtLink to="/career" class="menu-card">
        <span class="menu-icon">🗺</span>
        <span class="menu-title">キャリアプラン</span>
        <span class="menu-desc">Will・Can・Must を統合したキャリアプランを確認・編集する</span>
      </NuxtLink>

      <NuxtLink to="/skills/my" class="menu-card">
        <span class="menu-icon">🛠</span>
        <span class="menu-title">スキル管理</span>
        <span class="menu-desc">自分のスキルを登録・編集する</span>
      </NuxtLink>

      <NuxtLink
        v-if="auth.currentUser?.role === 'Manager' || auth.currentUser?.role === 'Senior' || auth.currentUser?.role === 'Executive' || auth.currentUser?.role === 'Admin'"
        to="/skills/approval"
        class="menu-card"
      >
        <span class="menu-icon">✅</span>
        <span class="menu-title">スキル承認</span>
        <span class="menu-desc">部下のスキル申請を承認・差し戻しする</span>
      </NuxtLink>

      <NuxtLink
        v-if="auth.currentUser?.role === 'Senior' || auth.currentUser?.role === 'Executive' || auth.currentUser?.role === 'Admin'"
        to="/skills/map"
        class="menu-card"
      >
        <span class="menu-icon">🗺</span>
        <span class="menu-title">組織スキルマップ</span>
        <span class="menu-desc">全エンジニアのスキル分布を確認する</span>
      </NuxtLink>

      <NuxtLink
        v-if="auth.currentUser?.role === 'Senior' || auth.currentUser?.role === 'Executive' || auth.currentUser?.role === 'Admin'"
        to="/career/org-overview"
        class="menu-card"
      >
        <span class="menu-icon">👥</span>
        <span class="menu-title">組織キャリア俯瞰</span>
        <span class="menu-desc">全タレントのキャリアプランサマリーを一覧で確認する</span>
      </NuxtLink>

      <NuxtLink
        v-if="auth.currentUser?.role === 'Admin'"
        to="/admin/skill-master"
        class="menu-card"
      >
        <span class="menu-icon">⚙</span>
        <span class="menu-title">スキルマスター管理</span>
        <span class="menu-desc">カテゴリとスキル種別を管理する</span>
      </NuxtLink>
    </div>

    <p class="api-status">
      API: <strong>{{ apiStatus }}</strong>
    </p>
  </main>
</template>

<script setup lang="ts">
const auth = useAuthStore()
const config = useRuntimeConfig()

const { data, error } = await useFetch<{ status: string }>(
  `${config.public.apiBaseUrl}/health`,
  { server: false }
)

const apiStatus = computed(() =>
  error.value ? '接続エラー' : (data.value?.status ?? '確認中…')
)

function roleLabel(role: string) {
  const map: Record<string, string> = {
    Talent: 'タレント', Manager: '上長', Senior: '管理職', Executive: '幹部長', Admin: '管理者',
  }
  return map[role] ?? role
}

async function handleLogout() {
  auth.logout()
  await navigateTo('/select-user')
}
</script>

<style scoped>
.container {
  max-width: 680px;
  margin: 0 auto;
  padding: 2rem 1rem;
  font-family: sans-serif;
}

.page-header {
  margin-bottom: 2rem;
}

.header-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.5rem;
}

h1 {
  font-size: 1.4rem;
  font-weight: 800;
  margin: 0;
  color: #111827;
}

.btn-logout {
  background: none;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  padding: 0.35rem 0.75rem;
  font-size: 0.8rem;
  color: #6b7280;
  cursor: pointer;
}

.btn-logout:hover { background: #f3f4f6; }

.user-info {
  display: flex;
  align-items: center;
  gap: 0.6rem;
}

.role-badge {
  font-size: 0.72rem;
  font-weight: 700;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
}

.role-talent   { background: #dbeafe; color: #1d4ed8; }
.role-manager  { background: #d1fae5; color: #065f46; }
.role-senior   { background: #fef3c7; color: #92400e; }
.role-executive { background: #fee2e2; color: #991b1b; }
.role-admin    { background: #f3f4f6; color: #374151; }

.user-name {
  font-weight: 600;
  color: #111827;
}

.period-name {
  font-size: 0.85rem;
  color: #9ca3af;
}

.menu-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-bottom: 2rem;
}

.menu-card {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
  padding: 1.5rem;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  background: #fff;
  text-decoration: none;
  transition: all 0.12s;
}

.menu-card:hover {
  border-color: #2563eb;
  box-shadow: 0 2px 12px rgba(37, 99, 235, 0.1);
  transform: translateY(-1px);
}

.menu-icon { font-size: 1.75rem; }
.menu-title { font-weight: 700; font-size: 1rem; color: #111827; }
.menu-desc  { font-size: 0.8rem; color: #6b7280; }

.api-status {
  font-size: 0.8rem;
  color: #9ca3af;
  text-align: center;
}
</style>
