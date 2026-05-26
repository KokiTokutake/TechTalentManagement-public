<template>
  <div class="page-wrap-wide">
    <header class="page-header">
      <div class="header-row">
        <div>
          <h1>重要ポジション管理</h1>
          <p class="page-subtitle">後継者計画の対象となる重要ポジションを管理する</p>
        </div>
        <NuxtLink to="/mdc/succession" class="btn-ghost">サクセッションマップへ</NuxtLink>
      </div>
    </header>

    <div v-if="store.loading" class="loading">読み込み中…</div>
    <div v-else-if="store.error" class="error-box">{{ store.error }}</div>

    <template v-else>
      <!-- Position list -->
      <div class="card">
        <h2 class="section-title">ポジション一覧</h2>

        <div v-if="store.positions.length === 0" class="empty-note">
          重要ポジションは登録されていません。
        </div>

        <table v-else class="data-table">
          <thead>
            <tr>
              <th>ポジション名</th>
              <th>重要度</th>
              <th>現担当者</th>
              <th>候補者数</th>
              <th>Ready Now</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="pos in store.positions"
              :key="pos.id"
              :data-testid="`position-row-${pos.id}`"
            >
              <td class="name-cell">
                <NuxtLink :to="`/mdc/succession/positions/${pos.id}`" class="position-link">
                  {{ pos.positionName }}
                </NuxtLink>
              </td>
              <td>
                <span :class="`importance-badge importance-${pos.importance.toLowerCase()}`">
                  {{ importanceLabel(pos.importance) }}
                </span>
              </td>
              <td>{{ pos.currentHolderName ?? '—' }}</td>
              <td class="center-cell">{{ pos.candidateCount }}</td>
              <td class="center-cell">
                <span v-if="pos.hasReadyNowCandidate" class="badge-ok">✓</span>
                <span v-else class="badge-warn">未定</span>
              </td>
              <td class="action-cell">
                <NuxtLink :to="`/mdc/succession/positions/${pos.id}`" class="btn-detail">詳細</NuxtLink>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Add position form -->
      <div class="card">
        <h2 class="section-title">ポジションを追加</h2>
        <div v-if="formError" class="error-box">{{ formError }}</div>
        <form class="position-form" @submit.prevent="submitCreate">
          <div class="form-row">
            <div class="form-group">
              <label class="label" for="position-name">ポジション名 <span class="required">*</span></label>
              <input
                id="position-name"
                v-model="form.positionName"
                data-testid="position-name-input"
                class="input"
                type="text"
                placeholder="例: バックエンドテックリード"
                required
              >
            </div>
            <div class="form-group">
              <label class="label" for="position-importance">重要度 <span class="required">*</span></label>
              <select
                id="position-importance"
                v-model="form.importance"
                data-testid="position-importance-select"
                class="select"
                required
              >
                <option value="Critical">Critical</option>
                <option value="High">High</option>
                <option value="Medium">Medium</option>
              </select>
            </div>
            <div class="form-group">
              <label class="label" for="position-holder">現担当者（任意）</label>
              <select id="position-holder" v-model="form.currentHolderId" class="select">
                <option :value="null">なし</option>
                <option v-for="user in allUsers" :key="user.id" :value="user.id">
                  {{ user.displayName }}
                </option>
              </select>
            </div>
          </div>
          <button
            type="submit"
            class="btn-primary"
            data-testid="position-save-button"
            :disabled="submitting"
          >
            {{ submitting ? '追加中…' : 'ポジションを追加' }}
          </button>
        </form>
      </div>
    </template>

    <!-- Add button (floating shortcut) -->
    <button
      class="fab"
      data-testid="position-add-button"
      title="ポジションを追加"
      @click="scrollToForm"
    >
      ＋
    </button>
  </div>
</template>

<script setup lang="ts">
import type { AppUser } from '~/types/user'
import type { PositionImportance } from '~/types/succession'

const auth = useAuthStore()
const store = useSuccessionStore()

const allowedRoles = ['Senior', 'Executive', 'Admin']
if (!auth.currentUser || !allowedRoles.includes(auth.currentUser.role)) await navigateTo('/')

const allUsers = ref<AppUser[]>([])
const form = ref<{ positionName: string; importance: PositionImportance; currentHolderId: string | null }>({
  positionName: '',
  importance: 'Critical',
  currentHolderId: null,
})
const submitting = ref(false)
const formError = ref<string | null>(null)

function importanceLabel(imp: PositionImportance) {
  return { Critical: 'Critical', High: 'High', Medium: 'Medium' }[imp]
}

function scrollToForm() {
  document.querySelector('.position-form')?.scrollIntoView({ behavior: 'smooth' })
}

async function submitCreate() {
  if (!auth.currentUser) return
  submitting.value = true
  formError.value = null
  try {
    await store.createPosition(auth.currentUser.id, {
      positionName: form.value.positionName,
      importance: form.value.importance,
      currentHolderId: form.value.currentHolderId,
    })
    form.value = { positionName: '', importance: 'Critical', currentHolderId: null }
  }
  catch {
    formError.value = 'ポジションの追加に失敗しました。もう一度お試しください。'
  }
  finally {
    submitting.value = false
  }
}

onMounted(async () => {
  if (!auth.currentUser) return
  const config = useRuntimeConfig()
  const [, users] = await Promise.all([
    store.fetchPositions(auth.currentUser.id),
    $fetch<AppUser[]>(`${config.public.apiBaseUrl}/api/users`),
  ])
  allUsers.value = users
})
</script>

<style scoped>
.header-row { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; }

.position-link { font-weight: 600; color: var(--accent); text-decoration: none; }
.position-link:hover { text-decoration: underline; }

.importance-badge {
  display: inline-block;
  padding: 0.15rem 0.5rem;
  border-radius: var(--radius-sm);
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.02em;
}
.importance-critical { background: #fee2e2; color: #991b1b; }
.importance-high { background: #fef3c7; color: #92400e; }
.importance-medium { background: #dbeafe; color: #1e40af; }

.center-cell { text-align: center; }

.badge-ok { color: var(--success, #16a34a); font-weight: 700; }
.badge-warn {
  display: inline-block;
  background: #fee2e2;
  color: #991b1b;
  font-size: 0.7rem;
  padding: 0.1rem 0.4rem;
  border-radius: var(--radius-sm);
  font-weight: 600;
}

.btn-detail {
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
.btn-detail:hover { background: var(--border); color: var(--text); }

.action-cell { text-align: right; }

.position-form { display: flex; flex-direction: column; gap: 1.25rem; }
.form-row { display: grid; grid-template-columns: 2fr 1fr 1.5fr; gap: 1rem; }
.form-group { display: flex; flex-direction: column; gap: 0.4rem; }

.fab {
  position: fixed;
  bottom: 2rem;
  right: 2rem;
  width: 3.5rem;
  height: 3.5rem;
  border-radius: 50%;
  background: var(--accent);
  color: #fff;
  font-size: 1.5rem;
  border: none;
  cursor: pointer;
  box-shadow: 0 4px 12px rgba(0,0,0,0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}
.fab:hover { opacity: 0.9; }
</style>
