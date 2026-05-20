<template>
  <div class="eval-item" :class="{ submitted: disabled }">
    <div class="goal-header">
      <span class="category-badge" :class="`badge-${item.goalCategory.toLowerCase()}`">
        {{ item.goalCategory }}
      </span>
      <span class="goal-title">{{ item.goalTitle }}</span>
    </div>

    <div class="score-row">
      <span class="label">達成度</span>
      <div class="score-buttons">
        <button
          v-for="n in 5"
          :key="n"
          type="button"
          class="score-btn"
          :class="{ active: item.achievementScore === n }"
          :disabled="disabled"
          :aria-label="`達成度 ${n}`"
          @click="!disabled && $emit('update:score', n)"
        >
          {{ n }}
        </button>
      </div>
      <span v-if="item.achievementScore === 0" class="unscored">未入力</span>
    </div>

    <div class="comment-row">
      <label :for="`comment-${item.goalId}`" class="label">コメント</label>
      <textarea
        :id="`comment-${item.goalId}`"
        :value="item.comment"
        :disabled="disabled"
        rows="3"
        maxlength="1000"
        placeholder="達成状況・取り組み内容などを記入（任意）"
        @input="$emit('update:comment', ($event.target as HTMLTextAreaElement).value)"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { EvalFormItem } from '~/stores/selfEvaluation'

defineProps<{
  item: EvalFormItem
  disabled?: boolean
}>()

defineEmits<{
  'update:score': [number]
  'update:comment': [string]
}>()
</script>

<style scoped>
.eval-item {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 1.25rem;
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
}

.eval-item.submitted {
  background: var(--surface-muted);
  opacity: 0.85;
}

.goal-header { display: flex; align-items: center; gap: 0.6rem; }

.goal-title { font-weight: 600; font-size: 0.9rem; color: var(--text); }

.category-badge {
  font-size: 0.72rem;
  font-weight: 700;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
  flex-shrink: 0;
}

.badge-will { background: #dbeafe; color: #1d4ed8; }
.badge-can  { background: var(--success-bg); color: var(--success-text); }
.badge-must { background: var(--danger-bg); color: var(--danger-text); }

.score-row, .comment-row { display: flex; align-items: flex-start; gap: 1rem; }

.label {
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--text-muted);
  width: 4rem;
  flex-shrink: 0;
  padding-top: 0.35rem;
}

.score-buttons { display: flex; gap: 0.4rem; }

.score-btn {
  width: 2.25rem;
  height: 2.25rem;
  border-radius: var(--radius-sm);
  border: 2px solid var(--border);
  background: var(--surface-muted);
  font-weight: 700;
  font-size: 0.9rem;
  font-family: var(--font-mono);
  cursor: pointer;
  color: var(--text-secondary);
}

.score-btn:hover:not(:disabled) {
  border-color: var(--accent);
  background: var(--accent-light);
  color: var(--accent);
}

.score-btn.active {
  border-color: var(--accent);
  background: var(--accent);
  color: #fff;
}

.score-btn:disabled { cursor: not-allowed; }

.unscored { font-size: 0.8rem; color: var(--danger); padding-top: 0.45rem; }

textarea {
  flex: 1;
  border: 1px solid var(--border-strong);
  border-radius: var(--radius-sm);
  padding: 0.5rem 0.75rem;
  font-size: 0.9rem;
  font-family: var(--font-body);
  color: var(--text);
  background: var(--surface);
  resize: vertical;
  outline: none;
}

textarea:focus {
  border-color: var(--accent);
  box-shadow: 0 0 0 3px rgba(192, 86, 33, 0.12);
}

textarea:disabled { background: var(--surface-muted); }
</style>
