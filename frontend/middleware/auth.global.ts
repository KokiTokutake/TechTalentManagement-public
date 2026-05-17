export default defineNuxtRouteMiddleware((to) => {
  if (to.path === '/select-user') return

  const auth = useAuthStore()
  auth.init()

  if (!auth.isAuthenticated) {
    return navigateTo('/select-user')
  }
})
