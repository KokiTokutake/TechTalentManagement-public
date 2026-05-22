export default defineNuxtRouteMiddleware((to) => {
  if (to.path === '/select-user') return

  // localStorage は SSR では利用不可のため、クライアント側でのみ認証チェックを行う
  if (import.meta.server) return

  const auth = useAuthStore()
  auth.init()

  if (!auth.isAuthenticated) {
    return navigateTo('/select-user')
  }
})
