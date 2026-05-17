<template>
  <main class="container">
    <header class="page-header">
      <NuxtLink to="/" class="back-link">← ホームへ戻る</NuxtLink>
      <h1>スキルマスター管理</h1>
    </header>

    <div v-if="!isAdmin" class="error-box" role="alert">
      この画面は管理者専用です。
    </div>

    <template v-else>
      <div v-if="store.error" class="error-box" role="alert">{{ store.error }}</div>

      <!-- カテゴリ追加フォーム -->
      <section class="section">
        <h2>カテゴリ追加</h2>
        <form class="inline-form" @submit.prevent="submitCategory">
          <input
            v-model="catForm.name"
            class="input"
            placeholder="カテゴリ名"
            required
            :disabled="catSaving"
          />
          <input
            v-model.number="catForm.displayOrder"
            class="input input-sm"
            type="number"
            min="1"
            placeholder="表示順"
            required
            :disabled="catSaving"
          />
          <button class="btn-primary" type="submit" :disabled="catSaving">
            {{ catSaving ? '保存中…' : '追加' }}
          </button>
        </form>
      </section>

      <!-- カテゴリ一覧 -->
      <section class="section">
        <div v-if="store.loading" class="loading">読み込み中…</div>

        <div v-for="cat in store.categories" :key="cat.id" class="category-card">
          <!-- カテゴリヘッダー -->
          <div class="category-header">
            <template v-if="editingCat?.id === cat.id">
              <input v-model="editingCat.name" class="input input-inline" />
              <input v-model.number="editingCat.displayOrder" class="input input-sm" type="number" min="1" />
              <button class="btn-sm btn-primary" @click="saveCategory">保存</button>
              <button class="btn-sm btn-ghost" @click="editingCat = null">取消</button>
            </template>
            <template v-else>
              <span class="cat-order">{{ cat.displayOrder }}</span>
              <span class="cat-name">{{ cat.name }}</span>
              <div class="cat-actions">
                <button class="btn-sm btn-ghost" @click="startEditCat(cat)">編集</button>
                <button class="btn-sm btn-danger" @click="confirmDeleteCat(cat)">削除</button>
                <button
                  class="btn-sm btn-ghost"
                  @click="toggleItems(cat.id)"
                >
                  {{ expandedCats.has(cat.id) ? '▲ 閉じる' : '▼ スキル種別' }}
                </button>
              </div>
            </template>
          </div>

          <!-- スキル種別一覧 -->
          <div v-if="expandedCats.has(cat.id)" class="items-panel">
            <!-- スキル種別追加フォーム -->
            <form class="inline-form item-form" @submit.prevent="submitItem(cat.id)">
              <input
                v-model="itemForms[cat.id].name"
                class="input"
                placeholder="スキル名"
                required
              />
              <input
                v-model.number="itemForms[cat.id].displayOrder"
                class="input input-sm"
                type="number"
                min="1"
                placeholder="表示順"
                required
              />
              <button class="btn-sm btn-primary" type="submit">追加</button>
            </form>

            <div v-if="!store.itemsByCategory[cat.id]?.length" class="empty">
              スキル種別がありません
            </div>

            <div
              v-for="item in store.itemsByCategory[cat.id]"
              :key="item.id"
              class="item-row"
            >
              <template v-if="editingItem?.id === item.id">
                <input v-model="editingItem.name" class="input input-inline" />
                <input v-model.number="editingItem.displayOrder" class="input input-sm" type="number" min="1" />
                <button class="btn-sm btn-primary" @click="saveItem(cat.id)">保存</button>
                <button class="btn-sm btn-ghost" @click="editingItem = null">取消</button>
              </template>
              <template v-else>
                <span class="item-order">{{ item.displayOrder }}</span>
                <span class="item-name">{{ item.name }}</span>
                <div class="item-actions">
                  <button class="btn-sm btn-ghost" @click="startEditItem(item)">編集</button>
                  <button class="btn-sm btn-danger" @click="confirmDeleteItem(item, cat.id)">削除</button>
                </div>
              </template>
            </div>
          </div>
        </div>

        <p v-if="!store.loading && !store.categories.length" class="empty">
          カテゴリがまだありません。上のフォームから追加してください。
        </p>
      </section>
    </template>

    <!-- 削除確認ダイアログ -->
    <div v-if="deleteConfirm" class="overlay" @click.self="deleteConfirm = null">
      <div class="dialog">
        <p class="dialog-msg">{{ deleteConfirm.message }}</p>
        <div class="dialog-actions">
          <button class="btn-danger" @click="execDelete">削除する</button>
          <button class="btn-ghost" @click="deleteConfirm = null">キャンセル</button>
        </div>
      </div>
    </div>
  </main>
