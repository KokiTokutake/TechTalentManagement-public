<template>
  <span class="tooltip-wrap">
    <button
      type="button"
      class="tooltip-trigger"
      data-testid="skill-level-tooltip-trigger"
      aria-label="レベル定義を表示"
      @click="toggle"
    >?</button>

    <div
      v-if="open"
      class="tooltip-panel"
      data-testid="skill-level-tooltip-content"
      role="tooltip"
    >
      <button class="tooltip-close" type="button" aria-label="閉じる" @click="open = false">✕</button>
      <h4 class="tooltip-title">レベル定義 — {{ skillItemName }}</h4>
      <div v-if="defStore.loading" class="tooltip-loading">読み込み中…</div>
      <table v-else class="level-table">
        <thead>
          <tr>
            <th>Lv</th>
            <th>説明</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="d in definitions"
            :key="d.level"
            :class="{ 'row-current': d.level === currentLevel }"
          >
            <td class="lv-cell">
              <span class="lv-badge" :class="{ 'lv-badge-current': d.level === currentLevel }">
                {{ d.level }}
              </span>
            </td>
            <td>
              <span class="def-text">{{ d.description }}</span>
              <span v-if="d.isFallback" class="fallback-note">（汎用）</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </span>
</template>

<script setup lang="ts">
const props = defineProps<{
  skillItemId: string
  skillItemName: string
  currentLevel?: number
}>()

const defStore = useSkillLevelDefinitionStore()
const open = ref(false)

const definitions = computed(() => defStore.getDefinitions(props.skillItemId)?.levelDefinitions ?? [])

async function toggle() {
  open.value = !open.value
  if (open.value) await defStore.fetchDefinitions(props.skillItemId)
}
</script>

<style scoped>
.tooltip-wrap { position: relative; display: inline-block; }

.tooltip-trigger {
  width: 1.2rem; height: 1.2rem;
  border-radius: 50%;
  border: 1px solid var(--accent);
  background: transparent;
  color: var(--accent);
  font-size: 0.7rem; font-weight: 700;
  cursor: pointer;
  line-height: 1;
  padding: 0;
  vertical-align: middle;
}
.tooltip-trigger:hover { background: var(--accent); color: #fff; }

.tooltip-panel {
  position: absolute;
  top: 1.6rem; left: 0;
  z-index: 100;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  box-shadow: 0 4px 16px rgba(0,0,0,0.15);
  padding: 0.75rem 1rem;
  min-width: 340px;
  max-width: 420px;
}

.tooltip-close {
  position: absolute; top: 0.4rem; right: 0.6rem;
  background: none; border: none; cursor: pointer;
  color: var(--text-muted); font-size: 0.8rem;
}

.tooltip-title {
  font-size: 0.85rem; font-weight: 700;
  color: var(--text); margin-bottom: 0.6rem;
}

.tooltip-loading { font-size: 0.82rem; color: var(--text-muted); }

.level-table { width: 100%; border-collapse: collapse; font-size: 0.82rem; }
.level-table th { text-align: left; color: var(--text-muted); font-weight: 600; padding: 0.2rem 0.4rem; border-bottom: 1px solid var(--border); }
.level-table td { padding: 0.25rem 0.4rem; vertical-align: top; }
.level-table tr.row-current td { background: color-mix(in srgb, var(--accent) 8%, transparent); }

.lv-cell { width: 2rem; text-align: center; }
.lv-badge {
  display: inline-block; width: 1.4rem; height: 1.4rem;
  border-radius: 50%; background: var(--border);
  color: var(--text-muted); font-size: 0.72rem; font-weight: 700;
  text-align: center; line-height: 1.4rem;
}
.lv-badge-current { background: var(--accent); color: #fff; }

.def-text { color: var(--text); }
.fallback-note { color: var(--text-muted); font-size: 0.75rem; margin-left: 0.3rem; }
</style>
