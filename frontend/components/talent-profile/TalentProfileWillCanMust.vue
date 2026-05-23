<template>
  <section class="card" data-testid="talent-profile-wcm-section">
    <h2 class="section-title">Will-Can-Must</h2>
    <div v-if="willCanMust" class="wcm-content">
      <dl class="wcm-detail">
        <div class="detail-row">
          <dt>長期目標</dt>
          <dd>{{ willCanMust.longTermGoal }}</dd>
        </div>
        <div class="detail-row">
          <dt>中期目標</dt>
          <dd>{{ willCanMust.mediumTermGoal }}</dd>
        </div>
        <div class="detail-row">
          <dt>現在の注力</dt>
          <dd>{{ willCanMust.currentFocus }}</dd>
        </div>
      </dl>
    </div>
    <p v-else class="empty-note" data-testid="talent-profile-wcm-empty">
      未設定
    </p>
  </section>
</template>

<script setup lang="ts">
import type { WillCanMust } from '~/types/talentProfile'

defineProps<{ willCanMust: WillCanMust | null }>()
</script>

<style scoped>
.wcm-content { display: flex; flex-direction: column; gap: 1rem; }
.wcm-detail { display: flex; flex-direction: column; gap: 0.75rem; margin: 0; }
.detail-row { display: flex; gap: 1rem; }
.detail-row dt { color: #6b7280; width: 8rem; flex-shrink: 0; }
.detail-row dd { font-weight: 500; margin: 0; line-height: 1.5; }
.empty-note { color: #9ca3af; font-size: 0.875rem; }
</style>
