<template>
  <aside v-if="store.previousMeeting" class="previous-panel" data-testid="previous-meeting-panel">
    <h3 class="panel-title">前回の面談記録</h3>

    <div class="panel-meta">
      <span class="panel-date" data-testid="previous-meeting-date">
        {{ formatDate(store.previousMeeting.conductedAt) }}
      </span>
      <span class="format-badge" :class="`format-${store.previousMeeting.format.toLowerCase()}`">
        {{ formatLabel(store.previousMeeting.format) }}
      </span>
    </div>

    <!-- トピックサマリー -->
    <div class="panel-topics">
      <div v-if="store.previousMeeting.willTopics" class="topic-item topic-item--will">
        <span class="topic-label">Will</span>
        <p class="topic-text">{{ truncate(store.previousMeeting.willTopics) }}</p>
      </div>
      <div v-if="store.previousMeeting.canTopics" class="topic-item topic-item--can">
        <span class="topic-label">Can</span>
        <p class="topic-text">{{ truncate(store.previousMeeting.canTopics) }}</p>
      </div>
      <div v-if="store.previousMeeting.mustTopics" class="topic-item topic-item--must">
        <span class="topic-label">Must</span>
        <p class="topic-text">{{ truncate(store.previousMeeting.mustTopics) }}</p>
      </div>
    </div>

    <!-- 未完了宿題 -->
    <div
      v-if="pendingItems.length > 0"
      class="panel-section"
    >
      <h4 class="panel-section-title">未完了の宿題</h4>
      <ul class="pending-list" data-testid="previous-meeting-pending-items">
        <li
          v-for="item in pendingItems"
          :key="item.id"
          class="pending-item"
          :data-testid="`previous-pending-item-${item.id}`"
        >
          <span class="pending-bullet">•</span>
          <span class="pending-text">{{ item.description }}</span>
        </li>
      </ul>
    </div>

    <NuxtLink
      :to="`/one-on-one/${store.previousMeeting.id}`"
      class="panel-link"
    >
      前回の記録を全て見る →
    </NuxtLink>
  </aside>
</template>

<script setup lang="ts">
import type { ActionItem } from '~/types/oneOnOne'

const props = defineProps<{ talentId: string }>()
const store = useOneOnOneStore()

const pendingItems = computed<ActionItem[]>(() =>
  store.previousMeeting?.actionItems.filter(i => i.status === 'Pending') ?? [],
)

function formatDate(dateStr: string) {
  return dateStr.replace(/-/g, '/')
}

function formatLabel(format: string) {
  const map: Record<string, string> = { InPerson: '対面', Online: 'オンライン', Hybrid: 'ハイブリッド' }
  return map[format] ?? format
}

function truncate(text: string, len = 80) {
  return text.length <= len ? text : text.slice(0, len) + '…'
}

onMounted(async () => {
  if (props.talentId) {
    await store.fetchPreviousMeeting(props.talentId)
  }
})
</script>

<style scoped>
.previous-panel {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 1.25rem;
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
}

.panel-title {
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.07em;
  color: var(--text-muted);
  margin: 0;
}

.panel-meta {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.panel-date {
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--text);
  font-family: var(--font-mono);
}

.format-badge {
  font-size: 0.68rem;
  font-weight: 600;
  padding: 0.1rem 0.4rem;
  border-radius: 999px;
}

.format-inperson { background: #dcfce7; color: #166534; }
.format-online   { background: #dbeafe; color: #1e40af; }
.format-hybrid   { background: #fef9c3; color: #854d0e; }

.panel-topics {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.topic-item {
  padding: 0.5rem 0.6rem;
  border-radius: 6px;
  background: var(--surface-muted);
  border-left: 2px solid var(--border);
}

.topic-item--will { border-left-color: #7c3aed; }
.topic-item--can  { border-left-color: #0891b2; }
.topic-item--must { border-left-color: #059669; }

.topic-label {
  font-size: 0.65rem;
  font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  display: block;
  margin-bottom: 0.15rem;
}

.topic-text {
  font-size: 0.8rem;
  color: var(--text-secondary);
  margin: 0;
  line-height: 1.4;
}

.panel-section {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.panel-section-title {
  font-size: 0.7rem;
  font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin: 0;
}

.pending-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.pending-item {
  display: flex;
  gap: 0.4rem;
  align-items: flex-start;
  font-size: 0.8rem;
}

.pending-bullet {
  color: #ef4444;
  font-weight: 700;
  flex-shrink: 0;
  margin-top: 0.05rem;
}

.pending-text {
  color: var(--text-secondary);
  line-height: 1.4;
}

.panel-link {
  font-size: 0.78rem;
  color: var(--accent);
  text-decoration: none;
  font-weight: 600;
  margin-top: 0.1rem;
}

.panel-link:hover {
  text-decoration: underline;
}
</style>
