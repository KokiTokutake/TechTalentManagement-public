export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',
  devtools: { enabled: true },
  modules: ['@pinia/nuxt', '@nuxt/eslint'],
  runtimeConfig: {
    // NUXT_API_BASE_URL 環境変数で上書き可能
    apiBaseUrl: '',
    public: {
      // NUXT_PUBLIC_API_BASE_URL 環境変数で上書き可能
      apiBaseUrl: 'http://localhost:5000'
    }
  },
  typescript: {
    strict: true
  }
})
