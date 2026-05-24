<template>
  <div class="page-wrap">
    <header class="page-header">
      <NuxtLink to="/" class="back-link">← ホームへ戻る</NuxtLink>
      <h1>スキルギャップ比較</h1>
      <p class="subtitle">自己評価と上長評価のギャップを可視化します</p>
    </header>

    <div v-if="!auth.currentPeriod" class="error-box" role="alert">
      考課期間が選択されていません。ホームで期間を選択してください。
    </div>

    <template v-else>
      <!-- ターゲット選択（Manager以上） -->
      <div v-if="auth.isManager" class="section">
        <div class="card">
          <div class="form-row">
            <div class="form-group">
              <label class="label">対象タレント</label>
              <select v-model="selectedTalentId" class="select" @change="loadGap">
                <option value="">（自分のギャップを表示）</option>
                <option v-for="u in talentUsers" :key="u.id" :value="u.id">{{ u.displayName }}</option>
              </select>
            </div>
            <div class="form-group form-group-sm">
              <label class="label">注意閾値</label>
              <input
                v-model.number="localThreshold"
                type="number"
                class="input input-sm"
                min="1"
                max="6"
                @change="loadGap"
              />
            </div>
          </div>
        </div>
      </div>

      <div v-if="gapStore.error" class="error-box" role="alert">{{ gapStore.error }}</div>
      <div v-if="gapStore.loading" class="loading">読み込み中…</div>

      <template v-else-if="gapStore.gapData">
        <!-- スキルギャップセクション -->
        <section class="section">
          <div class="card">
            <h2 class="section-title">スキル評価ギャップ</h2>
            <div v-if="attentionSkills.length" class="section" data-testid="gap-attention-section">
              <h3 class="subsection-title attention">⚠ 要確認スキル（差分 {{ gapStore.threshold }} 以上）</h3>
              <SkillGapChart :items="attentionSkills" :max-level="7" />
            </div>
            <div v-if="normalSkills.length">
              <h3 class="subsection-title">その他のスキル</h3>
              <SkillGapChart :items="normalSkills" :max-level="7" />
            </div>
            <div v-if="!gapStore.gapData.skills.length" class="empty-note">スキルデータがありません。</div>
          </div>
        </section>

        <!-- コンピテンシーギャップセクション -->
        <section class="section">
          <div class="card">
            <h2 class="section-title">コンピテンシーギャップ</h2>
            <div v-if="attentionComps.length" class="section" data-testid="gap-attention-section">
              <h3 class="subsection-title attention">⚠ 要確認コンピテンシー（差分 {{ gapStore.threshold }} 以上）</h3>
              <SkillGapChart :items="attentionComps" :max-level="5" />
            </div>
            <div v-if="normalComps.length">
              <h3 class="subsection-title">その他のコンピテンシー</h3>
              <SkillGapChart :items="normalComps" :max-level="5" />
            </div>
            <div v-if="!gapStore.gapData.competencies.length" class="empty-note">コンピテンシーデータがありません。</div>
          </div>
        </section>
      </template>

      <div v-else-if="!gapStore.loading" class="empty-note">
        対象者を選択してください。
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
interface UserEntry { id: string; displayName: string; role: string }

const auth = useAuthStore()
const gapStore = useSkillGapStore()
const config = useRuntimeConfig()

const selectedTalentId = ref('')
const localThreshold = ref(2)
const talentUsers = ref<UserEntry[]>([])

const targetTalentId = computed(() =>
  selectedTalentId.value || auth.currentUser?.id || '',
)

const attentionSkills = computed(() =>
  (gapStore.gapData?.skills ?? [])
    .filter(s => s.requiresAttention)
    .map(s => ({ id: s.skillItemId, name: s.skillItemName, ...s })),
)

const normalSkills = computed(() =>
  (gapStore.gapData?.skills ?? [])
    .filter(s => !s.requiresAttention)
    .map(s => ({ id: s.skillItemId, name: s.skillItemName, ...s })),
)

const attentionComps = computed(() =>
  (gapStore.gapData?.competencies ?? [])
    .filter(c => c.requiresAttention)
    .map(c => ({ id: c.competencyId, name: c.competencyName, ...c })),
)

const normalComps = computed(() =>
  (gapStore.gapData?.competencies ?? [])
    .filter(c => !c.requiresAttention)
    .map(c => ({ id: c.competencyId, name: c.competencyName, ...c })),
)

onMounted(async () => {
  if (auth.isManager) {
    const users = await $fetch<UserEntry[]>(`${config.public.apiBaseUrl}/api/users`)
    talentUsers.value = users.filter(u => u.role === 'Talent')
  }
  await loadGap()
})

async function loadGap() {
  const pid = auth.currentPeriod?.id
  const rid = auth.currentUser?.id
  const tid = targetTalentId.value
  if (!pid || !rid || !tid) return
  await gapStore.fetchGap(rid, tid, pid, localThreshold.value)
}
</script>

<style scoped>
.page-header { margin-bottom: 1.5rem; }
h1 { font-size: 1.4rem; font-weight: 800; margin: 0 0 0.25rem; color: var(--text); font-family: var(--font-heading); }
.subtitle { font-size: 0.9rem; color: var(--text-muted); margin: 0; }
.back-link { font-size: 0.85rem; color: var(--accent); text-decoration: none; display: block; margin-bottom: 0.5rem; }
.back-link:hover { text-decoration: underline; }

.section { margin-bottom: 1.25rem; }
.section-title { font-family: var(--font-heading); font-size: 1rem; font-weight: 700; margin-bottom: 1rem; }
.subsection-title { font-size: 0.875rem; font-weight: 700; color: var(--text-secondary); margin: 0 0 0.5rem; }
.subsection-title.attention { color: var(--danger-text); }

.form-row { display: flex; gap: 1rem; flex-wrap: wrap; }
.form-group { display: flex; flex-direction: column; gap: 0.3rem; flex: 1; }
.form-group-sm { flex: 0 0 100px; }

.empty-note { color: var(--text-muted); font-size: 0.9rem; padding: 1rem 0; text-align: center; }
</style>
