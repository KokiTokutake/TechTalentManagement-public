<template>
  <div class="fm-form-wrap">
    <div class="fm-header">
      <h3 class="fm-title">フィードバック面談記録</h3>
      <FeedbackDiscloseBadge v-if="existing" :is-disclosed="existing.isDisclosed" />
    </div>

    <div class="form-row">
      <label class="form-label">面談日 <span class="required">*</span></label>
      <input
        v-model="form.conductedAt"
        type="date"
        class="form-input"
        data-testid="feedback-conducted-at-input"
      />
    </div>
    <div class="form-row">
      <label class="form-label">総合評価コメント</label>
      <textarea
        v-model="form.overallComment"
        rows="3"
        class="form-textarea"
        placeholder="今期の総合的な評価・フィードバックを記入"
        data-testid="feedback-overall-comment-input"
      />
    </div>
    <div class="form-row">
      <label class="form-label">次期への期待事項</label>
      <textarea
        v-model="form.nextPeriodExpectation"
        rows="2"
        class="form-textarea"
        placeholder="来期に向けての期待・目標"
        data-testid="feedback-next-period-expectation-input"
      />
    </div>
    <div class="form-row">
      <label class="form-label">タレントの反応・コメント</label>
      <textarea
        v-model="form.talentReactionNote"
        rows="2"
        class="form-textarea"
        placeholder="面談時のタレントの反応や発言"
        data-testid="feedback-talent-reaction-note-input"
      />
    </div>

    <p v-if="errorMsg" class="error-text" role="alert">{{ errorMsg }}</p>
    <div class="btn-row">
      <button
        class="btn-save"
        :disabled="saving || !isValid"
        data-testid="feedback-save-button"
        @click="handleSave"
      >
        {{ saving ? '保存中…' : (existing ? '更新' : '保存') }}
      </button>
      <button
        v-if="existing && !existing.isDisclosed"
        class="btn-disclose"
        data-testid="feedback-disclose-button"
        @click="showDialog = true"
      >
        タレントに開示する
      </button>
    </div>

    <div v-if="showDialog" class="dialog-overlay" data-testid="feedback-disclose-dialog">
      <div class="dialog">
        <p class="dialog-text">この面談記録をタレントに開示しますか？<br>開示後は取り消せません。</p>
        <div class="dialog-btns">
          <button class="btn-cancel" @click="showDialog = false">キャンセル</button>
          <button
            class="btn-confirm"
            :disabled="disclosing"
            data-testid="feedback-disclose-confirm-button"
            @click="handleDisclose"
          >
            {{ disclosing ? '開示中…' : '開示する' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { FeedbackMeetingDto, CreateFeedbackMeetingRequest } from '~/types/goalKpi'
import FeedbackDiscloseBadge from '~/components/evaluation/FeedbackDiscloseBadge.vue'

const props = defineProps<{
  selfEvaluationId: string
  talentId: string
  periodId: string
  existing: FeedbackMeetingDto | null
}>()

const emit = defineEmits<{ saved: []; disclosed: [] }>()

const store = useFeedbackMeetingStore()

const today = new Date().toISOString().split('T')[0]
const form = reactive({
  conductedAt: props.existing?.conductedAt ?? today,
  overallComment: props.existing?.overallComment ?? '',
  nextPeriodExpectation: props.existing?.nextPeriodExpectation ?? '',
  talentReactionNote: props.existing?.talentReactionNote ?? '',
})

watch(() => props.existing, (m) => {
  if (m) {
    form.conductedAt = m.conductedAt
    form.overallComment = m.overallComment ?? ''
    form.nextPeriodExpectation = m.nextPeriodExpectation ?? ''
    form.talentReactionNote = m.talentReactionNote ?? ''
  }
})

const isValid = computed(() => form.conductedAt !== '')
const saving = ref(false)
const disclosing = ref(false)
const errorMsg = ref<string | null>(null)
const showDialog = ref(false)

async function handleSave() {
  if (!isValid.value) return
  saving.value = true
  errorMsg.value = null
  try {
    const req: CreateFeedbackMeetingRequest = {
      selfEvaluationId: props.selfEvaluationId,
      talentId: props.talentId,
      conductedAt: form.conductedAt,
      overallComment: form.overallComment || null,
      nextPeriodExpectation: form.nextPeriodExpectation || null,
      talentReactionNote: form.talentReactionNote || null,
    }
    await store.createMeeting(req)
    await store.fetchMeeting(props.talentId, props.periodId)
    emit('saved')
  } catch (e: any) {
    errorMsg.value = e?.data?.error ?? '面談記録の保存に失敗しました。'
  } finally {
    saving.value = false
  }
}

async function handleDisclose() {
  if (!props.existing) return
  disclosing.value = true
  errorMsg.value = null
  try {
    await store.disclose(props.existing.id, props.talentId, props.periodId)
    showDialog.value = false
    emit('disclosed')
  } catch (e: any) {
    errorMsg.value = e?.data?.error ?? '開示に失敗しました。'
    showDialog.value = false
  } finally {
    disclosing.value = false
  }
}
</script>

<style scoped>
.fm-form-wrap {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 1rem;
  position: relative;
}
.fm-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}
.fm-title { font-size: 0.9rem; font-weight: 700; margin: 0; }
.form-row { margin-bottom: 0.75rem; }
.form-label {
  display: block;
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--text-secondary);
  margin-bottom: 0.3rem;
}
.required { color: var(--danger-text); }
.form-input, .form-textarea {
  width: 100%;
  padding: 0.4rem 0.6rem;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  font-size: 0.875rem;
  box-sizing: border-box;
  resize: vertical;
}
.error-text { color: var(--danger-text); font-size: 0.8rem; margin: 0.25rem 0; }
.btn-row { display: flex; gap: 0.5rem; }
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
.btn-disclose {
  background: #059669;
  color: #fff;
  border: none;
  border-radius: var(--radius-sm);
  padding: 0.4rem 1rem;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
}
.dialog-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}
.dialog {
  background: #fff;
  border-radius: var(--radius);
  padding: 1.5rem;
  max-width: 360px;
  width: 90%;
}
.dialog-text { margin: 0 0 1rem; font-size: 0.9rem; line-height: 1.6; }
.dialog-btns { display: flex; justify-content: flex-end; gap: 0.5rem; }
.btn-cancel {
  background: none;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  padding: 0.4rem 1rem;
  cursor: pointer;
}
.btn-confirm {
  background: #059669;
  color: #fff;
  border: none;
  border-radius: var(--radius-sm);
  padding: 0.4rem 1rem;
  font-weight: 600;
  cursor: pointer;
}
.btn-confirm:disabled { opacity: 0.5; cursor: not-allowed; }
</style>
