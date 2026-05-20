<template>
  <div class="page-wrap">
    <header class="page-header">
      <h1>スキル管理</h1>
      <p class="page-subtitle">{{ auth.currentUser?.displayName }} さんのスキル</p>
    </header>

    <div v-if="store.error" class="error-box" role="alert">{{ store.error }}</div>

    <!-- Add skill form -->
    <section class="section">
      <div class="card">
        <h2 class="section-title">スキルを追加</h2>
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
                <option v-for="item in availableItems" :key="item.id" :value="item.id">
                  {{ item.name }}
                </option>
              </select>
            </div>
            <div class="form-group form-group-sm">
              <label class="label">レベル</label>
              <select v-model.number="addForm.level" class="select" required>
                <option v-for="n in 5" :key="n" :value="n">{{ n }} — {{ levelLabel(n) }}</option>
              </select>
            </div>
          </div>
          <div class="form-group">
            <label class="label">補足コメント（任意）</label>
            <input v-model="addForm.comment" class="input" placeholder="業務経験・実績など" />
          </div>
          <div class="form-footer">
            <button class="btn-primary" type="submit" :disabled="addSaving">
              {{ addSaving ? '登録中…' : 'スキルを追加する' }}
            </button>
          </div>
        </form>
      </div>
    </section>

    <!-- Skill list -->
    <section class="section">
      <h2 class="section-title-plain">登録済みスキル</h2>
      <div v-if="store.loading" class="loading">読み込み中…</div>
      <p v-else-if="!store.skills.length" class="empty-note">
        スキルがまだ登録されていません。上のフォームから追加してください。
      </p>

      <div v-for="group in store.skillsByCategory" :key="group.categoryId" class="category-block">
        <h3 class="category-label">{{ group.categoryName }}</h3>
        <div class="card skill-card-group">
          <div v-for="(skill, index) in group.skills" :key="skill.id" class="skill-card"
            :class="{ 'skill-card-border': index > 0 }">
            <template v-if="editingId !== skill.id">
              <div class="skill-row">
                <div class="skill-info">
                  <span class="skill-name">{{ skill.skillItemName }}</span>
                  <span class="level-pill">Lv.{{ skill.level }}</span>
                  <span class="status-badge" :class="`status-${skill.status.toLowerCase()}`">
                    {{ statusLabel(skill.status) }}
                  </span>
                </div>
                <div class="skill-actions">
                  <button class="btn-ghost btn-sm" @click="startEdit(skill)">編集</button>
                  <button class="btn-danger btn-sm" @click="confirmDelete(skill)">削除</button>
                </div>
              </div>
              <p v-if="skill.comment" class="skill-comment">{{ skill.comment }}</p>
              <p v-if="skill.rejectedReason" class="reject-reason">
                差し戻し理由: {{ skill.rejectedReason }}
              </p>
            </template>

            <template v-else>
              <div class="edit-row">
                <span class="skill-name">{{ skill.skillItemName }}</span>
                <select v-model.number="editForm.level" class="select select-sm">
                  <option v-for="n in 5" :key="n" :value="n">Lv.{{ n }} — {{ levelLabel(n) }}</option>
                </select>
              </div>
              <input v-model="editForm.comment" class="input" placeholder="補足コメント（任意）" style="margin:0.5rem 0" />
              <div class="skill-actions">
                <button class="btn-primary btn-sm" :disabled="editSaving" @click="saveEdit(skill.id)">
                  {{ editSaving ? '保存中…' : '保存' }}
                </button>
                <button class="btn-ghost btn-sm" @click="editingId = null">取消</button>
              </div>
            </template>
          </div>
        </div>
      </div>
    </section>

    <!-- Delete confirm -->
    <div v-if="deleteTarget" class="overlay" @click.self="deleteTarget = null">
      <div class="dialog">
        <h3 class="dialog-title">スキルの削除</h3>
        <p class="dialog-msg">「{{ deleteTarget.skillItemName }}」を削除しますか？この操作は取り消せません。</p>
        <div class="dialog-actions">
          <button class="btn-ghost" @click="deleteTarget = null">キャンセル</button>
          <button class="btn-danger" @click="execDelete">削除する</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { UserSkill } from '~/types/skill'

