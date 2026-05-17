<template>
  <main class="container">
    <header class="page-header">
      <NuxtLink to="/" class="back-link">← ホームへ戻る</NuxtLink>
      <h1>組織スキルマップ</h1>
      <p class="subtitle">承認済みスキルのみ表示されます</p>
    </header>

    <div v-if="error" class="error-box" role="alert">{{ error }}</div>

    <div v-if="loading" class="loading">読み込み中…</div>

    <template v-else-if="skillMap">
      <div v-if="!skillMap.users.length" class="empty-state">
        タレントが登録されていません。
      </div>
      <div v-else-if="!skillMap.categories.length" class="empty-state">
        承認済みスキルがまだありません。
      </div>

      <div v-for="cat in skillMap.categories" :key="cat.categoryId" class="category-section">
        <h2 class="category-title">{{ cat.categoryName }}</h2>
        <div class="table-wrapper">
          <table class="skill-table">
            <thead>
              <tr>
                <th class="skill-col">スキル</th>
                <th v-for="user in skillMap.users" :key="user.id" class="user-col">
                  {{ shortName(user.displayName) }}
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in cat.items" :key="item.skillItemId">
                <td class="skill-col">{{ item.skillItemName }}</td>
                <td
                  v-for="user in skillMap.users"
                  :key="user.id"
                  class="level-cell"
                  :class="levelClass(getLevelForUser(item, user.id))"
                  :title="levelTooltip(getLevelForUser(item, user.id))"
                >
                  <span v-if="getLevelForUser(item, user.id)">
                    {{ getLevelForUser(item, user.id) }}
                  </span>
                  <span v-else class="empty-cell">—</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- 凡例 -->
      <section class="legend">
        <h3>レベル凡例</h3>
        <div class="legend-items">
          <span v-for="(label, idx) in levelLabels" :key="idx" class="legend-item" :class="`lv-${idx + 1}`">
            {{ idx + 1 }} - {{ label }}
          </span>
        </div>
      </section>
    </template>
  </main>
</template>

<script setup lang="ts">
import type { SkillMap, SkillMapItem } from '~/types/skill'

const auth = useAuthStore()
const config = useRuntimeConfig()

const skillMap = ref<SkillMap | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)

const requesterId = computed(() => auth.currentUser?.id ?? '')

const levelLabels = ['知識あり', '学習中', '実務可', '指導可', 'エキスパート']

onMounted(async () => {
  loading.value = true
  error.value = null
  try {
    skillMap.value = await $fetch<SkillMap>(
      `${config.public.apiBaseUrl}/api/skill-map?requesterId=${requesterId.value}`,
    )
  }
  catch (e: unknown) {
    if (e && typeof e === 'object' && 'data' in e) {
      const data = (e as { data: unknown }).data
      if (data && typeof data === 'object' && 'error' in data) {
        error.value = String((data as { error: unknown }).error)
        return
      }
    }
    error.value = 'スキルマップの取得に失敗しました。'
  }
  finally {
    loading.value = false
  }
})

function getLevelForUser(item: SkillMapItem, userId: string): number | null {
  return item.userLevels.find(ul => ul.userId === userId)?.level ?? null
}

function shortName(displayName: string): string {
  return displayName.split(/\s+/)[0] ?? displayName
}

function levelClass(level: number | null): string {
  if (!level) return ''
  return `lv-${level}`
}

function levelTooltip(level: number | null): string {
  if (!level) return 'スキルなし'
  return `Lv.${level} - ${levelLabels[level - 1] ?? ''}`
}
</script>

<style scoped>
.container {
  max-width: 960px;
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
.subtitle { font-size: 0.9rem; color: #9ca3af; margin: 0; }

.error-box {
  background: #fef2f2;
  border: 1px solid #fca5a5;
  color: #991b1b;
  padding: 0.75rem 1rem;
  border-radius: 8px;
  margin-bottom: 1rem;
  font-size: 0.9rem;
}

.loading, .empty-state { color: #9ca3af; font-size: 0.9rem; padding: 2rem 0; text-align: center; }

.category-section { margin-bottom: 2rem; }

.category-title {
  font-size: 1rem;
  font-weight: 700;
  color: #374151;
  margin: 0 0 0.75rem;
  padding-bottom: 0.4rem;
  border-bottom: 2px solid #e5e7eb;
}

.table-wrapper { overflow-x: auto; }

.skill-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.85rem;
}

.skill-table th {
  background: #f9fafb;
  font-weight: 700;
  color: #374151;
  padding: 0.5rem 0.75rem;
  border: 1px solid #e5e7eb;
  white-space: nowrap;
}

.skill-col { text-align: left; min-width: 140px; }
.user-col { text-align: center; min-width: 60px; }

.skill-table td {
  padding: 0.4rem 0.5rem;
  border: 1px solid #e5e7eb;
  text-align: center;
  font-weight: 700;
}

.skill-col.skill-table td,
td.skill-col {
  text-align: left;
  color: #374151;
  font-weight: 400;
  background: #fff;
}

.level-cell { font-size: 0.9rem; font-weight: 700; }

.empty-cell { color: #d1d5db; font-weight: 400; font-size: 0.85rem; }

/* レベル別カラー */
.lv-1 { background: #f0fdf4; color: #166534; }
.lv-2 { background: #ecfdf5; color: #065f46; }
.lv-3 { background: #dbeafe; color: #1d4ed8; }
.lv-4 { background: #e0e7ff; color: #3730a3; }
.lv-5 { background: #fef3c7; color: #92400e; }

/* 凡例 */
.legend {
  margin-top: 2rem;
  padding: 1rem 1.25rem;
  background: #f9fafb;
  border-radius: 8px;
  border: 1px solid #e5e7eb;
}

.legend h3 { font-size: 0.85rem; font-weight: 700; color: #374151; margin: 0 0 0.5rem; }

.legend-items { display: flex; gap: 0.75rem; flex-wrap: wrap; }

.legend-item {
  font-size: 0.8rem;
  font-weight: 600;
  padding: 0.2rem 0.6rem;
  border-radius: 4px;
}
</style>
