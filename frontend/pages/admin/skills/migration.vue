<template>
  <div class="page-wrap">
    <header class="page-header">
      <NuxtLink to="/" class="back-link">← ホームへ戻る</NuxtLink>
      <h1>スキルスケール移行ツール</h1>
      <p class="subtitle">5段階 → 7段階への移行を実行します（Admin 専用・不可逆操作）</p>
    </header>

    <div v-if="!isAdmin" class="error-box" role="alert">この画面は管理者専用です。</div>

    <template v-else>
      <!-- 警告バナー -->
      <div class="warning-banner" role="alert">
        ⚠ この操作は元に戻せません。プレビューを確認してから実行してください。
      </div>

      <!-- プレビューセクション -->
      <section class="section">
        <div class="card">
          <h2 class="section-title">移行プレビュー</h2>
          <p class="hint-text">
            現在の5段階評価を7段階に変換するプレビューを取得します。<br>
            変換ルール: 1→1, 2→2, 3→3, 4→5, 5→7
          </p>

          <div v-if="previewError" class="error-box" role="alert">{{ previewError }}</div>

          <div v-if="!preview" class="preview-empty">
            <button
              class="btn-secondary"
              :disabled="previewLoading"
              data-testid="migration-preview-button"
              @click="loadPreview"
            >
              {{ previewLoading ? '取得中…' : 'プレビューを取得' }}
            </button>
          </div>

          <template v-else>
            <MigrationPreviewTable
              :affected-count="preview.affectedCount"
              :conversion-rule="preview.conversionRule"
              :preview="preview.preview"
            />

            <div class="action-row">
              <button class="btn-secondary" :disabled="previewLoading" @click="loadPreview">
                再取得
              </button>
              <button
                class="btn-danger"
                :disabled="executing || preview.affectedCount === 0"
                data-testid="migration-execute-button"
                @click="showConfirm = true"
              >
                移行を実行する（{{ preview.affectedCount }}件）
              </button>
            </div>
          </template>
        </div>
      </section>

      <!-- 実行結果 -->
      <section v-if="executeResult" class="section">
        <div class="card result-card">
          <h2 class="section-title">実行完了</h2>
          <div class="result-info">
            <div class="result-item">
              <span class="result-label">変換件数</span>
              <span class="result-val">{{ executeResult.affectedCount }} 件</span>
            </div>
            <div class="result-item">
              <span class="result-label">実行日時</span>
              <span class="result-val">{{ formatDate(executeResult.executedAt) }}</span>
            </div>
            <div class="result-item">
              <span class="result-label">ログID</span>
              <span class="result-val mono">{{ executeResult.migrationLogId }}</span>
            </div>
          </div>
        </div>
      </section>
    </template>

    <!-- 確認ダイアログ -->
    <Teleport to="body">
      <div v-if="showConfirm" class="dialog-overlay" @click.self="showConfirm = false">
        <div class="dialog" role="dialog" aria-modal="true">
          <h3 class="dialog-title">移行実行の確認</h3>
          <p class="dialog-body">
            {{ preview?.affectedCount }} 件のスキルレベルを 5段階 → 7段階へ変換します。<br>
            この操作は元に戻せません。続行しますか？
          </p>
          <div v-if="executeError" class="error-box" role="alert">{{ executeError }}</div>
          <div class="dialog-actions">
            <button class="btn-ghost" :disabled="executing" @click="showConfirm = false">キャンセル</button>
            <button
              class="btn-danger"
              :disabled="executing"
              data-testid="migration-confirm-execute-button"
              @click="handleExecute"
            >
              {{ executing ? '実行中…' : '実行する' }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import type { MigrationPreviewResponse, MigrationExecuteResponse } from '~/types/skill'

const auth = useAuthStore()
const config = useRuntimeConfig()

const isAdmin = computed(() => auth.currentUser?.role === 'Admin')

const preview = ref<MigrationPreviewResponse | null>(null)
const previewLoading = ref(false)
const previewError = ref<string | null>(null)

const showConfirm = ref(false)
const executing = ref(false)
const executeError = ref<string | null>(null)
const executeResult = ref<MigrationExecuteResponse | null>(null)

async function loadPreview() {
  previewLoading.value = true
  previewError.value = null
  try {
    preview.value = await $fetch<MigrationPreviewResponse>(
      `${config.public.apiBaseUrl}/api/admin/skill-migration/preview?requesterId=${auth.currentUser?.id ?? ''}`,
    )
  }
  catch {
    previewError.value = 'プレビューの取得に失敗しました。'
  }
  finally {
    previewLoading.value = false
  }
}

async function handleExecute() {
  executing.value = true
  executeError.value = null
  try {
    executeResult.value = await $fetch<MigrationExecuteResponse>(
      `${config.public.apiBaseUrl}/api/admin/skill-migration/execute?requesterId=${auth.currentUser?.id ?? ''}`,
      { method: 'POST' },
    )
    showConfirm.value = false
    preview.value = null
  }
  catch {
    executeError.value = '移行の実行に失敗しました。既に実行済みの場合は 409 エラーになります。'
  }
  finally {
    executing.value = false
  }
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleString('ja-JP')
}
</script>

<style scoped>
.page-header { margin-bottom: 1.5rem; }
h1 { font-size: 1.4rem; font-weight: 800; margin: 0 0 0.25rem; color: var(--text); font-family: var(--font-heading); }
.subtitle { font-size: 0.9rem; color: var(--text-muted); margin: 0; }
.back-link { font-size: 0.85rem; color: var(--accent); text-decoration: none; display: block; margin-bottom: 0.5rem; }
.back-link:hover { text-decoration: underline; }

.warning-banner {
  background: color-mix(in srgb, #f59e0b 15%, transparent);
  border: 1px solid #f59e0b;
  color: #92400e;
  padding: 0.75rem 1rem;
  border-radius: var(--radius);
  font-size: 0.875rem;
  font-weight: 600;
  margin-bottom: 1.25rem;
}

.section { margin-bottom: 1.25rem; }
.section-title { font-family: var(--font-heading); font-size: 1rem; font-weight: 700; margin-bottom: 0.75rem; }
.hint-text { font-size: 0.85rem; color: var(--text-muted); margin-bottom: 1rem; }

.preview-empty { padding: 1rem 0; }

.action-row {
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid var(--border);
}

.btn-danger {
  padding: 0.55rem 1.1rem;
  background: var(--danger-text, #dc2626);
  color: #fff;
  border: none;
  border-radius: var(--radius);
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
}
.btn-danger:hover:not(:disabled) { opacity: 0.88; }
.btn-danger:disabled { opacity: 0.5; cursor: not-allowed; }

.result-card { border-color: #22c55e; background: color-mix(in srgb, #22c55e 5%, var(--surface)); }
.result-info { display: flex; flex-direction: column; gap: 0.5rem; }
.result-item { display: flex; gap: 0.75rem; align-items: center; }
.result-label { font-size: 0.8rem; color: var(--text-muted); min-width: 80px; }
.result-val { font-size: 0.875rem; font-weight: 700; }
.mono { font-family: monospace; font-size: 0.8rem; }

/* ダイアログ */
.dialog-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
}

.dialog {
  background: var(--surface);
  border-radius: var(--radius-lg, 12px);
  padding: 1.5rem;
  width: min(480px, 90vw);
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.dialog-title { font-size: 1.1rem; font-weight: 800; margin: 0 0 0.75rem; color: var(--danger-text, #dc2626); }
.dialog-body { font-size: 0.9rem; color: var(--text-secondary); line-height: 1.6; margin-bottom: 1rem; }
.dialog-actions { display: flex; gap: 0.75rem; justify-content: flex-end; margin-top: 1rem; }
</style>