const auth = useAuthStore()
const store = useUserSkillsStore()
const masterStore = useSkillMasterStore()

const userId = computed(() => auth.currentUser?.id ?? '')

const addForm = reactive({ categoryId: '', skillItemId: '', level: 3, comment: '' })
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
  await Promise.all([store.fetchSkills(userId.value), masterStore.fetchCategories()])
})

watch(() => addForm.categoryId, async (id) => {
  if (id && !masterStore.itemsByCategory[id]) await masterStore.fetchItems(id)
})

async function submitAdd() {
  if (!addForm.skillItemId) return
  addSaving.value = true; store.error = null
  try {
    await store.addSkill(userId.value, addForm.skillItemId, addForm.level, addForm.comment || null)
    addForm.skillItemId = ''; addForm.comment = ''; addForm.level = 3
  }
  catch (e: unknown) { store.error = extractError(e) ?? 'スキルの追加に失敗しました。' }
  finally { addSaving.value = false }
}

function startEdit(skill: UserSkill) {
  editingId.value = skill.id
  editForm.level = skill.level
  editForm.comment = skill.comment ?? ''
}

async function saveEdit(userSkillId: string) {
  editSaving.value = true; store.error = null
  try {
    await store.updateSkill(userId.value, userSkillId, editForm.level, editForm.comment || null)
    editingId.value = null
  }
  catch (e: unknown) { store.error = extractError(e) ?? 'スキルの更新に失敗しました。' }
  finally { editSaving.value = false }
}

function confirmDelete(skill: UserSkill) { deleteTarget.value = skill }

async function execDelete() {
  if (!deleteTarget.value) return
  store.error = null
  try {
    await store.deleteSkill(userId.value, deleteTarget.value.id)
    deleteTarget.value = null
  }
  catch (e: unknown) { store.error = extractError(e) ?? '削除に失敗しました。'; deleteTarget.value = null }
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
    if (data && typeof data === 'object' && 'error' in data) return String((data as { error: unknown }).error)
  }
  return null
}
</script>

<style scoped>
.add-form { display: flex; flex-direction: column; gap: 0.75rem; }
.form-row { display: flex; gap: 0.75rem; flex-wrap: wrap; }
.form-group { display: flex; flex-direction: column; gap: 0.3rem; flex: 1; min-width: 140px; }
.form-group-sm { flex: 0 0 160px; }
.form-footer { display: flex; justify-content: flex-end; margin-top: 0.25rem; }

.section-title-plain {
  font-family: var(--font-heading);
  font-size: 1rem;
  font-weight: 700;
  color: var(--text);
  margin-bottom: 1rem;
}

.category-block { margin-bottom: 1.5rem; }
.category-label { font-size: 0.75rem; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.06em; margin-bottom: 0.5rem; }

.skill-card-group { padding: 0; overflow: hidden; }
.skill-card { padding: 0.875rem 1rem; }
.skill-card-border { border-top: 1px solid var(--border); }

.skill-row { display: flex; align-items: center; justify-content: space-between; gap: 0.75rem; }
.skill-info { display: flex; align-items: center; gap: 0.6rem; flex: 1; flex-wrap: wrap; }
.skill-name { font-weight: 600; color: var(--text); font-size: 0.9rem; }
.level-pill { font-size: 0.72rem; font-family: var(--font-mono); font-weight: 700; color: var(--accent); }
.skill-actions { display: flex; gap: 0.4rem; flex-shrink: 0; }
.skill-comment { font-size: 0.82rem; color: var(--text-muted); margin-top: 0.3rem; }
.reject-reason { font-size: 0.8rem; color: var(--danger-text); background: var(--danger-bg); padding: 0.3rem 0.6rem; border-radius: var(--radius-sm); margin-top: 0.3rem; }

.edit-row { display: flex; align-items: center; gap: 0.75rem; }
.select-sm { width: auto; min-width: 150px; }

.dialog-title { font-family: var(--font-heading); font-size: 1.1rem; font-weight: 700; color: var(--text); margin-bottom: 0.75rem; }
.dialog-msg { font-size: 0.9rem; color: var(--text-secondary); margin-bottom: 1.5rem; }
.dialog-actions { display: flex; gap: 0.75rem; justify-content: flex-end; }
</style>
