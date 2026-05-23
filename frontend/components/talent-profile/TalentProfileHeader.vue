<template>
  <div class="profile-header card">
    <div class="header-main">
      <div class="header-identity">
        <h1 class="talent-name">{{ profile.talentName }}</h1>
        <span class="role-badge">{{ profile.role }}</span>
        <span
          v-if="profile.isKeyPerson"
          class="key-person-badge"
          data-testid="talent-profile-key-person-badge"
        >
          重点管理
        </span>
      </div>

      <div class="header-actions">
        <select
          v-if="periods.length > 1"
          :value="selectedPeriodId"
          class="period-select"
          data-testid="talent-profile-period-select"
          @change="$emit('period-change', ($event.target as HTMLSelectElement).value)"
        >
          <option v-for="p in periods" :key="p.id" :value="p.id">{{ p.name }}</option>
        </select>
        <span v-else class="period-label">{{ profile.periodName }}</span>

        <button
          v-if="canToggleKeyPerson"
          class="btn-secondary"
          data-testid="talent-profile-key-person-toggle-button"
          @click="$emit('toggle-key-person')"
        >
          {{ profile.isKeyPerson ? '重点管理解除' : '重点管理指定' }}
        </button>

        <button
          class="btn-ghost"
          data-testid="talent-profile-pdf-export-button"
          @click="handlePdfExport"
        >
          PDF出力
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { TalentProfileDto } from '~/types/talentProfile'

interface Period { id: string; name: string }

const props = defineProps<{
  profile: TalentProfileDto
  periods: Period[]
  selectedPeriodId: string
  canToggleKeyPerson: boolean
}>()

defineEmits<{
  (e: 'period-change', periodId: string): void
  (e: 'toggle-key-person'): void
}>()

function handlePdfExport() {
  window.print()
}
</script>

<style scoped>
.profile-header { margin-bottom: 1.5rem; }
.header-main { display: flex; justify-content: space-between; align-items: flex-start; flex-wrap: wrap; gap: 1rem; }
.header-identity { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }
.talent-name { font-size: 1.5rem; font-weight: 700; margin: 0; }
.role-badge { background: #e0f2fe; color: #0369a1; font-size: 0.75rem; padding: 0.2rem 0.6rem; border-radius: 9999px; }
.key-person-badge { background: #fef3c7; color: #92400e; font-size: 0.75rem; padding: 0.2rem 0.6rem; border-radius: 9999px; font-weight: 600; }
.header-actions { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }
.period-select { border: 1px solid #d1d5db; border-radius: 0.375rem; padding: 0.375rem 0.75rem; font-size: 0.875rem; }
.period-label { font-size: 0.875rem; color: #6b7280; }
@media print { .header-actions { display: none; } }
</style>
