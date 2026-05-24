<template>
  <div class="page-wrap">
    <header class="page-header">
      <NuxtLink to="/" class="back-link">← ホームへ戻る</NuxtLink>
      <h1>コンピテンシー評価</h1>
      <p class="page-subtitle">{{ pageSubtitle }}</p>
    </header>

    <div v-if="!auth.currentPeriod" class="error-box" role="alert">
      考課期間が選択されていません。ホームで期間を選択してください。
    </div>

    <template v-else>
      <!-- ロール切り替え（Manager以上のみ） -->
      <div v-if="auth.isManager" class="tab-bar">
        <button
          class="tab-btn"
          :class="{ active: mode === 'self' }"
          data-testid="competency-tab"
          @click="switchMode('self')"
        >自己評価（自分）</button>
        <button
          class="tab-btn"
          :class="{ active: mode === 'manage' }"
          @click="switchMode('manage')"
        >上長評価（タレント選択）</button>
      </div>

      <!-- タレント選択（上長モード） -->
      <div v-if="mode === 'manage'" class="section">
        <div class="card">
          <div class="form-group">
            <label class="label">評価対象タレント</label>
            <select v-model="selectedTalentId" class="select" @change="loadData">
              <option value="">選択してください</option>
              <option v-for="u in talentUsers" :key="u.id" :value="u.id">{{ u.displayName }}</option>
            </select>
          </div>
        </div>
      </div>

      <!-- エラー -->
      <div v-if="evalStore.error || masterStore.error" class="error-box" role="alert">
        {{ evalStore.error ?? masterStore.error }}
      </div>

      <!-- 評価フォーム -->
      <section v-if="targetTalentId" class="section">
        <div class="card">
          <h2 class="section-title">コンピテンシー評価</h2>
          <p class="hint-text">各コンピテンシーについて、1（初歩的）〜 5（高度・リード）でレベルを選択してください。</p>

          <div v-if="evalStore.loading || masterStore.loading" class="loading">読み込み中…</div>

          <SkillCompetencyEvalForm
            v-else
            v-model="levelMap"
            :competencies="masterStore.competencies"
            :saving="evalStore.saving"
            :save-status="saveStatus"
            @save="handleSave"
          />
        </div>
      </section>

      <!-- 評価結果サマリ（既存評価がある場合） -->
      <section v-if="targetTalentId && evalStore.evaluationsData?.evaluations.length" class="section">
        <div class="card">
          <h2 class="section-title">現在の評価</h2>
          <div class="result-grid">
            <div
              v-for="item in evalStore.evaluationsData.evaluations"
              :key="item.competencyId"
              class="result-row"
            >
              <span class="result-name">{{ item.competencyName }}</span>
              <div class="result-levels">
                <span class="level-chip self" :class="{ empty: !item.selfEvaluation }">
                  自: {{ item.selfEvaluation?.level ?? '—' }}
                </span>
                <span class="level-chip manager" :class="{ empty: !item.managerEvaluation }">
                  上: {{ item.managerEvaluation?.level ?? '—' }}
                </span>
                <span v-if="item.gap !== null" class="gap-chip" :class="{ attention: Math.abs(item.gap) >= 2 }">
                  Δ{{ item.gap > 0 ? '+' : '' }}{{ item.gap }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
interface UserEntry { id: string; displayName: string; role: string }

const auth = useAuthStore()
const masterStore = useCompetencyMasterStore()
const evalStore = useCompetencyEvaluationStore()
const config = useRuntimeConfig()

const mode = ref<'self' | 'manage'>('self')
const selectedTalentId = ref('')
const talentUsers = ref<UserEntry[]>([])
const levelMap = ref<Record<string, number>>({})
const saveStatus = ref<'idle' | 'saved' | 'error'>('idle')

const targetTalentId = computed(() => {
  if (mode.value === 'self') return auth.currentUser?.id ?? ''
  return selectedTalentId.value
})

const evaluatorType = computed(() => mode.value === 'self' ? 'Self' : 'Manager')

const pageSubtitle = computed(() => {
  if (mode.value === 'manage' && selectedTalentId.value) {
    const u = talentUsers.value.find(u => u.id === selectedTalentId.value)
    return u ? `${u.displayName} さんの評価` : ''
  }
  return `${auth.currentUser?.displayName ?? ''} さんのコンピテンシー`
})

onMounted(async () => {
  await masterStore.fetchCompetencies()
  if (auth.isManager) {
    const users = await $fetch<UserEntry[]>(`${config.public.apiBaseUrl}/api/users`)
    talentUsers.value = users.filter(u => u.role === 'Talent')
  }
  await loadData()
})

async function switchMode(m: 'self' | 'manage') {
  mode.value = m
  selectedTalentId.value = ''
  levelMap.value = {}
  evalStore.evaluationsData = null
  if (m === 'self') await loadData()
}

async function loadData() {
  const tid = targetTalentId.value
  const pid = auth.currentPeriod?.id
  const rid = auth.currentUser?.id
  if (!tid || !pid || !rid) return

  await evalStore.fetchEvaluations(rid, tid, pid)
  syncLevelMap()
}

function syncLevelMap() {
  const evals = evalStore.evaluationsData?.evaluations ?? []
  const map: Record<string, number> = {}
  for (const e of evals) {
    const mine = evaluatorType.value === 'Self' ? e.selfEvaluation : e.managerEvaluation
    if (mine) map[e.competencyId] = mine.level
  }
  levelMap.value = map
}

async function handleSave() {
  const tid = targetTalentId.value
  const pid = auth.currentPeriod?.id
  const rid = auth.currentUser?.id
  if (!tid || !pid || !rid) return

  const evaluations = masterStore.competencies
    .filter(c => levelMap.value[c.id])
    .map(c => ({ competencyId: c.id, level: levelMap.value[c.id], comment: null }))

  try {
    await evalStore.saveEvaluations(rid, {
      talentId: tid,
      periodId: pid,
      evaluatorType: evaluatorType.value,
      evaluations,
    })
    saveStatus.value = 'saved'
    await evalStore.fetchEvaluations(rid, tid, pid)
    setTimeout(() => { saveStatus.value = 'idle' }, 3000)
  }
  catch {
    saveStatus.value = 'error'
  }
}
</script>

<style scoped>
.page-subtitle { color: var(--text-muted); font-size: 0.9rem; }
.back-link { font-size: 0.85rem; color: var(--accent); text-decoration: none; display: block; margin-bottom: 0.5rem; }
.back-link:hover { text-decoration: underline; }

.tab-bar { display: flex; gap: 0; border-bottom: 2px solid var(--border); margin-bottom: 1.25rem; }
.tab-btn {
  padding: 0.55rem 1.1rem;
  border: none; background: none;
  cursor: pointer;
  font-size: 0.875rem; font-weight: 500;
  color: var(--text-muted);
  border-bottom: 2px solid transparent;
  margin-bottom: -2px;
}
.tab-btn.active { color: var(--accent); border-bottom-color: var(--accent); font-weight: 700; }

.section { margin-bottom: 1.5rem; }
.section-title { font-family: var(--font-heading); font-size: 1rem; font-weight: 700; margin-bottom: 0.75rem; }
.hint-text { font-size: 0.82rem; color: var(--text-muted); margin-bottom: 1rem; }

.form-group { display: flex; flex-direction: column; gap: 0.3rem; }

.result-grid { display: flex; flex-direction: column; gap: 0.5rem; }
.result-row { display: flex; align-items: center; justify-content: space-between; gap: 0.75rem; padding: 0.4rem 0; border-bottom: 1px solid var(--border); }
.result-row:last-child { border-bottom: none; }
.result-name { font-size: 0.88rem; font-weight: 500; flex: 1; }
.result-levels { display: flex; gap: 0.4rem; align-items: center; }

.level-chip {
  font-size: 0.75rem; font-weight: 700;
  padding: 0.15rem 0.5rem;
  border-radius: var(--radius-sm);
}
.level-chip.self { background: color-mix(in srgb, var(--accent) 12%, transparent); color: var(--accent); }
.level-chip.manager { background: color-mix(in srgb, #8b5cf6 12%, transparent); color: #7c3aed; }
.level-chip.empty { opacity: 0.4; }

.gap-chip {
  font-size: 0.72rem; font-weight: 700;
  padding: 0.15rem 0.45rem;
  border-radius: var(--radius-sm);
  background: var(--border); color: var(--text-muted);
}
.gap-chip.attention { background: color-mix(in srgb, var(--danger-text) 12%, transparent); color: var(--danger-text); }
</style>
