<template>
  <main class="container">
    <header class="page-header">
      <NuxtLink to="/" class="back-link">← ホームへ戻る</NuxtLink>
      <h1>スキル管理</h1>
      <p class="subtitle">{{ auth.currentUser?.displayName }} さんのスキル</p>
    </header>

    <div v-if="store.error" class="error-box" role="alert">{{ store.error }}</div>

    <!-- スキル追加フォーム -->
    <section class="section">
      <h2>スキルを追加</h2>
      <form class="add-form" @submit.prevent="submitAdd">
        <div class="form-row">
          <div class="form-group">
            <label class="label">カテゴリ</label>
            <select v-model="addForm.categoryId" class="select" required @change="addForm.skillItemId = ''">
              <option value="">選択してください</option>
              <option v-for="cat in masterStore.categories" :key="cat.id" :value="cat.id">
                {{ cat.name }}
              </option>
            </select>
          </div>
          <div class="form-group">
            <label class="label">スキル種別</label>
            <select v-model="addForm.skillItemId" class="select" required :disabled="!addForm.categoryId">
              <option value="">選択してください</option>
              <option
                v-for="item in availableItems"
                :key="item.id"
                :value="item.id"
              >
                {{ item.name }}
              </option>
            </select>
          </div>
          <div class="form-group form-group-sm">
            <label class="label">レベル</label>
            <select v-model.number="addForm.level" class="select" required>
              <option v-for="n in 5" :key="n" :value="n">{{ n }} - {{ levelLabel(n) }}</option>
            </select>
          </div>
        </div>
        <div class="form-group">
          <label class="label">補足コメント（任意）</label>
          <input v-model="addForm.comment" class="input" placeholder="業務経験・実績など" />
        </div>
        <div class="form-footer">
          <button class="btn-primary" type="submit" :disabled="addSaving">
            {{ addSaving ? '登録中…' : '追加する' }}
          </button>
        </div>
      </form>
    </section>

    <!-- スキル一覧 -->
    <section class="section">
      <h2>登録済みスキル</h2>

      <div v-if="store.loading" class="loading">読み込み中…</div>
      <p v-else-if="!store.skills.length" class="empty">
        スキルがまだ登録されていません。上のフォームから追加してください。
      </p>

      <div v-for="group in store.skillsByCategory" :key="group.categoryId" class="category-block">
        <h3 class="category-name">{{ group.categoryName }}</h3>
        <div
          v-for="skill in group.skills"
          :key="skill.id"
          class="skill-card"
        >
          <!-- 通常表示 -->
          <template v-if="editingId !== skill.id">
            <div class="skill-info">
              <span class="skill-name">{{ skill.skillItemName }}</span>
              <span class="level-badge">Lv.{{ skill.level }}</span>
              <span class="status-badge" :class="`status-${skill.status.toLowerCase()}`">
                {{ statusLabel(skill.status) }}
              </span>
            </div>
            <p v-if="skill.comment" class="skill-comment">{{ skill.comment }}</p>
            <p v-if="skill.rejectedReason" class="reject-reason">
              差し戻し理由: {{ skill.rejectedReason }}
            </p>
            <div class="skill-actions">
              <button class="btn-sm btn-ghost" @click="startEdit(skill)">編集</button>
              <button class="btn-sm btn-danger" @click="confirmDelete(skill)">削除</button>
            </div>
          </template>

          <!-- 編集フォーム -->
          <template v-else>
            <div class="edit-row">
              <span class="skill-name">{{ skill.skillItemName }}</span>
              <select v-model.number="editForm.level" class="select select-sm">
                <option v-for="n in 5" :key="n" :value="n">Lv.{{ n }} - {{ levelLabel(n) }}</option>
              </select>
            </div>
            <input v-model="editForm.comment" class="input" placeholder="補足コメント（任意）" />
            <div class="skill-actions">
              <button class="btn-sm btn-primary" :disabled="editSaving" @click="saveEdit(skill.id)">
                {{ editSaving ? '保存中…' : '保存' }}
              </button>
              <button class="btn-sm btn-ghost" @click="editingId = null">取消</button>
            </div>
          </template>
        </div>
      </div>
    </section>

    <!-- 削除確認ダイアログ -->
    <div v-if="deleteTarget" class="overlay" @click.self="deleteTarget = null">
      <div class="dialog">
        <p class="dialog-msg">「{{ deleteTarget.skillItemName }}」を削除しますか？</p>
        <div class="dialog-actions">
          <button class="btn-danger" @click="execDelete">削除する</button>
          <button class="btn-ghost" @click="deleteTarget = null">キャンセル</button>
        </div>
      </div>
    </div>
  </main>