</template>

<script setup lang="ts">
import type { SkillCategory, SkillItem } from '~/types/skill'

const auth = useAuthStore()
const store = useSkillMasterStore()

const isAdmin = computed(() => auth.currentUser?.role === 'Admin')
const requesterId = computed(() => auth.currentUser?.id ?? '')

// カテゴリ追加フォーム
const catForm = reactive({ name: '', displayOrder: 1 })
const catSaving = ref(false)

// カテゴリ編集
const editingCat = ref<{ id: string, name: string, displayOrder: number } | null>(null)

// スキル種別追加フォーム（カテゴリIDごと）
const itemForms = reactive<Record<string, { name: string, displayOrder: number }>>({})

// スキル種別編集
const editingItem = ref<{ id: string, name: string, displayOrder: number } | null>(null)

// 展開中のカテゴリ
const expandedCats = ref<Set<string>>(new Set())

// 削除確認
const deleteConfirm = ref<{ message: string, action: () => Promise<void> } | null>(null)

onMounted(async () => {
  if (isAdmin.value) {
    await store.fetchCategories()
  }
})

async function submitCategory() {
  catSaving.value = true
  store.error = null
  try {
    await store.createCategory(requesterId.value, catForm.name, catForm.displayOrder)
    catForm.name = ''
    catForm.displayOrder = store.categories.length + 1
  }
  catch (e: unknown) {
    store.error = extractError(e) ?? 'カテゴリの追加に失敗しました。'
  }
  finally {
    catSaving.value = false
  }
}

function startEditCat(cat: SkillCategory) {
  editingCat.value = { id: cat.id, name: cat.name, displayOrder: cat.displayOrder }
}

async function saveCategory() {
  if (!editingCat.value) return
  store.error = null
  try {
    await store.updateCategory(requesterId.value, editingCat.value.id, editingCat.value.name, editingCat.value.displayOrder)
    editingCat.value = null
  }
  catch (e: unknown) {
    store.error = extractError(e) ?? 'カテゴリの更新に失敗しました。'
  }
}

function confirmDeleteCat(cat: SkillCategory) {
  deleteConfirm.value = {
    message: `「${cat.name}」を削除しますか？このカテゴリのスキル種別が残っている場合は削除できません。`,
    action: async () => {
      await store.deleteCategory(requesterId.value, cat.id)
    },
  }
}

async function toggleItems(categoryId: string) {
  if (expandedCats.value.has(categoryId)) {
    expandedCats.value.delete(categoryId)
  }
  else {
    expandedCats.value.add(categoryId)
    if (!store.itemsByCategory[categoryId]) {
      await store.fetchItems(categoryId)
    }
    if (!itemForms[categoryId]) {
      itemForms[categoryId] = { name: '', displayOrder: 1 }
    }
  }
}

async function submitItem(categoryId: string) {
  const form = itemForms[categoryId]
  if (!form) return
  store.error = null
  try {
    await store.createItem(requesterId.value, categoryId, form.name, form.displayOrder)
    form.name = ''
    form.displayOrder = (store.itemsByCategory[categoryId]?.length ?? 0) + 1
  }
  catch (e: unknown) {
    store.error = extractError(e) ?? 'スキル種別の追加に失敗しました。'
  }
}

