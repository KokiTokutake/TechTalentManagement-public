<template>
  <div class="page-wrap-wide">
    <header class="page-header">
      <div class="header-row">
        <div>
          <NuxtLink to="/mdc/succession/positions" class="breadcrumb">← 重要ポジション一覧</NuxtLink>
          <h1>{{ store.candidateList?.positionName ?? '読み込み中…' }}</h1>
          <p class="page-subtitle">候補者の管理・育成アクションの記録</p>
        </div>
        <button
          class="btn-primary"
          data-testid="candidate-add-button"
          @click="showAddDialog = true"
        >
          候補者を追加
        </button>
      </div>
    </header>

    <div v-if="store.loading" class="loading">読み込み中…</div>
    <div v-else-if="store.error" class="error-box">{{ store.error }}</div>

    <template v-else>
      <!-- Readiness groups -->
      <div v-for="group in readinessGroups" :key="group.level" class="card">
        <h2 class="section-title">
          <span :class="`readiness-label readiness-${group.level.toLowerCase()}`">{{ group.label }}</span>
          <span class="candidate-count">{{ group.candidates.length }}名</span>
        </h2>

        <div v-if="group.candidates.length === 0" class="empty-note">候補者なし</div>

        <div v-else class="candidate-list">
          <div v-for="c in group.candidates" :key="c.id" class="candidate-card">
            <div class="candidate-header">
              <span class="candidate-name">{{ c.talentName }}</span>
              <div class="candidate-actions">
                <select
                  :value="c.readiness"
                  class="readiness-select"
                  :disabled="updatingId === c.id"
                  @change="onReadinessChange(c, ($event.target as HTMLSelectElement).value)"
                >
                  <option value="ReadyNow">Ready Now</option>
                  <option value="ReadySoon">Ready Soon</option>
                  <option value="ReadyMidTerm">Ready Mid-Term</option>
                </select>
                <button
                  class="btn-save-readiness"
                  data-testid="candidate-readiness-save-button"
                  :disabled="pendingReadiness[c.id] === undefined || updatingId === c.id"
                  @click="saveReadiness(c)"
                >
                  {{ updatingId === c.id ? '保存中…' : '保存' }}
                </button>
                <button class="btn-remove" :disabled="removingId === c.id" @click="confirmRemove(c)">
                  削除
                </button>
              </div>
            </div>

            <div v-if="c.developmentGap" class="gap-text">
              <span class="gap-label">育成ギャップ:</span> {{ c.developmentGap }}
            </div>
            <div class="nominated-by">
              指名者: {{ c.nominatedByName }} · {{ formatDate(c.nominatedAt) }}
            </div>

            <!-- Development actions -->
            <div class="actions-section">
              <div class="actions-header">
                <span class="actions-title">育成アクション</span>
                <button class="btn-add-action" @click="openAddAction(c.id)">＋ 追加</button>
              </div>
              <div v-if="c.developmentActions.length === 0" class="empty-actions">アクションなし</div>
              <ul v-else class="action-list">
                <li v-for="action in c.developmentActions" :key="action.id" class="action-item">
                  <div class="action-body">
                    <span class="action-desc">{{ action.description }}</span>
                    <span v-if="action.dueDate" class="action-due">期限: {{ formatDate(action.dueDate) }}</span>
                  </div>
                  <select
                    :value="action.status"
                    class="action-status-select"
                    @change="onActionStatusChange(c.id, action, ($event.target as HTMLSelectElement).value)"
                  >
                    <option value="NotStarted">未着手</option>
                    <option value="InProgress">進行中</option>
                    <option value="Done">完了</option>
                  </select>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </template>

    <!-- Add candidate dialog -->
    <div v-if="showAddDialog" class="modal-overlay" @click.self="closeAddDialog">
      <div class="modal">
        <h3 class="modal-title">候補者を追加</h3>

        <div v-if="addError" class="error-box">{{ addError }}</div>

        <div class="form-group">
          <label class="label">タレント <span class="required">*</span></label>
          <select v-model="addForm.talentId" data-testid="candidate-talent-select" class="select">
            <option value="">選択してください</option>
            <option
              v-for="user in allUsers"
              :key="user.id"
              :value="user.id"
              :disabled="isAlreadyCandidate(user.id)"
            >
              {{ user.displayName }}{{ isAlreadyCandidate(user.id) ? '（登録済み）' : '' }}
            </option>
          </select>
        </div>

        <div class="form-group">
          <label class="label">Readiness <span class="required">*</span></label>
          <select v-model="addForm.readiness" data-testid="candidate-readiness-select" class="select">
            <option value="ReadyNow">Ready Now</option>
            <option value="ReadySoon">Ready Soon</option>
            <option value="ReadyMidTerm">Ready Mid-Term</option>
          </select>
        </div>

        <div class="form-group">
          <label class="label">育成ギャップ（任意）</label>
          <textarea v-model="addForm.developmentGap" class="textarea" rows="3" placeholder="現時点での課題や育成すべき点" />
        </div>

        <div class="suggest-section">
          <button
            class="btn-suggest"
            data-testid="candidate-suggest-button"
            :disabled="!periodId || loadingSuggestions"
            @click="loadSuggestions"
          >
            {{ loadingSuggestions ? '読み込み中…' : '推薦候補を表示（9ボックス連動）' }}
          </button>
          <div v-if="store.suggestions" data-testid="candidate-suggest-list" class="suggest-list">
            <div
              v-for="s in store.suggestions.suggestions"
              :key="s.talentId"
              class="suggest-item"
              :class="{ 'already-candidate': s.alreadyCandidate }"
            >
              <span class="suggest-name">{{ s.talentName }}</span>
              <span class="suggest-level">Potential: {{ s.potentialLevel }}</span>
              <button
                v-if="!s.alreadyCandidate"
                class="btn-select-suggest"
                @click="selectSuggestion(s.talentId)"
              >
                選択
              </button>
              <span v-else class="already-badge">登録済み</span>
            </div>
          </div>
        </div>

        <div class="modal-actions">
          <button class="btn-ghost" @click="closeAddDialog">キャンセル</button>
          <button
            class="btn-primary"
            data-testid="candidate-confirm-button"
            :disabled="!addForm.talentId || addingCandidate"
            @click="submitAdd"
          >
            {{ addingCandidate ? '追加中…' : '追加する' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Add development action dialog -->
    <div v-if="showActionDialog" class="modal-overlay" @click.self="closeActionDialog">
      <div class="modal modal-sm">
        <h3 class="modal-title">育成アクションを追加</h3>
        <div class="form-group">
          <label class="label">内容 <span class="required">*</span></label>
          <textarea v-model="actionForm.description" class="textarea" rows="3" required />
        </div>
        <div class="form-group">
          <label class="label">期限（任意）</label>
          <input v-model="actionForm.dueDate" type="date" class="input">
        </div>
        <div class="modal-actions">
          <button class="btn-ghost" @click="closeActionDialog">キャンセル</button>
          <button class="btn-primary" :disabled="!actionForm.description || addingAction" @click="submitAction">
            {{ addingAction ? '追加中…' : '追加' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Remove confirm modal -->
    <div v-if="removeTarget" class="modal-overlay" @click.self="removeTarget = null">
      <div class="modal modal-sm">
        <h3 class="modal-title">候補者を削除</h3>
        <p class="modal-body">
          <strong>{{ removeTarget.talentName }}</strong> を候補者から削除しますか？
        </p>
        <div class="modal-actions">
          <button class="btn-ghost" @click="removeTarget = null">キャンセル</button>
          <button class="btn-danger" :disabled="removingId === removeTarget.id" @click="executeRemove">
            {{ removingId === removeTarget.id ? '削除中…' : '削除する' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { AppUser } from '~/types/user'
import type { CandidateDto, ReadinessLevel, ActionStatus } from '~/types/succession'

const route = useRoute()
const positionId = route.params.positionId as string

const auth = useAuthStore()
const store = useSuccessionStore()

const allowedRoles = ['Senior', 'Executive', 'Admin']
if (!auth.currentUser || !allowedRoles.includes(auth.currentUser.role)) await navigateTo('/')

const allUsers = ref<AppUser[]>([])
const periodId = ref<string | null>(null)

const showAddDialog = ref(false)
const addError = ref<string | null>(null)
const addingCandidate = ref(false)
const addForm = ref<{ talentId: string; readiness: ReadinessLevel; developmentGap: string }>({
  talentId: '',
  readiness: 'ReadySoon',
  developmentGap: '',
})
const loadingSuggestions = ref(false)

const removeTarget = ref<CandidateDto | null>(null)
const removingId = ref<string | null>(null)

const updatingId = ref<string | null>(null)
const pendingReadiness = ref<Record<string, ReadinessLevel>>({})

const showActionDialog = ref(false)
const addingAction = ref(false)
const targetCandidateId = ref<string | null>(null)
const actionForm = ref<{ description: string; dueDate: string }>({ description: '', dueDate: '' })

const readinessGroups = computed(() => {
  const candidates = store.candidateList?.candidates ?? []
  return [
    { level: 'ReadyNow', label: 'Ready Now', candidates: candidates.filter(c => c.readiness === 'ReadyNow') },
    { level: 'ReadySoon', label: 'Ready Soon', candidates: candidates.filter(c => c.readiness === 'ReadySoon') },
    { level: 'ReadyMidTerm', label: 'Ready Mid-Term', candidates: candidates.filter(c => c.readiness === 'ReadyMidTerm') },
  ]
})

function isAlreadyCandidate(talentId: string) {
  return store.candidateList?.candidates.some(c => c.talentId === talentId) ?? false
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('ja-JP', { year: 'numeric', month: '2-digit', day: '2-digit' })
}

function closeAddDialog() {
  showAddDialog.value = false
  addError.value = null
  store.suggestions = null
  addForm.value = { talentId: '', readiness: 'ReadySoon', developmentGap: '' }
}

async function loadSuggestions() {
  if (!auth.currentUser || !periodId.value) return
  loadingSuggestions.value = true
  try {
    await store.fetchSuggestions(auth.currentUser.id, periodId.value, positionId)
  }
  finally {
    loadingSuggestions.value = false
  }
}

function selectSuggestion(talentId: string) {
  addForm.value.talentId = talentId
}

async function submitAdd() {
  if (!auth.currentUser || !addForm.value.talentId) return
  addingCandidate.value = true
  addError.value = null
  try {
    await store.addCandidate(auth.currentUser.id, positionId, {
      talentId: addForm.value.talentId,
      readiness: addForm.value.readiness,
      developmentGap: addForm.value.developmentGap || null,
    })
    closeAddDialog()
  }
  catch (e: unknown) {
    const err = e as { status?: number }
    addError.value = err?.status === 409
      ? 'このタレントはすでに候補者として登録されています。'
      : '候補者の追加に失敗しました。'
  }
  finally {
    addingCandidate.value = false
  }
}

function onReadinessChange(c: CandidateDto, value: string) {
  pendingReadiness.value[c.id] = value as ReadinessLevel
}

async function saveReadiness(c: CandidateDto) {
  if (!auth.currentUser || !pendingReadiness.value[c.id]) return
  updatingId.value = c.id
  try {
    await store.updateCandidate(auth.currentUser.id, c.id, {
      readiness: pendingReadiness.value[c.id],
      developmentGap: c.developmentGap,
    })
    delete pendingReadiness.value[c.id]
  }
  finally {
    updatingId.value = null
  }
}

function confirmRemove(c: CandidateDto) {
  removeTarget.value = c
}

async function executeRemove() {
  if (!auth.currentUser || !removeTarget.value) return
  removingId.value = removeTarget.value.id
  try {
    await store.removeCandidate(auth.currentUser.id, removeTarget.value.id)
    removeTarget.value = null
  }
  finally {
    removingId.value = null
  }
}

function openAddAction(candidateId: string) {
  targetCandidateId.value = candidateId
  actionForm.value = { description: '', dueDate: '' }
  showActionDialog.value = true
}

function closeActionDialog() {
  showActionDialog.value = false
  targetCandidateId.value = null
}

async function submitAction() {
  if (!auth.currentUser || !targetCandidateId.value || !actionForm.value.description) return
  addingAction.value = true
  try {
    await store.addDevelopmentAction(auth.currentUser.id, targetCandidateId.value, {
      description: actionForm.value.description,
      dueDate: actionForm.value.dueDate || null,
    })
    closeActionDialog()
  }
  finally {
    addingAction.value = false
  }
}

async function onActionStatusChange(candidateId: string, action: { id: string; description: string; dueDate: string | null; status: ActionStatus }, value: string) {
  if (!auth.currentUser) return
  await store.updateDevelopmentAction(auth.currentUser.id, action.id, {
    description: action.description,
    dueDate: action.dueDate,
    status: value as ActionStatus,
  })
}

onMounted(async () => {
  if (!auth.currentUser) return
  const config = useRuntimeConfig()
  const [, users, periods] = await Promise.all([
    store.fetchCandidates(auth.currentUser.id, positionId),
    $fetch<AppUser[]>(`${config.public.apiBaseUrl}/api/users`),
    $fetch<{ periods: { id: string; isCurrent: boolean }[] }>(`${config.public.apiBaseUrl}/api/periods`).catch(() => ({ periods: [] })),
  ])
  allUsers.value = users
  const currentPeriod = periods.periods.find(p => p.isCurrent) ?? periods.periods[0]
  if (currentPeriod) periodId.value = currentPeriod.id
})
</script>

<style scoped>
.header-row { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; }
.breadcrumb { font-size: 0.82rem; color: var(--text-muted); text-decoration: none; display: block; margin-bottom: 0.5rem; }
.breadcrumb:hover { color: var(--accent); }

.readiness-label {
  display: inline-block;
  padding: 0.2rem 0.6rem;
  border-radius: var(--radius-sm);
  font-size: 0.8rem;
  font-weight: 700;
  margin-right: 0.5rem;
}
.readiness-readynow { background: #dcfce7; color: #166534; }
.readiness-readysoon { background: #fef3c7; color: #92400e; }
.readiness-readymidterm { background: #dbeafe; color: #1e40af; }

.candidate-count { font-size: 0.85rem; font-weight: 400; color: var(--text-muted); }

.candidate-list { display: flex; flex-direction: column; gap: 1rem; }
.candidate-card {
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 1rem;
  background: var(--surface-muted);
}

.candidate-header { display: flex; align-items: center; justify-content: space-between; gap: 1rem; margin-bottom: 0.5rem; }
.candidate-name { font-weight: 700; font-size: 1rem; }
.candidate-actions { display: flex; align-items: center; gap: 0.5rem; }

.readiness-select {
  border: 1px solid var(--border-strong);
  border-radius: var(--radius-sm);
  padding: 0.25rem 0.5rem;
  font-size: 0.82rem;
  background: var(--surface);
  color: var(--text);
  font-family: var(--font-body);
}

.btn-save-readiness {
  border: 1px solid var(--accent);
  color: var(--accent);
  background: none;
  border-radius: var(--radius-sm);
  padding: 0.25rem 0.6rem;
  font-size: 0.8rem;
  cursor: pointer;
  font-family: var(--font-body);
}
.btn-save-readiness:disabled { opacity: 0.4; cursor: not-allowed; }
.btn-save-readiness:not(:disabled):hover { background: var(--accent); color: #fff; }

.btn-remove {
  background: none;
  border: 1px solid var(--danger-border);
  color: var(--danger);
  border-radius: var(--radius-sm);
  padding: 0.25rem 0.6rem;
  font-size: 0.8rem;
  cursor: pointer;
  font-family: var(--font-body);
}
.btn-remove:hover:not(:disabled) { background: var(--danger-bg); }
.btn-remove:disabled { opacity: 0.4; cursor: not-allowed; }

.gap-text { font-size: 0.85rem; color: var(--text-secondary); margin-bottom: 0.25rem; }
.gap-label { font-weight: 600; }
.nominated-by { font-size: 0.78rem; color: var(--text-muted); margin-bottom: 0.75rem; }

.actions-section { border-top: 1px solid var(--border); padding-top: 0.75rem; margin-top: 0.75rem; }
.actions-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 0.5rem; }
.actions-title { font-size: 0.85rem; font-weight: 600; }

.btn-add-action {
  background: none;
  border: 1px solid var(--border-strong);
  border-radius: var(--radius-sm);
  padding: 0.15rem 0.5rem;
  font-size: 0.78rem;
  cursor: pointer;
  color: var(--text-secondary);
  font-family: var(--font-body);
}
.btn-add-action:hover { background: var(--surface-muted); }

.empty-actions { font-size: 0.8rem; color: var(--text-muted); }

.action-list { list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: 0.4rem; }
.action-item { display: flex; align-items: center; justify-content: space-between; gap: 0.75rem; }
.action-body { display: flex; flex-direction: column; gap: 0.1rem; flex: 1; min-width: 0; }
.action-desc { font-size: 0.85rem; }
.action-due { font-size: 0.75rem; color: var(--text-muted); }
.action-status-select {
  border: 1px solid var(--border-strong);
  border-radius: var(--radius-sm);
  padding: 0.2rem 0.4rem;
  font-size: 0.78rem;
  background: var(--surface);
  color: var(--text);
  font-family: var(--font-body);
  white-space: nowrap;
}

/* Dialog */
.modal-sm { max-width: 420px; }
.modal-title { font-family: var(--font-heading); font-size: 1.1rem; font-weight: 700; color: var(--text); margin-bottom: 1rem; }
.modal-body { font-size: 0.9rem; color: var(--text-secondary); margin-bottom: 1.5rem; }
.modal-actions { display: flex; justify-content: flex-end; gap: 0.75rem; margin-top: 1.5rem; }

.form-group { display: flex; flex-direction: column; gap: 0.4rem; margin-bottom: 1rem; }

.suggest-section { margin-top: 0.75rem; }
.btn-suggest {
  background: none;
  border: 1px dashed var(--border-strong);
  border-radius: var(--radius-sm);
  padding: 0.4rem 0.8rem;
  font-size: 0.82rem;
  cursor: pointer;
  color: var(--text-secondary);
  font-family: var(--font-body);
  width: 100%;
}
.btn-suggest:hover:not(:disabled) { background: var(--surface-muted); }
.btn-suggest:disabled { opacity: 0.5; cursor: not-allowed; }

.suggest-list { margin-top: 0.75rem; display: flex; flex-direction: column; gap: 0.4rem; }
.suggest-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.4rem 0.6rem;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  background: var(--surface);
}
.suggest-item.already-candidate { opacity: 0.5; }
.suggest-name { flex: 1; font-size: 0.85rem; font-weight: 600; }
.suggest-level { font-size: 0.75rem; color: var(--text-muted); }
.btn-select-suggest {
  background: var(--accent);
  color: #fff;
  border: none;
  border-radius: var(--radius-sm);
  padding: 0.2rem 0.5rem;
  font-size: 0.75rem;
  cursor: pointer;
  font-family: var(--font-body);
}
.already-badge { font-size: 0.72rem; color: var(--text-muted); }
</style>
