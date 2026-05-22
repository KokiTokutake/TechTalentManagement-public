<template>
  <div class="calibration-table-wrap" data-testid="nine-box-calibration-table">
    <!-- 全体サマリー -->
    <div class="overall-summary">
      <div class="overall-stat">
        <span class="stat-label">組織全体 平均パフォーマンス</span>
        <span class="stat-value">{{ formatScore(overall.avgPerformanceScore) }}</span>
      </div>
      <div class="overall-stat">
        <span class="stat-label">組織全体 平均ポテンシャル</span>
        <span class="stat-value">{{ formatScore(overall.avgPotentialScore) }}</span>
      </div>
      <div class="overall-stat">
        <span class="stat-label">スター（High/High）</span>
        <span class="stat-value stat-value--star">{{ overall.distribution.highHigh }} 名</span>
      </div>
      <div class="overall-stat">
        <span class="stat-label">重点管理候補</span>
        <span class="stat-value stat-value--hp">{{ highPotentialTotal }} 名</span>
      </div>
    </div>

    <!-- チーム別テーブル -->
    <div class="table-scroll">
      <table class="calibration-table">
        <thead>
          <tr>
            <th class="col-manager">担当マネージャー</th>
            <th class="col-count">人数</th>
            <th class="col-count">評価済</th>
            <th class="col-score">平均P<br><small>（パフォーマンス）</small></th>
            <th class="col-score">平均P<br><small>（ポテンシャル）</small></th>
            <th class="col-dist" colspan="3">評価分布 — パフォーマンス</th>
          </tr>
          <tr class="sub-header">
            <th colspan="5"></th>
            <th class="dist-high">High</th>
            <th class="dist-mid">Mid</th>
            <th class="dist-low">Low</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="team in teams"
            :key="team.managerId"
            :data-testid="`nine-box-calibration-row-${team.managerId}`"
            class="team-row"
          >
            <td class="col-manager">
              <span class="manager-name">{{ team.managerName }}</span>
              <span v-if="team.assessedCount < team.talentCount" class="incomplete-badge">
                未評価 {{ team.talentCount - team.assessedCount }} 名
              </span>
            </td>
            <td class="col-count center">{{ team.talentCount }}</td>
            <td class="col-count center">
              <span :class="team.assessedCount === team.talentCount ? 'assessed-ok' : 'assessed-partial'">
                {{ team.assessedCount }}
              </span>
            </td>
            <td class="col-score center">
              <span class="score-pill" :style="{ backgroundColor: scoreColor(team.avgPerformanceScore) }">
                {{ formatScore(team.avgPerformanceScore) }}
              </span>
            </td>
            <td class="col-score center">
              <span class="score-pill" :style="{ backgroundColor: scoreColor(team.avgPotentialScore) }">
                {{ formatScore(team.avgPotentialScore) }}
              </span>
            </td>
            <td class="dist-high center">
              {{ highCount(team) }}
            </td>
            <td class="dist-mid center">
              {{ midCount(team) }}
            </td>
            <td class="dist-low center">
              {{ lowCount(team) }}
            </td>
          </tr>
        </tbody>
        <tfoot>
          <tr class="overall-row">
            <td class="col-manager"><strong>合計 / 平均</strong></td>
            <td class="col-count center">{{ totalTalents }}</td>
            <td class="col-count center">{{ totalAssessed }}</td>
            <td class="col-score center">
              <span class="score-pill" :style="{ backgroundColor: scoreColor(overall.avgPerformanceScore) }">
                {{ formatScore(overall.avgPerformanceScore) }}
              </span>
            </td>
            <td class="col-score center">
              <span class="score-pill" :style="{ backgroundColor: scoreColor(overall.avgPotentialScore) }">
                {{ formatScore(overall.avgPotentialScore) }}
              </span>
            </td>
            <td class="dist-high center">{{ overall.distribution.highHigh + overall.distribution.highMid + overall.distribution.highLow }}</td>
            <td class="dist-mid center">{{ overall.distribution.midHigh + overall.distribution.midMid + overall.distribution.midLow }}</td>
            <td class="dist-low center">{{ overall.distribution.lowHigh + overall.distribution.lowMid + overall.distribution.lowLow }}</td>
          </tr>
        </tfoot>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { NineBoxCalibrationTeam, NineBoxCalibrationOverall } from '~/types/nineBox'