</template>

<script setup lang="ts">
import type { UserSkill } from '~/types/skill'

const auth = useAuthStore()
const store = useUserSkillsStore()
const masterStore = useSkillMasterStore()

const userId = computed(() => auth.currentUser?.id ?? '')

const addForm = reactive({
  categoryId: '',
  skillItemId: '',
  level: 3,
  comment: '',
})
const addSaving = ref(false)

const editingId = ref<string | null>(null)
const editForm = reactive({ level: 3, comment: '' })
const editSaving = ref(false)

const deleteTarget = ref<UserSkill | null>(null)

const registeredItemIds = computed(() => new Set(store.skills.map(s => s.skillItemId)))

const availableItems = computed(() => {
  const items = masterStore.itemsByCategory[addForm.categoryId] ?? []
  return items.filter(i => !registeredItemIds.value.has(i.id))
})

onMounted(async () => {
  await Promise.all([
    store.fetchSkills(userId.value),
    masterStore.fetchCategories(),
  ])
})

watch(() => addForm.categoryId, async (id) => {
  if (id && !masterStore.itemsByCategory[id]) {
    await masterStore.fetchItems(id)
  }
})

async function submitAdd() {
  if (!addForm.skillItemId) return
  addSaving.value = true
  store.error = null
  try {
    await store.addSkill(userId.value, addForm.skillItemId, addForm.level, addForm.comment || null)
    addForm.skillItemId = ''
    addForm.comment = ''
    addForm.level = 3
  }
  catch (e: unknown) {
    store.error = extractError(e) ?? 'スキルの追加に失敗しました。'
  }
  finally {
    addSaving.value = false
  }
}

function startEdit(skill: UserSkill) {
  editingId.value = skill.id
  editForm.level = skill.level
  editForm.comment = skill.comment ?? ''
}

async function saveEdit(userSkillId: string) {
  editSaving.value = true
  store.error = null
  try {
    await store.updateSkill(userId.value, userSkillId, editForm.level, editForm.comment || null)
    editingId.value = null
  }
  catch (e: unknown) {
    store.error = extractError(e) ?? 'スキルの更新に失敗しました。'
  }
  finally {
    editSaving.value = false
  }
}

function confirmDelete(skill: UserSkill) {
  deleteTarget.value = skill
}

async function execDelete() {
  if (!deleteTarget.value) return
  store.error = null
  try {
    await store.deleteSkill(userId.value, deleteTarget.value.id)
    deleteTarget.value = null
  }
  catch (e: unknown) {
    store.error = extractError(e) ?? '削除に失敗しました。'
    deleteTarget.value = null
  }
}

function levelLabel(n: number) {
  return ['知識あり', '学習中', '実務可', '指導可', 'エキスパート'][n - 1] ?? ''
}

function statusLabel(status: string) {
  return { Pending: '申請中', Approved: '承認済み', Rejected: '差し戻し' }[status] ?? status
}

