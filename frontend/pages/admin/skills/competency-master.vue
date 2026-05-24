<template>
  <div class="page-wrap">
    <header class="page-header">
      <NuxtLink to="/" class="back-link">← ホームへ戻る</NuxtLink>
      <h1>コンピテンシーマスタ管理</h1>
      <p class="subtitle">コンピテンシーの追加・編集・無効化（Admin 専用）</p>
    </header>

    <div v-if="!isAdmin" class="error-box" role="alert">この画面は管理者専用です。</div>

    <template v-else>
      <!-- 追加フォーム -->
      <section class="section">
        <div class="card">
          <h2 class="section-title">コンピテンシー追加</h2>
          <div v-if="addError" class="error-box" role="alert">{{ addError }}</div>
          <form class="add-form" @submit.prevent="handleAdd">
            <div class="form-row">
              <div class="form-group">
                <label class="label">カテゴリ</label>
                <select
                  v-model="addForm.category"
                  class="select"
                  required
                  data-testid="competency-category-select"
                >
                  <option value="">選択してください</option>
                  <option v-for="cat in categoryOptions" :key="cat.value" :value="cat.value">
                    {{ cat.label }}
                  </option>
                </select>
              </div>
              <div class="form-group">
                <label class="label">名称</label>
                <input
                  v-model="addForm.name"
                  class="input"
                  placeholder="コンピテンシー名"
                  required
                  data-testid="competency-name-input"
                  :disabled="adding"
                />
              </div>
              <div class="form-group form-group-sm">
                <label class="label">表示順</label>
                <input
                  v-model.number="addForm.displayOrder"
                  type="number"
                  class="input input-sm"
                  min="1"
                  required
                  :disabled="adding"
                />
              </div>
            </div>
            <div class="form-group">
              <label class="label">説明（任意）</label>
              <input v-model="addForm.description" class="input" placeholder="説明" :disabled="adding" />
            </div>
            <div class="form-footer">
              <button
                type="submit"
                class="btn-primary"
                :disabled="adding"
                data-testid="competency-add-button"
              >
                {{ adding ? '追加中…' : '追加' }}
              </button>
            </div>
          </form>
        </div>
      </section>

      <!-- 一覧 -->
      <section class="section">
        <div class="card">
          <h2 class="section-title">コンピテンシー一覧</h2>
          <div v-if="masterStore.error" class="error-box" role="alert">{{ masterStore.error }}</div>
          <div v-if="masterStore.loading" class="loading">読み込み中…</div>
          <template v-else>
            <div v-for="[cat, items] in masterStore.byCategory" :key="cat" class="cat-group">
              <h3 class="cat-title">{{ categoryLabel(cat) }}</h3>
              <div class="comp-list">
                <div
                  v-for="comp in items"
                  :key="comp.id"
                  class="comp-row"
                  :class="{ inactive: !comp.isActive }"
                >
                  <template v-if="editingId === comp.id">
                    <div class="edit-form">
                      <input v-model="editForm.name" class="input input-inline" />
                      <input v-model="editForm.description" class="input input-inline" placeholder="説明" />
                      <input v-model.number="editForm.displayOrder" type="number" class="input input-sm" min="1" />
                      <label class="check-label">
                        <input v-model="editForm.isActive" type="checkbox" /> 有効
                      </label>
                      <button class="btn-sm btn-primary" :disabled="saving" @click="handleSave(comp.id)">保存</button>
                      <button class="btn-sm btn-ghost" @click="editingId = null">取消</button>
                    </div>
                  </template>
                  <template v-else>
                    <div class="comp-info">
                      <span class="comp-order">{{ comp.displayOrder }}</span>
                      <span class="comp-name">{{ comp.name }}</span>
                      <span v-if="comp.description" class="comp-desc">{{ comp.description }}</span>
                      <span v-if="!comp.isActive" class="inactive-badge">無効</span>
                    </div>
                    <button class="btn-sm btn-ghost" @click="startEdit(comp)">編集</button>
                  </template>
                </div>
              </div>
            </div>
            <div v-if="masterStore.competencies.length === 0" class="empty-note">
              コンピテンシーが登録されていません。
            </div>
          </template>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import type { CompetencyMaster, CompetencyCategory } from '~/types/skill'

const auth = useAuthStore()
const masterStore = useCompetencyMasterStore()
const config = useRuntimeConfig()

const isAdmin = computed(() => auth.currentUser?.role === 'Admin')

const categoryOptions = [
  { value: 'Technology', label: 'テクノロジー' },
  { value: 'Methodology', label: 'メソドロジー' },
  { value: 'BusinessIndustry', label: 'ビジネス・インダストリー' },
  { value: 'Common', label: '共通' },
]

