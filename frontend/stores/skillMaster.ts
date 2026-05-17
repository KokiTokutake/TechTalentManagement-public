import { defineStore } from 'pinia'
import type { SkillCategory, SkillItem } from '~/types/skill'

export const useSkillMasterStore = defineStore('skillMaster', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const categories = ref<SkillCategory[]>([])
  const itemsByCategory = ref<Record<string, SkillItem[]>>({})
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchCategories() {
    loading.value = true
    error.value = null
    try {
      const data = await $fetch<SkillCategory[]>(`${base}/api/skill-categories`)
      categories.value = data
    }
    catch {
      error.value = 'カテゴリの取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  async function fetchItems(categoryId: string) {
    try {
      const data = await $fetch<SkillItem[]>(`${base}/api/skill-categories/${categoryId}/items`)
      itemsByCategory.value[categoryId] = data
    }
    catch {
      error.value = 'スキル種別の取得に失敗しました。'
    }
  }

  async function createCategory(requesterId: string, name: string, displayOrder: number) {
    const data = await $fetch<SkillCategory>(`${base}/api/skill-categories?requesterId=${requesterId}`, {
      method: 'POST',
      body: { name, displayOrder },
    })
    categories.value.push(data)
    return data
  }

  async function updateCategory(requesterId: string, categoryId: string, name: string, displayOrder: number) {
    const data = await $fetch<SkillCategory>(`${base}/api/skill-categories/${categoryId}?requesterId=${requesterId}`, {
      method: 'PUT',
      body: { name, displayOrder },
    })
    const idx = categories.value.findIndex(c => c.id === categoryId)
    if (idx !== -1) categories.value[idx] = data
    return data
  }

  async function deleteCategory(requesterId: string, categoryId: string) {
    await $fetch(`${base}/api/skill-categories/${categoryId}?requesterId=${requesterId}`, {
      method: 'DELETE',
    })
    categories.value = categories.value.filter(c => c.id !== categoryId)
    const { [categoryId]: _removed, ...rest } = itemsByCategory.value
    itemsByCategory.value = rest
  }

  async function createItem(requesterId: string, categoryId: string, name: string, displayOrder: number) {
    const data = await $fetch<SkillItem>(`${base}/api/skill-categories/${categoryId}/items?requesterId=${requesterId}`, {
      method: 'POST',
      body: { name, displayOrder },
    })
    if (!itemsByCategory.value[categoryId]) itemsByCategory.value[categoryId] = []
    itemsByCategory.value[categoryId].push(data)
    return data
  }

  async function updateItem(requesterId: string, itemId: string, categoryId: string, name: string, displayOrder: number) {
    const data = await $fetch<SkillItem>(`${base}/api/skill-items/${itemId}?requesterId=${requesterId}`, {
      method: 'PUT',
      body: { name, displayOrder },
    })
    const items = itemsByCategory.value[categoryId]
    if (items) {
      const idx = items.findIndex(i => i.id === itemId)
      if (idx !== -1) items[idx] = data
    }
    return data
  }

  async function deleteItem(requesterId: string, itemId: string, categoryId: string) {
    await $fetch(`${base}/api/skill-items/${itemId}?requesterId=${requesterId}`, {
      method: 'DELETE',
    })
    if (itemsByCategory.value[categoryId]) {
      itemsByCategory.value[categoryId] = itemsByCategory.value[categoryId].filter(i => i.id !== itemId)
    }
  }

  return {
    categories,
    itemsByCategory,
    loading,
    error,
    fetchCategories,
    fetchItems,
    createCategory,
    updateCategory,
    deleteCategory,
    createItem,
    updateItem,
    deleteItem,
  }
})
