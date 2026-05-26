<template>
  <section class="project-section">
    <h2 class="section-heading">プロジェクト履歴</h2>

    <div v-if="loading" class="loading">読み込み中…</div>

    <div v-else-if="assignments.length === 0" class="empty-state">
      プロジェクト履歴はありません。
    </div>

    <div v-else class="assignments-list">
      <ProjectCard
        v-for="a in assignments"
        :key="a.id"
        :assignment="a"
        :can-edit="false"
        :can-comment="canComment"
        @comment="(id, comment) => $emit('comment', id, comment)"
      />
    </div>
  </section>
</template>

<script setup lang="ts">
import ProjectCard from '~/components/project/ProjectCard.vue'
import type { ProjectAssignment } from '~/types/projectAssignment'

defineProps<{
  assignments: ProjectAssignment[]
  loading: boolean
  canComment: boolean
}>()

defineEmits<{
  comment: [id: string, comment: string]
}>()
</script>

<style scoped>
.project-section { margin-top: 1.5rem; }
.section-heading { font-size: 1.125rem; font-weight: 700; margin-bottom: 0.75rem; }
.loading { color: #6b7280; }
.empty-state { color: #6b7280; font-size: 0.875rem; }
</style>
