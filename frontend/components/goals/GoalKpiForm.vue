<template>
  <div class="kpi-form-wrap">
    <h3 class="kpi-title">定量KPI設定</h3>

    <div v-if="!editable" class="disabled-message" data-testid="kpi-edit-disabled-message">
      目標設定期間が終了しているため、KPIを編集できません。
    </div>

    <template v-else>
      <div class="form-row">
        <label class="form-label">指標名 <span class="required">*</span></label>
        <input
          v-model="form.metricName"
          class="form-input"
          placeholder="例: 月間件数"
          data-testid="kpi-metric-name-input"
        />
      </div>
      <div class="form-row two-col">
        <div>
          <label class="form-label">目標値 <span class="required">*</span></label>
          <input
            v-model.number="form.targetValue"
            type="number"
            class="form-input"
            placeholder="10"
            data-testid="kpi-target-value-input"
          />
        </div>
        <div>
          <label class="form-label">単位 <span class="required">*</span></label>
          <input
            v-model="form.unit"
            class="form-input"
            placeholder="件"
            data-testid="kpi-unit-input"
          />
        </div>
      </div>
      <div class="form-row">
        <label class="form-label">測定方法</label>
        <input
          v-model="form.measurementMethod"
          class="form-input"
          placeholder="例: 月次集計"
          data-testid="kpi-measurement-method-input"
        />
      </div>
      <div class="form-row">
        <label class="form-label">期限</label>
        <input
          v-model="form.dueDate"
          type="date"
          class="form-input"
          data-testid="kpi-due-date-input"
        />
      </div>
      <p v-if="errorMsg" class="error-text" role="alert">{{ errorMsg }}</p>
      <button
        class="btn-save"
        :disabled="saving || !isValid"
        data-testid="kpi-save-button"
        @click="handleSave"
      >
        {{ saving ? '保存中…' : (hasKpi ? '更新' : '保存') }}
      </button>
    </template>
  </div>
</template>

<script setup lang="ts">
import type { GoalKpiDto } from '~/types/goalKpi'

const props = defineProps<{
  goalId: string
  kpi: GoalKpiDto | null
  editable: boolean
}>()

const emit = defineEmits<{ saved: [] }>()

const store = useGoalKpiStore()

const hasKpi = computed(() => props.kpi !== null)

const form = reactive({
  metricName: props.kpi?.metricName ?? '',
  targetValue: props.kpi?.targetValue ?? 0,
  unit: props.kpi?.unit ?? '',
  measurementMethod: props.kpi?.measurementMethod ?? '',
  dueDate: props.kpi?.dueDate ?? '',
})

watch(() => props.kpi, (kpi) => {
  if (kpi) {
    form.metricName = kpi.metricName
    form.targetValue = kpi.targetValue
    form.unit = kpi.unit
    form.measurementMethod = kpi.measurementMethod ?? ''
    form.dueDate = kpi.dueDate ?? ''
  }
})

const isValid = computed(() =>
  form.metricName.trim() !== '' && form.targetValue > 0 && form.unit.trim() !== ''
)

const saving = ref(false)
const errorMsg = ref<string | null>(null)

async function handleSave() {
  if (!isValid.value) return
  saving.value = true
  errorMsg.value = null
  try {
    await store.saveKpi(props.goalId, {
      metricName: form.metricName,
      targetValue: form.targetValue,
      unit: form.unit,
      measurementMethod: form.measurementMethod || null,
      dueDate: form.dueDate || null,
    })
    emit('saved')
  } catch (e: any) {
    errorMsg.value = e?.data?.error ?? 'KPIの保存に失敗しました。'
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.kpi-form-wrap {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 1rem;
  margin-top: 0.75rem;
}
.kpi-title {
  font-size: 0.9rem;
  font-weight: 700;
  margin: 0 0 0.75rem;
  color: var(--text);
}
.disabled-message {
  font-size: 0.875rem;
  color: #92400e;
  background: #fef3c7;
  border: 1px solid #fcd34d;
  border-radius: 6px;
  padding: 0.5rem 0.75rem;
}
.form-row { margin-bottom: 0.75rem; }
.two-col { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }
.form-label {
  display: block;
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--text-secondary);
  margin-bottom: 0.3rem;
}
.required { color: var(--danger-text); }
.form-input {
  width: 100%;
  padding: 0.4rem 0.6rem;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  font-size: 0.875rem;
  box-sizing: border-box;
}
.error-text { color: var(--danger-text); font-size: 0.8rem; margin: 0.25rem 0; }
.btn-save {
  background: var(--accent);
  color: #fff;
  border: none;
  border-radius: var(--radius-sm);
  padding: 0.4rem 1.2rem;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
}
.btn-save:disabled { opacity: 0.5; cursor: not-allowed; }
</style>
