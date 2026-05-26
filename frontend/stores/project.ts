import { defineStore } from 'pinia'
import { ref } from 'vue'
import type {
  ProjectAssignment,
  CreateProjectAssignmentRequest,
  UpdateProjectAssignmentRequest,
  SaveManagerCommentRequest,
} from '~/types/projectAssignment'

export const useProjectStore = defineStore('project', () => {
  const config = useRuntimeConfig()
  const base = config.public.apiBaseUrl

  const assignments = ref<ProjectAssignment[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchAssignments(talentId: string, requesterId: string) {
    loading.value = true
    error.value = null
    try {
      const res = await $fetch<{ assignments: ProjectAssignment[] }>(
        `${base}/api/project-assignments?talentId=${talentId}`,
        { headers: { 'X-User-Id': requesterId } },
      )
      assignments.value = res.assignments
    }
    catch {
      error.value = 'プロジェクト履歴の取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  async function createAssignment(requesterId: string, payload: CreateProjectAssignmentRequest) {
    const res = await $fetch<{ id: string }>(
      `${base}/api/project-assignments`,
      { method: 'POST', body: payload, headers: { 'X-User-Id': requesterId } },
    )
    await fetchAssignments(requesterId, requesterId)
    return res
  }

  async function updateAssignment(id: string, requesterId: string, payload: UpdateProjectAssignmentRequest) {
    await $fetch<{ id: string }>(
      `${base}/api/project-assignments/${id}`,
      { method: 'PUT', body: payload, headers: { 'X-User-Id': requesterId } },
    )
    await fetchAssignments(requesterId, requesterId)
  }

  async function deleteAssignment(id: string, requesterId: string, talentId: string) {
    await $fetch(
      `${base}/api/project-assignments/${id}`,
      { method: 'DELETE', headers: { 'X-User-Id': requesterId } },
    )
    await fetchAssignments(talentId, requesterId)
  }

  async function saveManagerComment(id: string, requesterId: string, talentId: string, payload: SaveManagerCommentRequest) {
    await $fetch(
      `${base}/api/project-assignments/${id}/manager-comment`,
      { method: 'PUT', body: payload, headers: { 'X-User-Id': requesterId } },
    )
    await fetchAssignments(talentId, requesterId)
  }

  function $reset() {
    assignments.value = []
    loading.value = false
    error.value = null
  }

  return {
    assignments, loading, error,
    fetchAssignments, createAssignment, updateAssignment, deleteAssignment, saveManagerComment, $reset,
  }
})
