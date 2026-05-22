<template>
  <Teleport to="body">
    <div
      v-if="talent"
      class="popover-backdrop"
      @click="$emit('close')"
    >
      <div
        class="popover"
        :style="popoverStyle"
        @click.stop
      >
        <div class="popover__header">
          <div class="popover__avatar" :style="{ background: cellColor }">
            {{ talent.initials }}
          </div>
          <div>
            <p class="popover__name">{{ talent.talentName }}</p>
            <p class="popover__role">{{ roleLabel }}</p>
          </div>
          <button class="popover__close" @click="$emit('close')">✕</button>
        </div>

        <div v-if="talent.isHighPotentialCandidate" class="popover__hp-badge">
          重点管理候補
        </div>

        <div v-if="talent.performanceRationale" class="popover__section">
          <p class="popover__section-label">パフォーマンスコメント</p>
          <p class="popover__section-body">{{ talent.performanceRationale }}</p>
        </div>

        <div v-if="talent.potentialRationale" class="popover__section">
          <p class="popover__section-label">ポテンシャルコメント</p>
          <p class="popover__section-body">{{ talent.potentialRationale }}</p>
        </div>

        <div v-if="!talent.performanceRationale && !talent.potentialRationale" class="popover__no-comment">
          コメントなし
        </div>

        <div v-if="showEditLink" class="popover__actions">
          <NuxtLink
            :to="`/mdc/nine-box/input?talentId=${talent.talentId}&periodId=${periodId}&talentName=${encodeURIComponent(talent.talentName)}`"
            class="popover__edit-btn"
            @click="$emit('close')"
          >
            評価を編集
          </NuxtLink>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import type { NineBoxTalent } from '~/types/nineBox'

const props = defineProps<{
  talent: NineBoxTalent | null
  cellColor: string
  periodId: string
  showEditLink: boolean
  anchorRect?: DOMRect
}>()

defineEmits<{
  close: []
}>()

const roleLabel = computed(() => {
  const map: Record<string, string> = {
    Talent: 'タレント',
    Manager: 'マネージャー',
    Senior: 'シニアマネージャー',
    Executive: '役員',
    Admin: '管理者',
  }
  return map[props.talent?.role ?? ''] ?? props.talent?.role
})

const popoverStyle = computed(() => ({
  maxWidth: '320px',
}))
</script>

<style scoped>
.popover-backdrop {
  position: fixed;
  inset: 0;
  z-index: 100;
  display: flex;
  align-items: center;
  justify-content: center;
}

.popover {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 1.25rem;
  box-shadow: 0 8px 32px rgba(0,0,0,0.15);
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  width: 300px;
}

.popover__header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.popover__avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  color: #fff;
  font-size: 0.8rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.popover__name {
  font-size: 0.95rem;
  font-weight: 700;
  color: var(--text);
  margin: 0;
}

.popover__role {
  font-size: 0.75rem;
  color: var(--text-secondary);
  margin: 0;
}

.popover__close {
  margin-left: auto;
  background: none;
  border: none;
  font-size: 0.875rem;
  color: var(--text-muted);
  cursor: pointer;
  padding: 0.25rem;
  line-height: 1;
  align-self: flex-start;
}

.popover__hp-badge {
  font-size: 0.7rem;
  font-weight: 600;
  background: #fef3c7;
  color: #92400e;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  width: fit-content;
}

.popover__section-label {
  font-size: 0.7rem;
  font-weight: 600;
  color: var(--text-secondary);
  margin: 0 0 0.25rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.popover__section-body {
  font-size: 0.8rem;
  color: var(--text);
  margin: 0;
  line-height: 1.5;
}

.popover__no-comment {
  font-size: 0.8rem;
  color: var(--text-muted);
  font-style: italic;
}

.popover__actions {
  padding-top: 0.25rem;
  border-top: 1px solid var(--border);
}

.popover__edit-btn {
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--accent);
  text-decoration: none;
}

.popover__edit-btn:hover {
  text-decoration: underline;
}
</style>
