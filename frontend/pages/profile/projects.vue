<template>
  <div class="page-wrap">
    <header class="page-header">
      <h1>プロジェクト履歴</h1>
      <p class="page-subtitle">{{ auth.currentUser?.displayName }} さんのプロジェクト参画履歴</p>
    </header>

    <div class="page-actions">
      <button
        class="btn-primary"
        data-testid="project-add-button"
        @click="openAddDialog"
      >＋ プロジェクトを追加</button>
    </div>

    <div v-if="store.error" class="error-box" role="alert">{{ store.error }}</div>

    <div v-if="store.loading" class="loading">読み込み中…</div>

    <div v-else-if="store.assignments.length === 0" class="empty-state">
      <p>プロジェクト履歴がまだありません。「プロジェクトを追加」から登録してください。</p>
    </div>

    <div v-else class="assignments-list">
      <ProjectCard
        v-for="a in store.assignments"
        :key="a.id"
        :assignment="a"
        :can-edit="true"
        :can-comment="false"
        @edit="openEditDialog"
        @delete="handleDelete"
        @comment="handleComment"
      />
    </div>

    <ProjectFormDialog
      v-if="showDialog"
      :assignment="editTarget"
      @close="closeDialog"
      @save="handleSave"
    />
  </div>
</template>

<script setup lang="ts">
import ProjectCard from '~/components/project/ProjectCard.vue'
import ProjectFormDialog from '~/components/project/ProjectFormDialog.vue'
import type { ProjectAssignment, CreateProjectAssignmentRequest } from '~/types/projectAssignment'

const auth = useAuthStore()
const store = useProjectStore()

const showDialog = ref(false)
const editTarget = ref<ProjectAssignment | null>(null)

onMounted(async () => {
  if (auth.currentUser?.id)
    await store.fetchAssignments(auth.currentUser.id, auth.currentUser.id)
})

function openAddDialog() {
  editTarget.value = null
  showDialog.value = true
}

function openEditDialog(assignment: ProjectAssignment) {
  editTarget.value = assignment
  showDialog.value = true
}

function closeDialog() {
  showDialog.value = false
  editTarget.value = null
}

async function handleSave(payload: CreateProjectAssignmentRequest) {
  if (!auth.currentUser?.id) return
  try {
    if (editTarget.value) {
      await store.updateAssignment(editTarget.value.id, auth.currentUser.id, payload)
    }
    else {
      await store.createAssignment(auth.currentUser.id, payload)
    }
    closeDialog()
  }
  catch {
    // エラーは store.error に反映される
  }
}

async function handleDelete(id: string) {
  if (!auth.currentUser?.id) return
  await store.deleteAssignment(id, auth.currentUser.id, auth.currentUser.id)
}

async function handleComment(id: string, comment: string) {
  if (!auth.currentUser?.id) return
  await store.saveManagerComment(id, auth.currentUser.id, auth.currentUser.id, { comment })
}
</script>

<style scoped>
.page-wrap { max-width: 800px; margin: 0 auto; padding: 1.5rem 1rem; }
.page-header { margin-bottom: 1rem; }
.page-header h1 { font-size: 1.5rem; font-weight: 700; margin: 0; }
.page-subtitle { color: #6b7280; margin: 0.25rem 0 0; }
.page-actions { margin-bottom: 1rem; }
.btn-primary { background: #4f46e5; color: #fff; border: none; border-radius: 6px; padding: 0.5rem 1.25rem; cursor: pointer; font-size: 0.875rem; }
.error-box { background: #fef2f2; color: #dc2626; border: 1px solid #fca5a5; border-radius: 6px; padding: 0.75rem; margin-bottom: 1rem; }
.loading { color: #6b7280; }
.empty-state { color: #6b7280; font-size: 0.9rem; padding: 2rem; text-align: center; }
.assignments-list { display: flex; flex-direction: column; }
</style>
