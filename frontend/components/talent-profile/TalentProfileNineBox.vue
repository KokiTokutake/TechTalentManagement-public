<template>
  <section class="card" data-testid="talent-profile-nine-box-section">
    <h2 class="section-title">9ボックス評価</h2>
    <div v-if="nineBox" class="nine-box-content">
      <div class="cell-badge" :style="{ background: nineBox.cellColor }">
        {{ nineBox.cellLabel }}
      </div>
      <dl class="nine-box-detail">
        <div class="detail-row">
          <dt>業績</dt>
          <dd>{{ nineBox.performanceLevel }}</dd>
        </div>
        <div class="detail-row">
          <dt>ポテンシャル</dt>
          <dd>{{ nineBox.potentialLevel }}</dd>
        </div>
        <div class="detail-row">
          <dt>評価日</dt>
          <dd>{{ formatDate(nineBox.assessedAt) }}</dd>
        </div>
      </dl>
    </div>
    <p v-else class="empty-note" data-testid="talent-profile-nine-box-empty">
      未評価
    </p>
  </section>
</template>

<script setup lang="ts">
import type { NineBoxSummary } from '~/types/talentProfile'

defineProps<{ nineBox: NineBoxSummary | null }>()

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('ja-JP')
}
</script>

<style scoped>
.nine-box-content { display: flex; flex-direction: column; gap: 1rem; }
.cell-badge { display: inline-block; color: #fff; font-weight: 700; padding: 0.5rem 1.25rem; border-radius: 0.5rem; font-size: 1rem; }
.nine-box-detail { display: flex; flex-direction: column; gap: 0.5rem; margin: 0; }
.detail-row { display: flex; gap: 1rem; }
.detail-row dt { color: #6b7280; width: 8rem; flex-shrink: 0; }
.detail-row dd { font-weight: 500; margin: 0; }
.empty-note { color: #9ca3af; font-size: 0.875rem; }
</style>