function extractError(e: unknown): string | null {
  if (e && typeof e === 'object' && 'data' in e) {
    const data = (e as { data: unknown }).data
    if (data && typeof data === 'object' && 'error' in data) {
      return String((data as { error: unknown }).error)
    }
  }
  return null
}
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
  font-size: 0.85rem;
  color: #6b7280;
  text-decoration: none;
  display: inline-block;
  margin-bottom: 0.5rem;
}
.back-link:hover { color: #2563eb; }

h1 { font-size: 1.4rem; font-weight: 800; margin: 0 0 0.25rem; color: #111827; }
h2 { font-size: 1rem; font-weight: 700; color: #374151; margin: 0 0 0.75rem; }
h3 { font-size: 0.9rem; font-weight: 700; color: #6b7280; margin: 0 0 0.5rem; }

.subtitle { font-size: 0.9rem; color: #9ca3af; margin: 0; }
.section { margin-bottom: 2rem; }

.error-box {
  background: #fef2f2;
  border: 1px solid #fca5a5;
  color: #991b1b;
  padding: 0.75rem 1rem;
  border-radius: 8px;
  margin-bottom: 1rem;
  font-size: 0.9rem;
}

.loading, .empty { color: #9ca3af; font-size: 0.9rem; padding: 0.5rem 0; }

.add-form {
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  padding: 1.25rem;
  background: #f9fafb;
}

.form-row {
  display: flex;
  gap: 0.75rem;
  flex-wrap: wrap;
  margin-bottom: 0.75rem;
}

.form-group { display: flex; flex-direction: column; gap: 0.3rem; flex: 1; min-width: 140px; }
.form-group-sm { flex: 0 0 160px; }
.form-footer { display: flex; justify-content: flex-end; margin-top: 0.75rem; }

.label { font-size: 0.8rem; font-weight: 600; color: #374151; }

.input, .select {
  border: 1px solid #d1d5db;
  border-radius: 6px;
  padding: 0.45rem 0.75rem;
  font-size: 0.9rem;
  background: #fff;
}
.select-sm { padding: 0.25rem 0.5rem; font-size: 0.85rem; }
.input:focus, .select:focus { outline: 2px solid #2563eb; }
.input:disabled, .select:disabled { background: #f3f4f6; color: #9ca3af; }

.category-block { margin-bottom: 1.5rem; }

.skill-card {
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 0.9rem 1rem;
  margin-bottom: 0.5rem;
  background: #fff;
}

.skill-info {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  margin-bottom: 0.3rem;
}

.skill-name { font-weight: 600; color: #111827; flex: 1; }

.level-badge {
  font-size: 0.78rem;
  font-weight: 700;
  background: #dbeafe;
  color: #1d4ed8;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
}

.status-badge {
  font-size: 0.75rem;
  font-weight: 700;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
}
.status-pending  { background: #fef3c7; color: #92400e; }
.status-approved { background: #d1fae5; color: #065f46; }
.status-rejected { background: #fee2e2; color: #991b1b; }

.skill-comment { font-size: 0.85rem; color: #6b7280; margin: 0.2rem 0; }
.reject-reason {
  font-size: 0.82rem;
  color: #991b1b;
  background: #fef2f2;
  padding: 0.3rem 0.6rem;
  border-radius: 4px;
  margin: 0.3rem 0;
}

.skill-actions { display: flex; gap: 0.4rem; margin-top: 0.5rem; }

.edit-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 0.5rem;
}

.btn-primary {
  background: #2563eb;
  color: #fff;
  border: none;
  border-radius: 6px;
  padding: 0.45rem 1.2rem;
  font-size: 0.9rem;
  cursor: pointer;
}
.btn-primary:hover:not(:disabled) { background: #1d4ed8; }
.btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }

.btn-ghost {
  background: none;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  padding: 0.45rem 1rem;
  font-size: 0.9rem;
  color: #374151;
  cursor: pointer;
}
.btn-ghost:hover { background: #f3f4f6; }

.btn-danger {
  background: #ef4444;
  color: #fff;
  border: none;
  border-radius: 6px;
  padding: 0.45rem 1rem;
  font-size: 0.9rem;
  cursor: pointer;
}
.btn-danger:hover { background: #dc2626; }

.btn-sm { padding: 0.25rem 0.6rem; font-size: 0.8rem; }

.overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}

.dialog {
  background: #fff;
  border-radius: 12px;
  padding: 1.5rem;
  max-width: 340px;
  width: 100%;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.15);
}

.dialog-msg { font-size: 0.95rem; color: #111827; margin: 0 0 1.25rem; line-height: 1.5; }

.dialog-actions { display: flex; gap: 0.75rem; justify-content: flex-end; }
</style>