const props = defineProps<{
  teams: NineBoxCalibrationTeam[]
  overall: NineBoxCalibrationOverall
}>()

const totalTalents = computed(() => props.teams.reduce((s, t) => s + t.talentCount, 0))
const totalAssessed = computed(() => props.teams.reduce((s, t) => s + t.assessedCount, 0))

const highPotentialTotal = computed(() =>
  props.overall.distribution.highHigh +
  props.overall.distribution.highMid +
  props.overall.distribution.midHigh,
)

function formatScore(score: number): string {
  return score.toFixed(2)
}

function scoreColor(score: number): string {
  if (score >= 2.5) return '#6ee7b7'
  if (score >= 1.5) return '#fde68a'
  return '#fca5a5'
}

function highCount(team: NineBoxCalibrationTeam): number {
  return team.distribution.highHigh + team.distribution.highMid + team.distribution.highLow
}

function midCount(team: NineBoxCalibrationTeam): number {
  return team.distribution.midHigh + team.distribution.midMid + team.distribution.midLow
}

function lowCount(team: NineBoxCalibrationTeam): number {
  return team.distribution.lowHigh + team.distribution.lowMid + team.distribution.lowLow
}
</script>

<style scoped>
.calibration-table-wrap {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.overall-summary {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  padding: 1rem 1.25rem;
  background: var(--surface-muted);
  border-radius: 10px;
}

.overall-stat {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  min-width: 160px;
}

.stat-label {
  font-size: 0.72rem;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.stat-value {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--text);
  font-family: var(--font-mono);
}

.stat-value--star { color: #8b5cf6; }
.stat-value--hp   { color: #f59e0b; }

.table-scroll {
  overflow-x: auto;
}

.calibration-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.85rem;
}

.calibration-table th {
  padding: 0.6rem 0.75rem;
  background: var(--surface-muted);
  color: var(--text-muted);
  font-size: 0.72rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  border-bottom: 2px solid var(--border);
  white-space: nowrap;
}

.sub-header th {
  padding: 0.3rem 0.75rem;
  background: var(--surface-muted);
  font-size: 0.7rem;
  border-bottom: 1px solid var(--border);
}

.calibration-table td {
  padding: 0.65rem 0.75rem;
  border-bottom: 1px solid var(--border);
  color: var(--text-secondary);
}

.team-row:hover td {
  background: var(--surface-muted);
}

.overall-row td {
  background: var(--surface-muted);
  font-weight: 600;
  color: var(--text);
  border-top: 2px solid var(--border);
}

.col-manager { min-width: 180px; }
.col-count   { width: 60px; }
.col-score   { width: 100px; }

.dist-high { color: #6366f1; font-weight: 600; }
.dist-mid  { color: #6b7280; font-weight: 600; }
.dist-low  { color: #ef4444; font-weight: 600; }

.center { text-align: center; }

.manager-name {
  font-weight: 600;
  color: var(--text);
}

.incomplete-badge {
  display: inline-block;
  margin-left: 0.5rem;
  font-size: 0.7rem;
  background: #fef3c7;
  color: #92400e;
  padding: 0.1rem 0.5rem;
  border-radius: 999px;
}

.assessed-ok      { color: #059669; font-weight: 600; }
.assessed-partial { color: #d97706; font-weight: 600; }

.score-pill {
  display: inline-block;
  padding: 0.15rem 0.6rem;
  border-radius: 999px;
  font-size: 0.8rem;
  font-weight: 700;
  font-family: var(--font-mono);
  color: #1f2937;
}
</style>