function categoryLabel(cat: CompetencyCategory): string {
  return categoryOptions.find(o => o.value === cat)?.label ?? cat
}

// 追加フォーム
const addForm = reactive({ category: '' as CompetencyCategory | '', name: '', description: '', displayOrder: 1 })
const adding = ref(false)
const addError = ref<string | null>(null)

// 編集
const editingId = ref<string | null>(null)
const editForm = reactive({ name: '', description: '', displayOrder: 1, isActive: true })
const saving = ref(false)

onMounted(async () => {
  await masterStore.fetchCompetencies(true)
})

async function handleAdd() {
  adding.value = true
  addError.value = null
  try {
    await $fetch(
      `${config.public.apiBaseUrl}/api/competency-masters`,
      {
        method: 'POST',
        body: {
          category: addForm.category,
          name: addForm.name,
          description: addForm.description || null,
          displayOrder: addForm.displayOrder,
        },
        headers: { 'X-User-Id': auth.currentUser?.id ?? '' },
      },
    )
    addForm.name = ''
    addForm.description = ''
    addForm.displayOrder = 1
    await masterStore.fetchCompetencies(true)
  }
  catch {
    addError.value = 'コンピテンシーの追加に失敗しました。重複がないか確認してください。'
  }
  finally {
    adding.value = false
  }
}

function startEdit(comp: CompetencyMaster) {
  editingId.value = comp.id
  editForm.name = comp.name
  editForm.description = comp.description ?? ''
  editForm.displayOrder = comp.displayOrder
  editForm.isActive = comp.isActive
}

async function handleSave(id: string) {
  saving.value = true
  try {
    await $fetch(
      `${config.public.apiBaseUrl}/api/competency-masters/${id}`,
      {
        method: 'PUT',
        body: {
          name: editForm.name,
          description: editForm.description || null,
          displayOrder: editForm.displayOrder,
          isActive: editForm.isActive,
        },
        headers: { 'X-User-Id': auth.currentUser?.id ?? '' },
      },
    )
    editingId.value = null
    await masterStore.fetchCompetencies(true)
  }
  catch {
    addError.value = 'コンピテンシーの更新に失敗しました。'
  }
  finally {
    saving.value = false
  }
}
</script>

<style scoped>
.page-header { margin-bottom: 1.5rem; }
h1 { font-size: 1.4rem; font-weight: 800; margin: 0 0 0.25rem; color: var(--text); font-family: var(--font-heading); }
.subtitle { font-size: 0.9rem; color: var(--text-muted); margin: 0; }
.back-link { font-size: 0.85rem; color: var(--accent); text-decoration: none; display: block; margin-bottom: 0.5rem; }
.back-link:hover { text-decoration: underline; }

.section { margin-bottom: 1.25rem; }
.section-title { font-family: var(--font-heading); font-size: 1rem; font-weight: 700; margin-bottom: 1rem; }

.add-form { display: flex; flex-direction: column; gap: 0.75rem; }
.form-row { display: flex; gap: 0.75rem; flex-wrap: wrap; }
.form-group { display: flex; flex-direction: column; gap: 0.3rem; flex: 1; min-width: 140px; }
.form-group-sm { flex: 0 0 80px; }
.form-footer { display: flex; gap: 0.5rem; align-items: center; }

.cat-group { margin-bottom: 1.25rem; }
.cat-title { font-size: 0.875rem; font-weight: 700; color: var(--text-secondary); margin: 0 0 0.5rem; }

.comp-list { display: flex; flex-direction: column; gap: 0.35rem; }

.comp-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  padding: 0.5rem 0.75rem;
  border-radius: var(--radius);
  border: 1px solid var(--border);
  background: var(--surface);
}

.comp-row.inactive { opacity: 0.5; }

.comp-info { display: flex; align-items: center; gap: 0.5rem; flex: 1; flex-wrap: wrap; }
.comp-order { font-size: 0.75rem; color: var(--text-muted); min-width: 24px; }
.comp-name { font-size: 0.875rem; font-weight: 600; }
.comp-desc { font-size: 0.8rem; color: var(--text-muted); }
.inactive-badge {
  font-size: 0.7rem;
  font-weight: 700;
  padding: 0.1rem 0.4rem;
  border-radius: 3px;
  background: var(--surface-muted);
  color: var(--text-muted);
}

.edit-form { display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap; flex: 1; }
.input-inline { flex: 1; min-width: 120px; }
.check-label { display: flex; align-items: center; gap: 0.3rem; font-size: 0.85rem; cursor: pointer; }

.empty-note { color: var(--text-muted); font-size: 0.9rem; padding: 1rem 0; text-align: center; }
</style>
