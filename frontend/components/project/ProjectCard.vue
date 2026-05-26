<template>
  <div
    class="project-card"
    :data-testid="`project-row-${assignment.id}`"
  >
    <div class="card-header">
      <div class="project-title-row">
        <h3 class="project-name">{{ assignment.projectName }}</h3>
        <span
          v-if="assignment.isActive"
          class="badge-active"
          data-testid="project-active-badge"
        >参画中</span>
      </div>
      <div class="project-meta">
        <span class="role">{{ assignment.role }}</span>
        <span class="period">
          {{ assignment.startDate }}
          <template v-if="assignment.endDate"> 〜 {{ assignment.endDate }}</template>
          <template v-else> 〜 現在</template>
        </span>
      </div>
    </div>

    <div v-if="assignment.description" class="card-description">
      {{ assignment.description }}
    </div>

    <div v-if="assignment.skills.length > 0" class="skill-tags">
      <span
        v-for="skill in assignment.skills"
        :key="skill.skillItemId"
        class="skill-tag"
      >{{ skill.skillName }}</span>
    </div>

    <div v-if="assignment.managerComment" class="manager-comment">
      <span class="comment-label">上長コメント</span>
      <p>{{ assignment.managerComment }}</p>
    </div>

    <!-- 上長コメント入力（Manager のみ） -->
    <div v-if="canComment" class="manager-comment-form">
      <textarea
        v-model="commentText"
        class="textarea"
        placeholder="業務評価コメントを入力"
        data-testid="project-manager-comment-input"
        rows="3"
      />
      <button
        class="btn-secondary"
        :disabled="savingComment"
        data-testid="project-manager-comment-save-button"
        @click="submitComment"
      >
        {{ savingComment ? '保存中…' : 'コメントを保存' }}
      </button>
    </div>

    <div v-if="canEdit" class="card-actions">
      <button class="btn-icon" data-testid="project-edit-button" @click="$emit('edit', assignment)">編集</button>
      <button
        class="btn-icon btn-danger"
        data-testid="project-delete-button"
        @click="confirmDelete"
      >削除</button>
    </div>

    <!-- 削除確認 -->
    <div v-if="showDeleteConfirm" class="delete-confirm">
      <p>このプロジェクト履歴を削除しますか？</p>
      <button
        class="btn-danger"
        data-testid="project-delete-confirm-button"
        @click="$emit('delete', assignment.id)"
      >削除する</button>
      <button class="btn-secondary" @click="showDeleteConfirm = false">キャンセル</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { ProjectAssignment } from '~/types/projectAssignment'

const props = defineProps<{
  assignment: ProjectAssignment
  canEdit: boolean
  canComment: boolean
}>()

const emit = defineEmits<{
  edit: [assignment: ProjectAssignment]
  delete: [id: string]
  comment: [id: string, comment: string]
}>()

const showDeleteConfirm = ref(false)
const commentText = ref(props.assignment.managerComment ?? '')
const savingComment = ref(false)

function confirmDelete() {
  showDeleteConfirm.value = true
}

async function submitComment() {
  if (!commentText.value.trim()) return
  savingComment.value = true
  try {
    emit('comment', props.assignment.id, commentText.value)
  }
  finally {
    savingComment.value = false
  }
}
</script>

<style scoped>
.project-card {
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 1.25rem;
  margin-bottom: 0.75rem;
}
.card-header { margin-bottom: 0.5rem; }
.project-title-row { display: flex; align-items: center; gap: 0.5rem; }
.project-name { font-size: 1rem; font-weight: 600; margin: 0; }
.badge-active {
  background: #dbeafe;
  color: #1d4ed8;
  font-size: 0.75rem;
  padding: 0.2rem 0.6rem;
  border-radius: 9999px;
  font-weight: 500;
}
.project-meta { display: flex; gap: 1rem; font-size: 0.875rem; color: #6b7280; margin-top: 0.25rem; }
.card-description { font-size: 0.875rem; color: #374151; margin: 0.5rem 0; }
.skill-tags { display: flex; flex-wrap: wrap; gap: 0.375rem; margin: 0.5rem 0; }
.skill-tag {
  background: #f3f4f6;
  color: #374151;
  font-size: 0.75rem;
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
}
.manager-comment {
  background: #f9fafb;
  border-left: 3px solid #6366f1;
  padding: 0.5rem 0.75rem;
  margin: 0.5rem 0;
  font-size: 0.875rem;
}
.comment-label { font-weight: 600; font-size: 0.75rem; color: #6366f1; display: block; margin-bottom: 0.25rem; }
.manager-comment-form { margin-top: 0.75rem; display: flex; flex-direction: column; gap: 0.5rem; }
.textarea { border: 1px solid #d1d5db; border-radius: 6px; padding: 0.5rem; font-size: 0.875rem; resize: vertical; }
.card-actions { display: flex; gap: 0.5rem; margin-top: 0.75rem; justify-content: flex-end; }
.btn-icon { background: none; border: 1px solid #d1d5db; border-radius: 6px; padding: 0.25rem 0.75rem; cursor: pointer; font-size: 0.875rem; }
.btn-danger { color: #dc2626; border-color: #fca5a5; }
.btn-secondary { background: #f3f4f6; border: 1px solid #d1d5db; border-radius: 6px; padding: 0.375rem 0.75rem; cursor: pointer; }
.delete-confirm { background: #fef2f2; border: 1px solid #fca5a5; border-radius: 6px; padding: 0.75rem; margin-top: 0.5rem; }
</style>
