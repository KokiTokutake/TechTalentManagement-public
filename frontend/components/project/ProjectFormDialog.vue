<template>
  <div class="dialog-overlay" @click.self="$emit('close')">
    <div class="dialog">
      <h2 class="dialog-title">{{ isEdit ? 'プロジェクトを編集' : 'プロジェクトを追加' }}</h2>

      <div class="form-group">
        <label class="label">プロジェクト名 <span class="required">*</span></label>
        <input
          v-model="form.projectName"
          class="input"
          placeholder="プロジェクト名"
          maxlength="200"
          data-testid="project-name-input"
          required
        />
      </div>

      <div class="form-group">
        <label class="label">役割 <span class="required">*</span></label>
        <input
          v-model="form.role"
          class="input"
          placeholder="バックエンドエンジニア、テックリード など"
          maxlength="100"
          data-testid="project-role-input"
          required
        />
      </div>

      <div class="form-row">
        <div class="form-group">
          <label class="label">開始日 <span class="required">*</span></label>
          <input
            v-model="form.startDate"
            type="date"
            class="input"
            data-testid="project-start-date-input"
            required
          />
        </div>
        <div class="form-group">
          <label class="label">終了日（空欄＝参画中）</label>
          <input
            v-model="form.endDate"
            type="date"
            class="input"
            data-testid="project-end-date-input"
          />
        </div>
      </div>

      <div class="form-group">
        <label class="label">概要・担当内容</label>
        <textarea
          v-model="form.description"
          class="textarea"
          placeholder="プロジェクト概要・担当した内容"
          maxlength="1000"
          rows="3"
        />
      </div>

      <div class="form-group">
        <label class="label">使用技術</label>
        <select
          v-model="form.skillItemIds"
          multiple
          class="select"
          data-testid="project-skill-select"
        >
          <optgroup
            v-for="cat in masterStore.categories"
            :key="cat.id"
            :label="cat.name"
          >
            <option
              v-for="item in (masterStore.itemsByCategory[cat.id] ?? [])"
              :key="item.id"
              :value="item.id"
            >{{ item.name }}</option>
          </optgroup>
        </select>
        <p class="hint">Ctrl（Mac: Cmd）キーを押しながらクリックで複数選択</p>
      </div>

      <div v-if="errorMsg" class="error-msg">{{ errorMsg }}</div>

      <div class="dialog-footer">
        <button class="btn-secondary" @click="$emit('close')">キャンセル</button>
        <button
          class="btn-primary"
          :disabled="saving"
          data-testid="project-save-button"
          @click="submit"
        >
          {{ saving ? '保存中…' : '保存する' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { ProjectAssignment, CreateProjectAssignmentRequest } from '~/types/projectAssignment'

const props = defineProps<{
  assignment?: ProjectAssignment | null
}>()

const emit = defineEmits<{
  close: []
  save: [payload: CreateProjectAssignmentRequest]
}>()

const masterStore = useSkillMasterStore()
const saving = ref(false)
const errorMsg = ref('')

const isEdit = computed(() => !!props.assignment)

const form = reactive<CreateProjectAssignmentRequest>({
  projectName: props.assignment?.projectName ?? '',
  role: props.assignment?.role ?? '',
  startDate: props.assignment?.startDate ?? '',
  endDate: props.assignment?.endDate ?? null,
  description: props.assignment?.description ?? null,
  skillItemIds: props.assignment?.skills.map(s => s.skillItemId) ?? [],
})

onMounted(async () => {
  if (masterStore.categories.length === 0)
    await masterStore.fetchCategories()
  // 各カテゴリのアイテムを取得
  for (const cat of masterStore.categories) {
    if (!masterStore.itemsByCategory[cat.id])
      masterStore.fetchItems(cat.id)
  }
})

async function submit() {
  errorMsg.value = ''
  if (!form.projectName.trim() || !form.role.trim() || !form.startDate) {
    errorMsg.value = 'プロジェクト名・役割・開始日は必須です。'
    return
  }
  if (form.endDate && form.endDate < form.startDate) {
    errorMsg.value = '終了日は開始日以降を指定してください。'
    return
  }
  saving.value = true
  try {
    emit('save', { ...form, endDate: form.endDate || null, description: form.description || null })
  }
  finally {
    saving.value = false
  }
}
</script>

<style scoped>
.dialog-overlay {
  position: fixed; inset: 0; background: rgba(0,0,0,0.4);
  display: flex; align-items: center; justify-content: center; z-index: 50;
}
.dialog {
  background: #fff; border-radius: 12px; padding: 1.5rem;
  width: min(560px, 95vw); max-height: 90vh; overflow-y: auto;
}
.dialog-title { font-size: 1.125rem; font-weight: 700; margin: 0 0 1rem; }
.form-group { margin-bottom: 0.875rem; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }
.label { display: block; font-size: 0.875rem; font-weight: 500; margin-bottom: 0.25rem; }
.required { color: #dc2626; }
.input { width: 100%; border: 1px solid #d1d5db; border-radius: 6px; padding: 0.5rem 0.75rem; font-size: 0.875rem; box-sizing: border-box; }
.textarea { width: 100%; border: 1px solid #d1d5db; border-radius: 6px; padding: 0.5rem 0.75rem; font-size: 0.875rem; resize: vertical; box-sizing: border-box; }
.select { width: 100%; border: 1px solid #d1d5db; border-radius: 6px; padding: 0.375rem; font-size: 0.875rem; min-height: 120px; }
.hint { font-size: 0.75rem; color: #6b7280; margin-top: 0.25rem; }
.error-msg { color: #dc2626; font-size: 0.875rem; margin-bottom: 0.75rem; }
.dialog-footer { display: flex; justify-content: flex-end; gap: 0.75rem; margin-top: 1rem; }
.btn-primary { background: #4f46e5; color: #fff; border: none; border-radius: 6px; padding: 0.5rem 1.25rem; cursor: pointer; font-size: 0.875rem; }
.btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
.btn-secondary { background: #f3f4f6; border: 1px solid #d1d5db; border-radius: 6px; padding: 0.5rem 1rem; cursor: pointer; font-size: 0.875rem; }
</style>
