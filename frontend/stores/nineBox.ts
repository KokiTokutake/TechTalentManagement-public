import { defineStore } from 'pinia'
import { ref } from 'vue'
import type {
  NineBoxAssessmentDto,
  NineBoxCellDefinition,
  NineBoxMapResponse,
  NineBoxLevel,
  CreateNineBoxAssessmentRequest,
  UpdateNineBoxAssessmentRequest,
} from '~/types/nineBox'

export const useNineBoxStore = defineStore('nineBox', () => {
  const mapData = ref<NineBoxMapResponse | null>(null)
  const cellDefinitions = ref<NineBoxCellDefinition[]>([])
  const currentAssessment = ref<NineBoxAssessmentDto | null>(null)
  const selectedPeriodId = ref<string | null>(null)
  const selectedComparePeriodId = ref<string | null>(null)
  const filters = ref({
    departmentId: null as string | null,
    role: null as string | null,
    searchName: '',
  })
  const loading = ref(false)
  const error = ref<string | null>(null)

  const api = useNineBoxApi()

  async function loadCellDefinitions() {
    if (cellDefinitions.value.length > 0) return
    try {
      cellDefinitions.value = await api.fetchCellDefinitions()
    } catch {
      error.value = 'セル定義の読み込みに失敗しました。'
    }
  }

  async function loadMap(periodId: string) {
    loading.value = true
    error.value = null
    try {
      mapData.value = await api.fetchMap(periodId, {
        departmentId: filters.value.departmentId ?? undefined,
        role: filters.value.role ?? undefined,
        searchName: filters.value.searchName || undefined,
        comparePeriodId: selectedComparePeriodId.value ?? undefined,
      })
      selectedPeriodId.value = periodId
    } catch {
      error.value = '9ボックスマップの読み込みに失敗しました。'
    } finally {
      loading.value = false
    }
  }

  async function loadCurrentAssessment(talentId: string, periodId: string) {
    loading.value = true
    error.value = null
    try {
      currentAssessment.value = await api.fetchByTalentAndPeriod(talentId, periodId)
    } catch {
      currentAssessment.value = null
    } finally {
      loading.value = false
    }
  }

  async function saveAssessment(
    req: CreateNineBoxAssessmentRequest,
    assessorId: string,
  ): Promise<NineBoxAssessmentDto> {
    loading.value = true
    error.value = null
    try {
      const result = await api.createAssessment(req, assessorId)
      currentAssessment.value = result
      return result
    } catch (e: unknown) {
      const err = e as { data?: { error?: string }; statusCode?: number }
      if (err?.statusCode === 409) {
        error.value = 'この期間の評価は既に登録されています。'
      } else {
        error.value = '評価の保存に失敗しました。'
      }
      throw e
    } finally {
      loading.value = false
    }
  }

  async function updateAssessment(
    id: string,
    req: UpdateNineBoxAssessmentRequest,
    assessorId: string,
  ): Promise<NineBoxAssessmentDto> {
    loading.value = true
    error.value = null
    try {
      const result = await api.updateAssessment(id, req, assessorId)
      currentAssessment.value = result
      return result
    } catch {
      error.value = '評価の更新に失敗しました。'
      throw error
    } finally {
      loading.value = false
    }
  }

  function getCellLabel(perf: NineBoxLevel, pot: NineBoxLevel): string {
    const def = cellDefinitions.value.find(
      c => c.performanceLevel === perf && c.potentialLevel === pot,
    )
    return def?.label ?? `${perf}/${pot}`
  }

  function getCellColor(perf: NineBoxLevel, pot: NineBoxLevel): string {
    const def = cellDefinitions.value.find(
      c => c.performanceLevel === perf && c.potentialLevel === pot,
    )
    return def?.color ?? '#e5e7eb'
  }

  return {
    mapData,
    cellDefinitions,
    currentAssessment,
    selectedPeriodId,
    selectedComparePeriodId,
    filters,
    loading,
    error,
    loadCellDefinitions,
    loadMap,
    loadCurrentAssessment,
    saveAssessment,
    updateAssessment,
    getCellLabel,
    getCellColor,
  }
})
