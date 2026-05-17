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
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 1.25rem;
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
}

.eval-item.submitted {
  background: #f9fafb;
  opacity: 0.85;
}

.goal-header {
  display: flex;
  align-items: center;
  gap: 0.6rem;
}

.goal-title {
  font-weight: 600;
  font-size: 0.95rem;
  color: #111827;
}

.category-badge {
  font-size: 0.72rem;
  font-weight: 700;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
  flex-shrink: 0;
}

.badge-will { background: #dbeafe; color: #1d4ed8; }
.badge-can  { background: #d1fae5; color: #065f46; }
.badge-must { background: #fee2e2; color: #991b1b; }

.score-row,
.comment-row {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
}

.label {
  font-size: 0.8rem;
  font-weight: 600;
  color: #6b7280;
  width: 4rem;
  flex-shrink: 0;
  padding-top: 0.35rem;
}

.score-buttons {
  display: flex;
  gap: 0.4rem;
}

.score-btn {
  width: 2.25rem;
  height: 2.25rem;
  border-radius: 6px;
  border: 2px solid #e5e7eb;
  background: #f9fafb;
  font-weight: 700;
  font-size: 0.95rem;
  cursor: pointer;
  color: #374151;
  transition: all 0.12s;
}

.score-btn:hover:not(:disabled) {
  border-color: #2563eb;
  background: #eff6ff;
  color: #2563eb;
}

.score-btn.active {
  border-color: #2563eb;
  background: #2563eb;
  color: #fff;
}

.score-btn:disabled {
  cursor: not-allowed;
}

.unscored {
  font-size: 0.8rem;
  color: #ef4444;
  padding-top: 0.45rem;
}

textarea {
  flex: 1;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  padding: 0.5rem 0.75rem;
  font-size: 0.9rem;
  resize: vertical;
  outline: none;
  transition: border-color 0.15s;
  font-family: inherit;
}

textarea:focus {
  border-color: #2563eb;
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}

textarea:disabled {
  background: #f3f4f6;
}
</style>
