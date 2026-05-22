<template>
  <div class="page-wrap">
    <!-- Welcome header -->
    <header class="dashboard-header">
      <div class="dashboard-greeting">
        <h1 class="greeting-title">
          おはようございます、<br>{{ auth.currentUser?.displayName }} さん
        </h1>
        <p class="greeting-sub">
          <span v-if="auth.currentUser" class="role-badge" :class="`role-${auth.currentUser.role.toLowerCase()}`">
            {{ roleLabel(auth.currentUser.role) }}
          </span>
          <span class="api-pill" :class="apiOk ? 'api-ok' : 'api-err'">
            {{ apiOk ? 'API 接続中' : 'API 接続エラー' }}
          </span>
        </p>
      </div>
      <div v-if="auth.currentPeriod" class="period-badge">
        <span class="period-badge-label">考課期間</span>
        <span class="period-badge-name">{{ auth.currentPeriod.name }}</span>
        <span class="period-badge-phase">{{ phaseLabel }}</span>
      </div>
    </header>

    <!-- Quick actions based on role -->
    <section class="section">
      <h2 class="section-title">クイックアクション</h2>
      <div class="action-grid">
        <NuxtLink
          v-for="action in quickActions"
          :key="action.to"
          :to="action.to"
          class="action-card"
        >
          <span class="action-number">{{ action.num }}</span>
          <div class="action-content">
            <span class="action-title">{{ action.title }}</span>
            <span class="action-desc">{{ action.desc }}</span>
          </div>
          <span class="action-arrow">→</span>
        </NuxtLink>
      </div>
    </section>

    <!-- All features -->
    <section class="section">
      <h2 class="section-title">すべての機能</h2>
      <div class="feature-groups">
        <!-- Personal -->
        <div class="feature-group">
          <h3 class="feature-group-label">マイページ</h3>
          <div class="feature-list">
            <NuxtLink
              v-if="auth.currentPeriod"
              :to="`/periods/${auth.currentPeriod.id}/goals`"
              class="feature-item"
            >
              <span>目標設定</span>
              <span class="feature-arrow">→</span>
            </NuxtLink>
            <NuxtLink
              v-if="auth.currentPeriod"
              :to="`/periods/${auth.currentPeriod.id}/self-evaluation`"
              class="feature-item"
            >
              <span>自己評価</span>
              <span class="feature-arrow">→</span>
            </NuxtLink>
            <NuxtLink to="/career" class="feature-item">
              <span>キャリアプラン</span>
              <span class="feature-arrow">→</span>
            </NuxtLink>
            <NuxtLink to="/skills/my" class="feature-item">
              <span>スキル管理</span>
              <span class="feature-arrow">→</span>
            </NuxtLink>
          </div>
        </div>

        <!-- Team management -->
        <div
          v-if="auth.currentUser?.role === 'Manager' || isSeniorPlus"
          class="feature-group"
        >
          <h3 class="feature-group-label">チーム管理</h3>
          <div class="feature-list">
            <NuxtLink to="/skills/approval" class="feature-item">
              <span>スキル承認</span>
              <span class="feature-arrow">→</span>
            </NuxtLink>
            <NuxtLink v-if="isSeniorPlus" to="/skills/map" class="feature-item">
              <span>組織スキルマップ</span>
              <span class="feature-arrow">→</span>
            </NuxtLink>
            <NuxtLink v-if="isSeniorPlus" to="/career/org-overview" class="feature-item">
              <span>組織キャリア俯瞰</span>
              <span class="feature-arrow">→</span>
            </NuxtLink>
          </div>
        </div>

        <!-- MDC -->
        <div
          v-if="auth.currentUser?.role === 'Manager' || isSeniorPlus"
          class="feature-group"
        >
          <h3 class="feature-group-label">人材開発委員会</h3>
          <div class="feature-list">
            <NuxtLink v-if="isSeniorPlus" to="/mdc/key-persons" class="feature-item">
              <span>重点管理人物</span>
              <span class="feature-arrow">→</span>
            </NuxtLink>
            <NuxtLink to="/mdc/sessions" class="feature-item">
              <span>委員会セッション</span>
              <span class="feature-arrow">→</span>
            </NuxtLink>
            <NuxtLink v-if="isSeniorPlus" to="/mdc/summary" class="feature-item">
              <span>委員会サマリー</span>
              <span class="feature-arrow">→</span>
            </NuxtLink>
          </div>
        </div>

        <!-- Admin -->
        <div v-if="auth.currentUser?.role === 'Admin'" class="feature-group">
          <h3 class="feature-group-label">管理</h3>
          <div class="feature-list">
            <NuxtLink to="/admin/skill-master" class="feature-item">
              <span>スキルマスター管理</span>
              <span class="feature-arrow">→</span>
            </NuxtLink>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
const auth = useAuthStore()
const config = useRuntimeConfig()

const { data, error } = await useFetch<{ status: string }>(
  `${config.public.apiBaseUrl}/health`,
  { server: false },
)

