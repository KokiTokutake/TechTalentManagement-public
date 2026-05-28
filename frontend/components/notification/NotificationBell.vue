<template>
  <div class="notif-bell-wrapper" v-click-outside="close">
    <button
      data-testid="notification-bell"
      class="notif-bell-btn"
      :aria-label="`通知 ${store.unreadCount > 0 ? `(未読${store.unreadCount}件)` : ''}`"
      @click="toggle"
    >
      <svg class="notif-bell-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
        <path d="M13.73 21a2 2 0 0 1-3.46 0" />
      </svg>
      <span
        v-if="store.unreadCount > 0"
        data-testid="notification-unread-badge"
        class="notif-badge"
      >
        {{ store.unreadCount > 99 ? '99+' : store.unreadCount }}
      </span>
    </button>

    <Transition name="dropdown">
      <NotificationDropdown v-if="open" @close="close" />
    </Transition>
  </div>
</template>

<script setup lang="ts">
const store = useNotificationStore()
const auth = useAuthStore()
const open = ref(false)

const userId = computed(() => auth.currentUser?.id ?? '')

async function toggle() {
  open.value = !open.value
  if (open.value && userId.value) {
    await store.fetchNotifications(userId.value, userId.value)
  }
}

function close() { open.value = false }

// ポーリング: 1分ごとに未読数を更新
let timer: ReturnType<typeof setInterval> | null = null

onMounted(() => {
  if (userId.value) {
    store.fetchUnreadCount(userId.value, userId.value)
    timer = setInterval(() => {
      if (userId.value) store.fetchUnreadCount(userId.value, userId.value)
    }, 60000)
  }
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
})

// v-click-outside ディレクティブ（ローカル定義）
type ClickOutsideEl = HTMLElement & { _clickOutside?: (e: Event) => void }

const vClickOutside = {
  mounted(el: ClickOutsideEl, binding: { value: () => void }) {
    el._clickOutside = (event: Event) => {
      if (!el.contains(event.target as Node)) binding.value()
    }
    document.addEventListener('click', el._clickOutside)
  },
  unmounted(el: ClickOutsideEl) {
    if (el._clickOutside) document.removeEventListener('click', el._clickOutside)
  },
}
</script>

<style scoped>
.notif-bell-wrapper {
  position: relative;
}

.notif-bell-btn {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2.2rem;
  height: 2.2rem;
  border-radius: 50%;
  background: none;
  border: none;
  cursor: pointer;
  color: var(--nav-muted, #9ca3af);
  transition: color 0.12s, background 0.12s;
}

.notif-bell-btn:hover {
  color: var(--nav-text, #f9fafb);
  background: rgba(255, 255, 255, 0.08);
}

.notif-bell-icon {
  width: 1.2rem;
  height: 1.2rem;
}

.notif-badge {
  position: absolute;
  top: 2px;
  right: 2px;
  min-width: 1rem;
  height: 1rem;
  padding: 0 3px;
  background: var(--accent, #c05621);
  color: #fff;
  font-size: 0.6rem;
  font-weight: 700;
  border-radius: 9999px;
  display: flex;
  align-items: center;
  justify-content: center;
  line-height: 1;
  font-family: var(--font-mono);
}

/* ドロップダウンアニメーション */
.dropdown-enter-active,
.dropdown-leave-active {
  transition: opacity 0.15s, transform 0.15s;
}
.dropdown-enter-from,
.dropdown-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}
</style>
