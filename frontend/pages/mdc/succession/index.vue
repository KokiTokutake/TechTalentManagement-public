<template>
  <div class="page-wrap-wide">
    <header class="page-header">
      <div class="header-row">
        <div>
          <h1>サクセッションマップ</h1>
          <p class="page-subtitle">重要ポジションの後継者候補をReadiness別に一覧表示する</p>
        </div>
        <NuxtLink to="/mdc/succession/positions" class="btn-ghost">ポジション管理へ</NuxtLink>
      </div>
    </header>

    <div v-if="store.loading" class="loading">読み込み中…</div>
    <div v-else-if="store.error" class="error-box">{{ store.error }}</div>

    <template v-else>
      <div v-if="!store.mapData || store.mapData.positions.length === 0" class="empty-state">
        <p>重要ポジションが登録されていません。</p>
        <NuxtLink to="/mdc/succession/positions" class="btn-primary" style="margin-top:1rem;display:inline-block;">
          ポジションを登録する
        </NuxtLink>
      </div>

      <!-- Succession Map Grid -->
      <div v-else class="map-wrapper">
        <div
          data-testid="succession-map"
          class="succession-map"
          :style="{ gridTemplateColumns: `160px repeat(${store.mapData.positions.length}, minmax(160px, 1fr))` }"
        >
          <!-- Header row -->
          <div class="map-header-cell map-corner">ポジション</div>
          <div
            v-for="pos in store.mapData.positions"
            :key="pos.positionId"
            class="map-header-cell"
          >
            <div class="pos-name">{{ pos.positionName }}</div>
            <span :class="`importance-badge importance-${pos.importance.toLowerCase()}`">
              {{ pos.importance }}
            </span>
            <div v-if="pos.currentHolderName" class="current-holder">
              現担当: {{ pos.currentHolderName }}
            </div>
            <NuxtLink :to="`/mdc/succession/positions/${pos.positionId}`" class="pos-detail-link">
              詳細
            </NuxtLink>
          </div>

          <!-- Ready Now row -->
          <div class="map-label-cell readiness-now">Ready Now</div>
          <div
            v-for="pos in store.mapData.positions"
            :key="`now-${pos.positionId}`"
            :data-testid="`map-cell-${pos.positionId}-ReadyNow`"
            class="map-cell"
            :class="{ 'cell-gap': pos.hasReadyNowGap }"
          >
            <div
              v-if="pos.hasReadyNowGap"
              :data-testid="`map-gap-badge-${pos.positionId}`"
              class="gap-badge"
            >
              後継者未定
            </div>
            <div v-for="c in pos.readyNow" :key="c.candidateId" class="candidate-chip candidate-now">
              <span class="chip-name">{{ c.talentName }}</span>
            </div>
          </div>

          <!-- Ready Soon row -->
          <div class="map-label-cell readiness-soon">Ready Soon</div>
          <div
            v-for="pos in store.mapData.positions"
            :key="`soon-${pos.positionId}`"
            :data-testid="`map-cell-${pos.positionId}-ReadySoon`"
            class="map-cell"
          >
            <div v-for="c in pos.readySoon" :key="c.candidateId" class="candidate-chip candidate-soon">
              <span class="chip-name">{{ c.talentName }}</span>
            </div>
          </div>

          <!-- Ready Mid-Term row -->
          <div class="map-label-cell readiness-mid">Ready Mid-Term</div>
          <div
            v-for="pos in store.mapData.positions"
            :key="`mid-${pos.positionId}`"
            :data-testid="`map-cell-${pos.positionId}-ReadyMidTerm`"
            class="map-cell"
          >
            <div v-for="c in pos.readyMidTerm" :key="c.candidateId" class="candidate-chip candidate-mid">
              <span class="chip-name">{{ c.talentName }}</span>
            </div>
          </div>
        </div>

        <!-- Legend -->
        <div class="map-legend">
          <span class="legend-item">
            <span class="candidate-chip candidate-now legend-chip"></span> Ready Now
          </span>
          <span class="legend-item">
            <span class="candidate-chip candidate-soon legend-chip"></span> Ready Soon
          </span>
          <span class="legend-item">
            <span class="candidate-chip candidate-mid legend-chip"></span> Ready Mid-Term
          </span>
          <span class="legend-item">
            <span class="gap-badge legend-chip">後継者未定</span> Critical ポジションに Ready Now 候補なし
          </span>
        </div>
      </div>

      <!-- Summary cards -->
      <div v-if="store.mapData && store.mapData.positions.length > 0" class="summary-row">
        <div class="summary-card">
          <div class="summary-value">{{ store.mapData.positions.length }}</div>
          <div class="summary-label">重要ポジション</div>
        </div>
        <div class="summary-card">
          <div class="summary-value">{{ totalCandidates }}</div>
          <div class="summary-label">候補者総数</div>
        </div>
        <div class="summary-card summary-card--warn">
          <div class="summary-value">{{ gapCount }}</div>
          <div class="summary-label">後継者未定 (Critical)</div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
