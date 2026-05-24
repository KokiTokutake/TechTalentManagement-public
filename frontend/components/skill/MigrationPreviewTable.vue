<template>
  <div class="preview-table-wrap" data-testid="migration-preview-wrap">
    <div class="meta-row">
      <div class="meta-item">
        <span class="meta-label">対象件数</span>
        <span class="meta-val">{{ affectedCount }} 件</span>
      </div>
      <div class="meta-item">
        <span class="meta-label">変換ルール</span>
        <span class="meta-val conversion-rule">
          <span v-for="(v, k) in conversionRule" :key="k" class="rule-chip">{{ k }}→{{ v }}</span>
        </span>
      </div>
    </div>

    <div v-if="!preview.length" class="empty-note">
      変換対象データがありません。
    </div>
    <div v-else class="table-wrap">
      <table class="preview-table">
        <thead>
          <tr>
            <th>タレント名</th>
            <th>スキル名</th>
            <th>現在レベル</th>
            <th>変換後</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(row, i) in visibleRows" :key="i">
            <td>{{ row.talentName }}</td>
            <td>{{ row.skillName }}</td>
            <td class="lv-cell current">{{ row.currentLevel }}</td>
            <td class="lv-cell new">{{ row.newLevel }}</td>
          </tr>
        </tbody>
      </table>
      <div v-if="preview.length > showLimit" class="more-hint">
        他 {{ preview.length - showLimit }} 件（上位 {{ showLimit }} 件を表示）
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { MigrationPreviewItem } from '~/types/skill'

const props = defineProps<{
  affectedCount: number
  conversionRule: Record<string, number>
  preview: MigrationPreviewItem[]
}>()

const showLimit = 20

const visibleRows = computed(() => props.preview.slice(0, showLimit))
</script>

<style scoped>
.meta-row {
  display: flex;
  gap: 1.5rem;
  flex-wrap: wrap;
  margin-bottom: 1rem;
  padding: 0.75rem 1rem;
  background: var(--surface-muted);
  border-radius: var(--radius);
  border: 1px solid var(--border);
}

.meta-item { display: flex; flex-direction: column; gap: 0.15rem; }

.meta-label { font-size: 0.75rem; color: var(--text-muted); }

.meta-val { font-size: 0.9rem; font-weight: 700; color: var(--text); }

.conversion-rule { display: flex; gap: 0.35rem; flex-wrap: wrap; }

.rule-chip {
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.15rem 0.5rem;
  border-radius: 3px;
  background: color-mix(in srgb, var(--accent) 12%, transparent);
  color: var(--accent);
}

.table-wrap { overflow-x: auto; }

.preview-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.85rem;
}

.preview-table th {
  background: var(--surface-muted);
  font-weight: 700;
  color: var(--text-secondary);
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--border);
  text-align: left;
}

.preview-table td {
  padding: 0.4rem 0.75rem;
  border: 1px solid var(--border);
}

.lv-cell { text-align: center; font-weight: 700; }

.current { color: var(--text-muted); }

.new { color: var(--accent); }

.more-hint {
  margin-top: 0.5rem;
  font-size: 0.8rem;
  color: var(--text-muted);
  text-align: center;
}

.empty-note { color: var(--text-muted); font-size: 0.9rem; padding: 1rem 0; text-align: center; }
</style>
