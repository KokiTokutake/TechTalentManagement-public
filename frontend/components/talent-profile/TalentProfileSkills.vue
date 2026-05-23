<template>
  <section class="card" data-testid="talent-profile-skills-section">
    <h2 class="section-title">スキルサマリー</h2>
    <div v-if="skillSummary.length > 0" class="skills-content">
      <div v-for="item in skillSummary" :key="item.skillName" class="skill-row">
        <div class="skill-info">
          <span class="skill-name">{{ item.skillName }}</span>
          <span class="skill-category">{{ item.categoryName }}</span>
        </div>
        <div class="skill-level">
          <span
            v-for="n in 5"
            :key="n"
            class="level-dot"
            :class="{ filled: n <= item.level }"
          />
          <span class="level-label">Lv.{{ item.level }}</span>
        </div>
      </div>
    </div>
    <p v-else class="empty-note" data-testid="talent-profile-skills-empty">
      スキルデータなし
    </p>
  </section>
</template>

<script setup lang="ts">
import type { SkillSummaryItem } from '~/types/talentProfile'

defineProps<{ skillSummary: SkillSummaryItem[] }>()
</script>

<style scoped>
.skills-content { display: flex; flex-direction: column; gap: 0.75rem; }
.skill-row { display: flex; justify-content: space-between; align-items: center; padding: 0.5rem 0; border-bottom: 1px solid #f3f4f6; }
.skill-row:last-child { border-bottom: none; }
.skill-info { display: flex; flex-direction: column; gap: 0.125rem; }
.skill-name { font-weight: 500; font-size: 0.9rem; }
.skill-category { font-size: 0.75rem; color: #6b7280; }
.skill-level { display: flex; align-items: center; gap: 0.375rem; }
.level-dot { display: inline-block; width: 0.625rem; height: 0.625rem; border-radius: 50%; background: #e5e7eb; }
.level-dot.filled { background: #3b82f6; }
.level-label { font-size: 0.75rem; color: #6b7280; margin-left: 0.25rem; }
.empty-note { color: #9ca3af; font-size: 0.875rem; }
</style>
