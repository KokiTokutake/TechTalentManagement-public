<template>
  <div class="page-wrap">
    <header class="page-header">
      <NuxtLink to="/" class="back-link">← ホームへ戻る</NuxtLink>
      <h1>スキルレベル定義管理</h1>
      <p class="subtitle">各スキルの 1〜7 レベル定義を編集します（Admin 専用）</p>
    </header>

    <div v-if="!isAdmin" class="error-box" role="alert">この画面は管理者専用です。</div>

    <template v-else>
      <!-- スキル選択 -->
      <section class="section">
        <div class="card">
          <div class="form-row">
            <div class="form-group">
              <label class="label">カテゴリ</label>
              <select v-model="selectedCategoryId" class="select" @change="selectedItemId = ''">
                <option value="">カテゴリを選択</option>
                <option v-for="cat in masterStore.categories" :key="cat.id" :value="cat.id">
                  {{ cat.name }}
                </option>
              </select>
            </div>
            <div class="form-group">
              <label class="label">スキルアイテム</label>
              <select v-model="selectedItemId" class="select" @change="loadDefinitions">
                <option value="">スキルを選択</option>
                <option v-for="item in filteredItems" :key="item.id" :value="item.id">
                  {{ item.name }}
                </option>
              </select>
            </div>
          </div>
        </div>
      </section>

      <!-- レベル定義編集 -->
      <section v-if="selectedItemId" class="section">
        <div class="card">
          <h2 class="section-title">レベル定義編集: {{ selectedItemName }}</h2>

          <div v-if="defStore.error" class="error-box" role="alert">{{ defStore.error }}</div>
          <div v-if="defStore.loading" class="loading">読み込み中…</div>

          <SkillLevelDefForm
            v-else
            :definitions="currentDefs"
            :saving="saving"
            :save-status="saveStatus"
            @save="handleSave"
          />
        </div>
      </section>

      <div v-else class="empty-note">上記からスキルアイテムを選択してください。</div>
    </template>
  </div>
</template>

<script setup lang="ts">
import type { SkillLevelDefinitions } from '~/types/skill'

const auth = useAuthStore()
const masterStore = useSkillMasterStore()
const defStore = useSkillLevelDefinitionStore()
const config = useRuntimeConfig()

const isAdmin = computed(() => auth.currentUser?.role === 'Admin')

const selectedCategoryId = ref('')
const selectedItemId = ref('')
const saving = ref(false)
const saveStatus = ref<'idle' | 'saved' | 'error'>('idle')

const filteredItems = computed(() =>
  masterStore.itemsByCategory[selectedCategoryId.value] ?? [],
)

const selectedItemName = computed(() => {
  for (const items of Object.values(masterStore.itemsByCategory)) {
    const item = items.find(i => i.id === selectedItemId.value)
    if (item) return item.name
  }
  return ''
})

const currentDefs = computed<SkillLevelDefinitions | null>(() =>
  defStore.getDefinitions(selectedItemId.value),
)

watch(selectedCategoryId, async (id) => {
  if (id && !masterStore.itemsByCategory[id]) {
    await masterStore.fetchItems(id)
  }
})

onMounted(async () => {
  await masterStore.fetchCategories()
})

async function loadDefinitions() {
  if (!selectedItemId.value) return
  await defStore.fetchDefinitions(selectedItemId.value)
}

async function handleSave(payload: Array<{ level: number; description: string; examples: string | null }>) {
  saving.value = true
  saveStatus.value = 'idle'
  try {
    await $fetch(
      `${config.public.apiBaseUrl}/api/skill-masters/${selectedItemId.value}/level-definitions`,
      {
        method: 'PUT',
        body: { levelDefinitions: payload },
        headers: { 'X-User-Id': auth.currentUser?.id ?? '' },
      },
    )
    saveStatus.value = 'saved'
    await defStore.fetchDefinitions(selectedItemId.value, true)
    setTimeout(() => { saveStatus.value = 'idle' }, 3000)
  }
  catch {
    saveStatus.value = 'error'
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

.form-row { display: flex; gap: 1rem; flex-wrap: wrap; }
.form-group { display: flex; flex-direction: column; gap: 0.3rem; flex: 1; min-width: 200px; }

.empty-note { color: var(--text-muted); font-size: 0.9rem; padding: 1rem 0; text-align: center; }
</style>