function startEditItem(item: SkillItem) {
  editingItem.value = { id: item.id, name: item.name, displayOrder: item.displayOrder }
}

async function saveItem(categoryId: string) {
  if (!editingItem.value) return
  store.error = null
  try {
    await store.updateItem(requesterId.value, editingItem.value.id, categoryId, editingItem.value.name, editingItem.value.displayOrder)
    editingItem.value = null
  }
  catch (e: unknown) {
    store.error = extractError(e) ?? 'スキル種別の更新に失敗しました。'
  }
}

function confirmDeleteItem(item: SkillItem, categoryId: string) {
  deleteConfirm.value = {
    message: `「${item.name}」を削除しますか？タレントが使用中のスキルは削除できません。`,
    action: async () => {
      await store.deleteItem(requesterId.value, item.id, categoryId)
    },
  }
}

async function execDelete() {
  if (!deleteConfirm.value) return
  store.error = null
  try {
    await deleteConfirm.value.action()
    deleteConfirm.value = null
  }
  catch (e: unknown) {
    store.error = extractError(e) ?? '削除に失敗しました。'
    deleteConfirm.value = null
  }
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
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem 1rem;
  font-family: sans-serif;
}

.page-header {
  margin-bottom: 2rem;
}

.back-link {
  font-size: 0.85rem;
  color: #6b7280;
  text-decoration: none;
  display: inline-block;
  margin-bottom: 0.5rem;
}
.back-link:hover { color: #2563eb; }

h1 { font-size: 1.4rem; font-weight: 800; margin: 0; color: #111827; }
h2 { font-size: 1rem; font-weight: 700; color: #374151; margin: 0 0 0.75rem; }

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

.loading, .empty {
  color: #9ca3af;
  font-size: 0.9rem;
  padding: 0.5rem 0;
}

.inline-form {
  display: flex;
  gap: 0.5rem;
  align-items: center;
  flex-wrap: wrap;
}

.input {
  border: 1px solid #d1d5db;
  border-radius: 6px;
  padding: 0.45rem 0.75rem;
  font-size: 0.9rem;
  flex: 1;
  min-width: 120px;
}
.input-sm { flex: 0 0 80px; min-width: 80px; }
.input-inline { flex: 1; min-width: 120px; }
.input:focus { outline: 2px solid #2563eb; }

.category-card {
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  margin-bottom: 0.75rem;
  overflow: hidden;
}

.category-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  background: #f9fafb;
}

.cat-order {
  font-size: 0.75rem;
  color: #9ca3af;
  width: 2rem;
  text-align: center;
}

.cat-name {
  font-weight: 700;
  color: #111827;
  flex: 1;
}

.cat-actions { display: flex; gap: 0.4rem; margin-left: auto; }

.items-panel {
  padding: 0.75rem 1rem;
  border-top: 1px solid #e5e7eb;
}

.item-form { margin-bottom: 0.75rem; }

.item-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.4rem 0;
  border-bottom: 1px solid #f3f4f6;
}
.item-row:last-child { border-bottom: none; }

.item-order {
  font-size: 0.75rem;
  color: #9ca3af;
  width: 2rem;
  text-align: center;
}

.item-name { flex: 1; font-size: 0.9rem; color: #374151; }
.item-actions { display: flex; gap: 0.4rem; }

/* ボタン */
.btn-primary {
  background: #2563eb;
  color: #fff;
  border: none;
  border-radius: 6px;
  padding: 0.45rem 1rem;
  font-size: 0.9rem;
  cursor: pointer;
  white-space: nowrap;
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

/* ダイアログ */
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
  max-width: 360px;
  width: 100%;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.15);
}

.dialog-msg {
  font-size: 0.95rem;
  color: #111827;
  margin: 0 0 1.25rem;
  line-height: 1.5;
}

.dialog-actions {
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
}
</style>
