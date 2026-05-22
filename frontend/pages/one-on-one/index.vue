<template>
  <div class="page-wrap">
    <header class="page-header">
      <h1>1on1 面談記録</h1>
    </header>

    <div v-if="loading" class="loading-text">読み込み中…</div>

    <div v-else-if="talents.length === 0" class="empty-state">
      配下タレントが見つかりません。
    </div>

    <div v-else class="talent-grid">
      <div
        v-for="talent in talents"
        :key="talent.id"
        class="talent-card"
        :data-testid="`one-on-one-talent-card-${talent.id}`"
      >
        <div class="talent-info">
          <span class="talent-name">{{ talent.displayName }}</span>
          <span class="talent-role">{{ talent.role }}</span>
        </div>
        <NuxtLink
          :to="`/one-on-one/create?talentId=${talent.id}&talentName=${talent.displayName}`"
          class="btn btn--primary btn--sm"
          :data-testid="`one-on-one-create-link-${talent.id}`"
        >
          面談を記録
        </NuxtLink>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
interface TalentUser {
  id: string
  displayName: string
  role: string
}

const auth = useAuthStore()
const config = useRuntimeConfig()
const loading = ref(true)
const talents = ref<TalentUser[]>([])

onMounted(async () => {
  if (!auth.currentUser) {
    await navigateTo('/')
    return
  }
  try {
    const users = await $fetch<TalentUser[]>(`${config.public.apiBaseUrl}/api/users`, {
      headers: { 'X-User-Id': auth.currentUser.id },
    })
    talents.value = users.filter(u => u.role === 'Talent')
  } catch {
    talents.value = []
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.page-header {
  margin-bottom: 1.5rem;
}

h1 {
  font-size: 1.4rem;
  font-weight: 800;
  margin: 0;
  color: var(--text);
  font-family: var(--font-heading);
}

.loading-text {
  color: var(--text-secondary);
  font-size: 0.9rem;
}

.empty-state {
  color: var(--text-muted);
  font-size: 0.9rem;
  padding: 2rem;
  text-align: center;
  border: 1px dashed var(--border);
  border-radius: 10px;
}

.talent-grid {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.talent-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.25rem;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 10px;
}

.talent-info {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}

.talent-name {
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--text);
}

.talent-role {
  font-size: 0.75rem;
  color: var(--text-muted);
}

.btn {
  padding: 0.55rem 1.25rem;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  border: none;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  transition: all 0.15s;
}

.btn--primary {
  background: var(--accent);
  color: #fff;
}

.btn--primary:hover { opacity: 0.88; }

.btn--sm {
  padding: 0.35rem 0.85rem;
  font-size: 0.8rem;
}
</style>
