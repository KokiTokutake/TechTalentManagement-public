<template>
  <div class="fm-view-wrap">
    <h3 class="fm-title">フィードバック面談</h3>

    <div v-if="!meeting" class="not-disclosed" data-testid="feedback-not-disclosed-message">
      フィードバック面談の結果はまだ開示されていません。
    </div>
    <template v-else>
      <div class="fm-row">
        <span class="fm-label">面談日</span>
        <span class="fm-value">{{ meeting.conductedAt }}</span>
      </div>
      <div v-if="meeting.overallComment" class="fm-row">
        <span class="fm-label">総合評価</span>
        <p class="fm-value pre">{{ meeting.overallComment }}</p>
      </div>
      <div v-if="meeting.nextPeriodExpectation" class="fm-row">
        <span class="fm-label">次期への期待</span>
        <p class="fm-value pre">{{ meeting.nextPeriodExpectation }}</p>
      </div>
      <div v-if="meeting.talentReactionNote" class="fm-row">
        <span class="fm-label">タレントの反応</span>
        <p class="fm-value pre">{{ meeting.talentReactionNote }}</p>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import type { FeedbackMeetingDto } from '~/types/goalKpi'
defineProps<{ meeting: FeedbackMeetingDto | null }>()
</script>

<style scoped>
.fm-view-wrap {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 1rem;
}
.fm-title { font-size: 0.9rem; font-weight: 700; margin: 0 0 0.75rem; }
.not-disclosed {
  font-size: 0.875rem;
  color: #92400e;
  background: #fef3c7;
  border: 1px solid #fcd34d;
  border-radius: 6px;
  padding: 0.5rem 0.75rem;
}
.fm-row { display: flex; gap: 0.75rem; margin-bottom: 0.5rem; }
.fm-label { min-width: 7rem; font-size: 0.8rem; color: var(--text-muted); font-weight: 600; }
.fm-value { flex: 1; font-size: 0.875rem; color: var(--text); margin: 0; }
.pre { white-space: pre-wrap; }
</style>