const apiOk = computed(() => !error.value && !!data.value)

const role = computed(() => auth.currentUser?.role ?? '')
const isSeniorPlus = computed(() => ['Senior', 'Executive', 'Admin'].includes(role.value))

function roleLabel(r: string) {
  const map: Record<string, string> = {
    Talent: 'タレント', Manager: '上長', Senior: '管理職', Executive: '幹部長', Admin: '管理者',
  }
  return map[r] ?? r
}

const phaseLabel = computed(() => {
  const phase = auth.currentPeriod?.currentPhase ?? ''
  const map: Record<string, string> = {
    GoalRegistration: '目標登録フェーズ',
    SelfEvaluation: '自己評価フェーズ',
    ManagerEvaluation: '上長評価フェーズ',
    Confirmation: '確定フェーズ',
    Closed: '完了',
  }
  return map[phase] ?? phase
})

const quickActions = computed(() => {
  const actions: { num: string, to: string, title: string, desc: string }[] = []
  const period = auth.currentPeriod

  if (period) {
    const phase = period.currentPhase
    if (phase === 'GoalRegistration') {
      actions.push({ num: '01', to: `/periods/${period.id}/goals`, title: '目標を設定する', desc: 'Will / Can / Must の目標を登録する' })
    }
    if (phase === 'SelfEvaluation') {
      actions.push({ num: '01', to: `/periods/${period.id}/self-evaluation`, title: '自己評価を入力する', desc: '今期の達成度を入力・提出する' })
    }
    if (phase === 'ManagerEvaluation' && (role.value === 'Manager' || isSeniorPlus.value)) {
      actions.push({ num: '01', to: '/skills/approval', title: '上長評価を実施する', desc: '部下の自己評価を確認し上長評価を入力する' })
    }
  }

  actions.push({ num: actions.length === 0 ? '01' : '02', to: '/career', title: 'キャリアプランを確認する', desc: 'Will・Can・Must を統合した自分のキャリアプラン' })
  actions.push({ num: String(actions.length).padStart(2, '0'), to: '/skills/my', title: 'スキルを管理する', desc: '自分のスキルを登録・更新する' })

  if (isSeniorPlus.value) {
    actions.push({ num: String(actions.length).padStart(2, '0'), to: '/mdc/key-persons', title: '重点管理人物を確認する', desc: '育成強化対象のタレントを管理する' })
  }

  return actions.slice(0, 3)
})
</script>

<style scoped>
.dashboard-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1.5rem;
  margin-bottom: 2.5rem;
  padding-bottom: 2rem;
  border-bottom: 1px solid var(--border);
}

.greeting-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--text);
  margin-bottom: 0.6rem;
  line-height: 1.4;
}

.greeting-sub {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.api-pill {
  font-size: 0.7rem;
  font-weight: 600;
  font-family: var(--font-mono);
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
}
.api-ok  { background: var(--success-bg); color: var(--success-text); }
.api-err { background: var(--danger-bg); color: var(--danger-text); }

.period-badge {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.15rem;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 0.75rem 1rem;
  min-width: 160px;
  flex-shrink: 0;
}

.period-badge-label {
  font-size: 0.65rem;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.08em;
  font-weight: 600;
}

.period-badge-name {
  font-size: 0.95rem;
  font-weight: 700;
  color: var(--text);
  font-family: var(--font-heading);
}

.period-badge-phase {
  font-size: 0.72rem;
  color: var(--accent);
  font-weight: 600;
  font-family: var(--font-mono);
}

/* Action cards */
.action-grid {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.action-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 1.25rem;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  text-decoration: none;
  transition: border-color 0.12s, box-shadow 0.12s;
}

.action-card:hover {
  border-color: var(--accent);
  box-shadow: var(--shadow-sm);
}

.action-number {
  font-family: var(--font-mono);
  font-size: 0.7rem;
  font-weight: 700;
  color: var(--accent);
  opacity: 0.6;
  min-width: 2ch;
}

.action-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
}

.action-title {
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--text);
}

.action-desc {
  font-size: 0.8rem;
  color: var(--text-muted);
}

.action-arrow {
  font-size: 0.9rem;
  color: var(--text-muted);
}

.action-card:hover .action-arrow {
  color: var(--accent);
}

/* Feature groups */
.feature-groups {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1.25rem;
}

.feature-group {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  overflow: hidden;
}

.feature-group-label {
  font-size: 0.7rem;
  font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.08em;
  padding: 0.65rem 1rem;
  background: var(--surface-muted);
  border-bottom: 1px solid var(--border);
}

.feature-list {
  padding: 0.35rem 0;
}

.feature-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.5rem 1rem;
  font-size: 0.875rem;
  color: var(--text-secondary);
  text-decoration: none;
}

.feature-item:hover {
  background: var(--surface-muted);
  color: var(--accent);
}

.feature-arrow {
  font-size: 0.75rem;
  color: var(--text-muted);
}
</style>
