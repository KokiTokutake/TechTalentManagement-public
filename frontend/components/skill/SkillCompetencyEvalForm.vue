<template>
  <div class="eval-form">
    <div v-if="!competencies.length" class="empty-note">
      コンピテンシーマスタが登録されていません。
    </div>

    <div
      v-for="group in groupedCompetencies"
      :key="group.category"
      class="category-block"
    >
      <h3 class="category-label">{{ categoryLabel(group.category) }}</h3>
      <div class="card items-card">
        <div
          v-for="(comp, idx) in group.items"
          :key="comp.id"
          class="eval-row"
          :class="{ 'eval-row-border': idx > 0 }"
        >
          <div class="comp-info">
            <span class="comp-name">{{ comp.name }}</span>
            <span v-if="comp.description" class="comp-desc">{{ comp.description }}</span>
          </div>
          <div class="level-select-wrap">
            <select
              :data-testid="`competency-level-select-${comp.id}`"
              :value="modelValue[comp.id] ?? 0"
              class="select select-sm"
              @change="onLevelChange(comp.id, +($event.target as HTMLSelectElement).value)"
            >
              <option value="0" disabled>選択</option>
              <option v-for="n in 5" :key="n" :value="n">{{ n }} — {{ compLevelLabel(n) }}</option>
            </select>
          </div>
        </div>
      </div>
    </div>

    <div class="form-footer">
      <span v-if="saveStatus === 'saved'" class="save-ok">保存しました</span>
      <span v-if="saveStatus === 'error'" class="save-ng">保存に失敗しました</span>
      <button
        class="btn-primary"
        data-testid="competency-eval-save-button"
        :disabled="saving"
        @click="$emit('save')"
      >
        {{ saving ? '保存中…' : '評価を保存する' }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { CompetencyMaster, CompetencyCategory } from '~/types/skill'

const props = defineProps<{
  competencies: CompetencyMaster[]
  modelValue: Record<string, number>
  saving: boolean
  saveStatus: 'idle' | 'saved' | 'error'
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', val: Record<string, number>): void
  (e: 'save'): void
}>()

function onLevelChange(id: string, level: number) {
  emit('update:modelValue', { ...props.modelValue, [id]: level })
}

const groupedCompetencies = computed(() => {
  const categories: CompetencyCategory[] = ['Technology', 'Methodology', 'BusinessIndustry', 'Common']
  return categories
    .map(cat => ({ category: cat, items: props.competencies.filter(c => c.category === cat) }))
    .filter(g => g.items.length > 0)
})

function categoryLabel(cat: CompetencyCategory) {
  return {
    Technology: 'テクノロジ',
    Methodology: 'メソドロジ',
    BusinessIndustry: 'ビジネスインダストリ',
    Common: '共通',
  }[cat]
}

function compLevelLabel(n: number) {
  return ['初歩的', '基礎的', '標準的', '応用・指導', '高度・リード'][n - 1] ?? ''
}
</script>

<style scoped>
.eval-form { display: flex; flex-direction: column; gap: 1.25rem; }

.category-block { display: flex; flex-direction: column; gap: 0.4rem; }
.category-label { font-size: 0.72rem; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.06em; }

.items-card { padding: 0; overflow: hidden; }
.eval-row { display: flex; align-items: center; justify-content: space-between; gap: 0.75rem; padding: 0.75rem 1rem; }
.eval-row-border { border-top: 1px solid var(--border); }

.comp-info { display: flex; flex-direction: column; gap: 0.15rem; flex: 1; }
.comp-name { font-weight: 600; color: var(--text); font-size: 0.9rem; }
.comp-desc { font-size: 0.78rem; color: var(--text-muted); }

.level-select-wrap { flex-shrink: 0; }
.select-sm { min-width: 160px; }

.form-footer { display: flex; align-items: center; justify-content: flex-end; gap: 0.75rem; padding-top: 0.5rem; }
.save-ok { font-size: 0.85rem; color: var(--success, #16a34a); }
.save-ng { font-size: 0.85rem; color: var(--danger-text); }
</style>
