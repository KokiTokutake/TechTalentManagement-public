<template>
  <section class="card" data-testid="talent-profile-meetings-section">
    <h2 class="section-title">1on1 最近の記録</h2>
    <div v-if="meetingSummary.length > 0" class="meetings-content">
      <div
        v-for="item in meetingSummary"
        :key="item.id"
        class="meeting-row"
      >
        <span class="meeting-date">{{ formatDate(item.conductedAt) }}</span>
        <p class="meeting-summary">{{ item.notesSummary }}</p>
      </div>
    </div>
    <p v-else class="empty-note" data-testid="talent-profile-meetings-empty">
      記録なし
    </p>
  </section>
</template>

<script setup lang="ts">
import type { MeetingSummaryItem } from '~/types/talentProfile'

defineProps<{ meetingSummary: MeetingSummaryItem[] }>()

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('ja-JP')
}
</script>

<style scoped>
.meetings-content { display: flex; flex-direction: column; gap: 1rem; }
.meeting-row { display: flex; flex-direction: column; gap: 0.25rem; padding: 0.75rem 0; border-bottom: 1px solid #f3f4f6; }
.meeting-row:last-child { border-bottom: none; }
.meeting-date { font-size: 0.75rem; color: #6b7280; font-weight: 500; }
.meeting-summary { margin: 0; font-size: 0.875rem; line-height: 1.6; color: #374151; }
.empty-note { color: #9ca3af; font-size: 0.875rem; }
</style>