const auth = useAuthStore()
const store = useSuccessionStore()

const allowedRoles = ['Senior', 'Executive', 'Admin']
if (!auth.currentUser || !allowedRoles.includes(auth.currentUser.role)) await navigateTo('/')

const totalCandidates = computed(() => {
  if (!store.mapData) return 0
  return store.mapData.positions.reduce(
    (sum, p) => sum + p.readyNow.length + p.readySoon.length + p.readyMidTerm.length,
    0,
  )
})

const gapCount = computed(() => {
  if (!store.mapData) return 0
  return store.mapData.positions.filter(p => p.hasReadyNowGap).length
})

onMounted(async () => {
  if (auth.currentUser) {
    await store.fetchMap(auth.currentUser.id)
  }
})
</script>

<style scoped>
.header-row { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; }

.map-wrapper { overflow-x: auto; }

.succession-map {
  display: grid;
  min-width: max-content;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  overflow: hidden;
  margin-bottom: 1.5rem;
}

.map-corner {
  background: var(--surface-muted);
  font-weight: 700;
  font-size: 0.82rem;
  color: var(--text-muted);
}

.map-header-cell {
  padding: 0.75rem 0.875rem;
  border-right: 1px solid var(--border);
  border-bottom: 1px solid var(--border);
  background: var(--surface-muted);
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}
.map-header-cell:last-child { border-right: none; }

.pos-name { font-weight: 700; font-size: 0.9rem; line-height: 1.3; }
.current-holder { font-size: 0.75rem; color: var(--text-muted); }
.pos-detail-link { font-size: 0.72rem; color: var(--accent); text-decoration: none; margin-top: 0.2rem; }
.pos-detail-link:hover { text-decoration: underline; }

.importance-badge {
  display: inline-block;
  padding: 0.1rem 0.4rem;
  border-radius: var(--radius-sm);
  font-size: 0.68rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.02em;
  width: fit-content;
}
.importance-critical { background: #fee2e2; color: #991b1b; }
.importance-high { background: #fef3c7; color: #92400e; }
.importance-medium { background: #dbeafe; color: #1e40af; }

.map-label-cell {
  padding: 0.75rem 0.875rem;
  border-right: 1px solid var(--border);
  border-bottom: 1px solid var(--border);
  font-weight: 700;
  font-size: 0.8rem;
  display: flex;
  align-items: center;
  white-space: nowrap;
}
.map-label-cell:last-child { border-bottom: none; }
.readiness-now { background: #f0fdf4; color: #166534; }
.readiness-soon { background: #fffbeb; color: #92400e; }
.readiness-mid { background: #eff6ff; color: #1e40af; }

.map-cell {
  padding: 0.625rem 0.75rem;
  border-right: 1px solid var(--border);
  border-bottom: 1px solid var(--border);
  min-height: 64px;
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}
.map-cell:last-child { border-right: none; }
.map-cell.cell-gap { background: #fff5f5; }

.candidate-chip {
  display: inline-flex;
  align-items: center;
  padding: 0.2rem 0.55rem;
  border-radius: 999px;
  font-size: 0.78rem;
  font-weight: 600;
  width: fit-content;
}
.candidate-now { background: #dcfce7; color: #166534; }
.candidate-soon { background: #fef3c7; color: #92400e; }
.candidate-mid { background: #dbeafe; color: #1e40af; }

.chip-name { white-space: nowrap; }

.gap-badge {
  display: inline-block;
  background: #fee2e2;
  color: #991b1b;
  font-size: 0.72rem;
  font-weight: 700;
  padding: 0.2rem 0.5rem;
  border-radius: var(--radius-sm);
  border: 1px solid #fca5a5;
  width: fit-content;
}

.map-legend {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  font-size: 0.78rem;
  color: var(--text-secondary);
  margin-bottom: 1.5rem;
}
.legend-item { display: flex; align-items: center; gap: 0.4rem; }
.legend-chip { font-size: 0.72rem; padding: 0.15rem 0.45rem; }

.summary-row {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
}
.summary-card {
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 1rem 1.25rem;
  background: var(--surface-muted);
  text-align: center;
}
.summary-card--warn { border-color: #fca5a5; background: #fff5f5; }
.summary-value { font-size: 2rem; font-weight: 800; color: var(--text); font-family: var(--font-heading); }
.summary-card--warn .summary-value { color: #991b1b; }
.summary-label { font-size: 0.82rem; color: var(--text-muted); margin-top: 0.25rem; }

.empty-state { text-align: center; padding: 3rem 1rem; color: var(--text-muted); }
</style>
