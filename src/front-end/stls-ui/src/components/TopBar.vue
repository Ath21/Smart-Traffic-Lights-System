<template>
  <header class="topbar">
    <BrandBlock />

    <WelcomeMsg v-if="auth.isAuthenticated" :username="username" />

    <div class="flex items-center gap-3 relative right-nav">
      <AuthLinks
        :is-login-or-register="isLoginOrRegister"
        :is-authenticated="auth.isAuthenticated"
        :home-path="homePath"
      />

      <UserMenu
        v-if="auth.isAuthenticated"
        :username="username"
        :home-path="homePath"
        :notification-count="notificationCount"
        @logout="logout"
        @alert="alertMe"
      />
    </div>
  </header>
</template>

<script setup>
import { computed } from 'vue'
import { useAuth } from '../stores/users'
import { useRoute, useRouter } from 'vue-router'
import { useNotifications } from '../stores/notifications'
import { storeToRefs } from 'pinia'

import BrandBlock from './BrandBlock.vue'
import WelcomeMsg from './WelcomeMsg.vue'
import AuthLinks from './AuthLinks.vue'
import UserMenu from './UserMenu.vue'

import '../assets/topbar.css'

const auth = useAuth()
const router = useRouter()
const route = useRoute()
const notificationsStore = useNotifications()
const { notifications } = storeToRefs(notificationsStore)

const isLoginOrRegister = computed(() =>
  ['/login', '/register', '/reset-password'].includes(route.path)
)


const homePath = computed(() => {
  const role = auth.user?.role?.toLowerCase()
  if (role === 'user') return '/app'
  if (role === 'operator') return '/operator'
  if (role === 'admin') return '/admin'
  return '/'
})

const username = computed(() => {
  if (auth.user?.username) return auth.user.username
  if (auth.user?.email) return auth.user.email.split('@')[0]
  return 'Guest'
})

const notificationCount = computed(() => notifications.value.length)

async function alertMe() {
  try {
    const res = await auth.apiFetch("http://localhost:5055/api/users/send-notification-request", {
      method: "POST",
      body: JSON.stringify({
        Message: "User subscribed to traffic alerts",
        Type: "Traffic"
      })
    })
    if (!res.ok) throw new Error(`HTTP ${res.status}`)
    const data = await res.json()
    alert(`✅ Notification request sent! (${data.type})`)
  } catch (err) {
    console.error("❌ Failed to send alert:", err)
    alert("❌ Failed to send notification")
  }
}


function logout() {
  auth.logout()
  notificationsStore.stopPolling()
  router.push('/login')
}
</script>
