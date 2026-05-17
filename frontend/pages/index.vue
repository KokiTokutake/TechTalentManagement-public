<template>
  <main style="font-family: sans-serif; padding: 2rem;">
    <h1>TeckTalentManagement</h1>
    <p>ITエンジニア向けタレントマネジメントシステム — 開発中</p>
    <p>バックエンドステータス: <strong>{{ apiStatus }}</strong></p>
  </main>
</template>

<script setup lang="ts">
const config = useRuntimeConfig()

const { data, error } = await useFetch<{ status: string }>(
  `${config.public.apiBaseUrl}/health`,
  { server: false }
)

const apiStatus = computed(() =>
  error.value ? '接続エラー' : (data.value?.status ?? '確認中…')
)
</script>
