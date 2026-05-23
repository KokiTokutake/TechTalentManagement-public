<template>
  <div class="page-container" data-testid="talent-profile-page">
    <div v-if="store.loading" class="loading-state">
      <p>読み込み中...</p>
    </div>

    <template v-else-if="store.profile">
      <TalentProfileHeader
        :profile="store.profile"
        :periods="periods"
        :selected-period-id="store.selectedPeriodId"
        :can-toggle-key-person="canToggleKeyPerson"
        @period-change="onPeriodChange"
        @toggle-key-person="onToggleKeyPerson"
      />

      <div class="profile-grid">
        <TalentProfileNineBox :nine-box="store.profile.nineBox" />
        <TalentProfileWillCanMust :will-can-must="store.profile.willCanMust" />
        <TalentProfileSkills :skill-summary="store.profile.skillSummary" />
        <TalentProfileMeetings :meeting-summary="store.profile.meetingSummary" />
      </div>

      <TalentProfileManagerComment
        v-if="canEditComment"
        :manager-comment="store.profile.managerComment"
        :saving="store.saving"
        :can-copy-prev="false"
        @save="onSaveComment"
        @copy-prev="onCopyPrev"
      />

      <!-- 価値観・やりがい（SPEC-CAREER-002） -->
      <section v-if="valuesStore.values?.importantThings || valuesStore.values?.motivationSituations || valuesStore.values?.avoidSituations" class="values-section-wrap">
        <h2 class="section-heading">価値観・やりがい</h2>
        <CareerValuesSection
          :model-value="{
            importantThings: valuesStore.values.importantThings ?? null,
            motivationSituations: valuesStore.values.motivationSituations ?? null,
            avoidSituations: valuesStore.values.avoidSituations ?? null,
          }"
          :readonly="true"
          data-testid="talent-profile-values-section"
          @update:model-value="() => {}"
        />
      </section>

      <TalentProfileMdcDecisions
        :decisions="decisionStore.talentDecisions"
        :loading="decisionStore.loading"
      />
    </template>

    <div v-else-if="store.error" class="error-state" data-testid="talent-profile-error">
      <p>{{ store.error }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import TalentProfileHeader from '~/components/talent-profile/TalentProfileHeader.vue'
import TalentProfileNineBox from '~/components/talent-profile/TalentProfileNineBox.vue'
import TalentProfileWillCanMust from '~/components/talent-profile/TalentProfileWillCanMust.vue'
import TalentProfileSkills from '~/components/talent-profile/TalentProfileSkills.vue'
import TalentProfileMeetings from '~/components/talent-profile/TalentProfileMeetings.vue'
import TalentProfileManagerComment from '~/components/talent-profile/TalentProfileManagerComment.vue'
import TalentProfileMdcDecisions from '~/components/talent-profile/TalentProfileMdcDecisions.vue'
import CareerValuesSection from '~/components/career/CareerValuesSection.vue'
import type { UpsertManagerCommentRequest } from '~/types/talentProfile'

const route = useRoute()
const talentId = route.params.talentId as string

const store = useTalentProfileStore()
const auth = useAuthStore()
const decisionStore = useMdcDecisionStore()
const valuesStore = useCareerValuesStore()

const periods = ref<{ id: string; name: string }[]>([])

const canToggleKeyPerson = computed(() =>
  ['Senior', 'Admin', 'Executive'].includes(auth.currentUser?.role ?? ''),
)

const canEditComment = computed(() =>
  ['Manager', 'Senior', 'Admin', 'Executive'].includes(auth.currentUser?.role ?? ''),
)

onMounted(async () => {
  await Promise.all([
    store.fetchProfile(talentId),
    decisionStore.fetchTalentDecisions(talentId),
  ])
  if (store.profile && auth.currentUser) {
    periods.value = [{ id: store.profile.periodId, name: store.profile.periodName }]
    valuesStore.fetchValues(talentId, store.profile.periodId, auth.currentUser.id).catch(() => {})
  }
})

onUnmounted(() => {
  store.$reset()
  decisionStore.$reset()
  valuesStore.$reset()
})

async function onPeriodChange(periodId: string) {
  await store.fetchProfile(talentId, periodId)
}

async function onToggleKeyPerson() {
  if (!store.profile) return
  const next = !store.profile.isKeyPerson
  await store.toggleKeyPerson(talentId, next)
}

async function onSaveComment(data: UpsertManagerCommentRequest) {
  if (!store.profile) return
  await store.saveManagerComment(talentId, store.selectedPeriodId, data)
}

function onCopyPrev() {
  // 前期コピーは将来実装
}
</script>

<style scoped>
.page-container { max-width: 960px; margin: 0 auto; padding: 1.5rem; }
.profile-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; margin-bottom: 1.5rem; }
.values-section-wrap { margin-bottom: 1.5rem; padding: 1.25rem; background: var(--surface); border: 1px solid var(--border); border-radius: 0.5rem; }
.section-heading { font-size: 1rem; font-weight: 600; margin: 0 0 1rem; }
.loading-state, .error-state { text-align: center; padding: 3rem; color: #6b7280; }
@media (max-width: 768px) { .profile-grid { grid-template-columns: 1fr; } }
@media print { .page-container { padding: 0; } }
</style>
