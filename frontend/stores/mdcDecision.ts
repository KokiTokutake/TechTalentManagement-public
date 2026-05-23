import { defineStore } from 'pinia'
import { ref } from 'vue'
import type {
  MdcDecisionDto,
  MdcActionItemDto,
  CreateDecisionRequest,
  UpdateDecisionRequest,
  CreateActionItemRequest,
  MdcActionItemStatus,
} from '~/types/mdcDecision'

export const useMdcDecisionStore = defineStore('mdcDecision', () => {
  const decisions = ref<MdcDecisionDto[]>([])
  const actionItems = ref<MdcActionItemDto[]>([])
  const carryoverItems = ref<MdcActionItemDto[]>([])
  const talentDecisions = ref<MdcDecisionDto[]>([])
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)

  const api = useMdcDecisionApi()

  async function fetchDecisions(sessionId: string) {
    loading.value = true
    error.value = null
    try {
      decisions.value = await api.fetchDecisions(sessionId)
    }
    catch {
      error.value = '決定事項の取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  async function createDecision(sessionId: string, data: CreateDecisionRequest) {
    saving.value = true
    error.value = null
    try {
      const created = await api.createDecision(sessionId, data)
      decisions.value = [created, ...decisions.value]
    }
    catch {
      error.value = '決定事項の作成に失敗しました。'
      throw error.value
    }
    finally {
      saving.value = false
    }
  }

  async function updateDecision(id: string, data: UpdateDecisionRequest) {
    saving.value = true
    error.value = null
    try {
      const updated = await api.updateDecision(id, data)
      const idx = decisions.value.findIndex(d => d.id === id)
      if (idx >= 0) decisions.value[idx] = updated
    }
    catch {
      error.value = '決定事項の更新に失敗しました。'
      throw error.value
    }
    finally {
      saving.value = false
    }
  }

  async function fetchActionItems(sessionId: string) {
    try {
      actionItems.value = await api.fetchActionItems(sessionId)
    }
    catch {
      error.value = 'アクションアイテムの取得に失敗しました。'
    }
  }

  async function addActionItem(decisionId: string, data: CreateActionItemRequest) {
    saving.value = true
    try {
      const created = await api.addActionItem(decisionId, data)
      actionItems.value = [...actionItems.value, created]
      const decision = decisions.value.find(d => d.id === decisionId)
      if (decision) {
        decision.actionItems = [...decision.actionItems, created]
      }
    }
    catch {
      error.value = 'アクションアイテムの追加に失敗しました。'
      throw error.value
    }
    finally {
      saving.value = false
    }
  }

  async function updateActionItemStatus(id: string, status: MdcActionItemStatus) {
    try {
      const updated = await api.updateActionItemStatus(id, status)
      const updateInList = (list: MdcActionItemDto[]) => {
        const idx = list.findIndex(a => a.id === id)
        if (idx >= 0) list[idx] = updated
      }
      updateInList(actionItems.value)
      updateInList(carryoverItems.value)
      for (const d of decisions.value) {
        updateInList(d.actionItems)
      }
      carryoverItems.value = carryoverItems.value.filter(
        a => a.status === 'NotStarted' || a.status === 'InProgress',
      )
    }
    catch {
      error.value = 'ステータスの更新に失敗しました。'
      throw error.value
    }
  }

  async function fetchCarryoverItems(prevSessionId: string) {
    try {
      const all = await api.fetchActionItems(prevSessionId)
      carryoverItems.value = all.filter(
        a => a.status === 'NotStarted' || a.status === 'InProgress',
      )
    }
    catch {
      // 前回セッションがない場合は無視
      carryoverItems.value = []
    }
  }

  async function fetchTalentDecisions(talentId: string) {
    loading.value = true
    error.value = null
    try {
      talentDecisions.value = await api.fetchTalentDecisions(talentId)
    }
    catch {
      error.value = 'MDC決定履歴の取得に失敗しました。'
    }
    finally {
      loading.value = false
    }
  }

  function $reset() {
    decisions.value = []
    actionItems.value = []
    carryoverItems.value = []
    talentDecisions.value = []
    loading.value = false
    saving.value = false
    error.value = null
  }

  return {
    decisions,
    actionItems,
    carryoverItems,
    talentDecisions,
    loading,
    saving,
    error,
    fetchDecisions,
    createDecision,
    updateDecision,
    fetchActionItems,
    addActionItem,
    updateActionItemStatus,
    fetchCarryoverItems,
    fetchTalentDecisions,
    $reset,
  }
})
