<template>
  <header class="topbar">
    <!-- Left side: Logos & Title -->
    <div class="left">
      <img src="/PADA.png" alt="PADA" class="pada-logo" />
      <h1 class="title">
        UNIWA <span class="highlight">STLS</span>
      </h1>
      <img src="/STLS-Logo-TopBar.png" alt="UNIWA STLS" class="stls-logo" />
    </div>

    <!-- Middle Live Alerts -->
    <div v-if="latestAlert" class="middle-alerts">
      <span class="alert-badge">Alert</span>
      <div class="alert-text">
        <strong>{{ latestAlert.Type }}</strong> — 
        {{ latestAlert.Message }} at 
        <strong>{{ latestAlert.Intersection }}</strong>
      </div>
    </div>

    <!-- Right side: Role-based Buttons / User Menu -->
    <div class="right-nav">
      <template v-if="!auth.isAuthenticated">
        <RouterLink to="/" class="btn">Home</RouterLink>
        <RouterLink to="/login" class="btn-outline">Login</RouterLink>
        <RouterLink to="/register" class="btn-primary">Register</RouterLink>
      </template>

      <template v-else>
        <template v-if="auth.user.role === 'User'">
          <RouterLink to="/stls" class="btn">Home</RouterLink>
          <RouterLink to="/stls/analytics" class="btn-outline">Analytics</RouterLink>
          <RouterLink to="/stls/subscribe" class="btn-outline">Alert Me</RouterLink>
          <UserMenu
            :username="auth.user.username || auth.user.email"
            :home-path="'/stls'"
            :notification-count="notificationCount"
            :isAuthenticated="auth.isAuthenticated"
            icon="user"
            @logout="handleLogout"
          />
        </template>

        <template v-else-if="auth.user.role === 'TrafficOperator'">
          <RouterLink to="/stls" class="btn">Home</RouterLink>
          <RouterLink to="/stls/analytics" class="btn-outline">Analytics</RouterLink>
          <UserMenu
            :username="auth.user.username || auth.user.email"
            :home-path="'/stls'"
            :notification-count="notificationCount"
            :isAuthenticated="auth.isAuthenticated"
            icon="traffic-light"
            @logout="handleLogout"
          />
        </template>

        <template v-else-if="auth.user.role === 'Admin'">
          <RouterLink to="/stls" class="btn">Home</RouterLink>
          <RouterLink to="/stls/users" class="btn-outline">Users</RouterLink>
          <RouterLink to="/stls/audits" class="btn-outline">User Audits</RouterLink>
          <RouterLink to="/stls/analytics" class="btn-outline">Analytics</RouterLink>
          <RouterLink to="/stls/subscribe" class="btn-outline">Alert Me</RouterLink>
          <RouterLink
            to="https://localhost:9443/#!/auth"
            class="btn-outline"
            @click.prevent="openPortainer"
          >
            Admin
          </RouterLink>
          <RouterLink to="/stls/logs" class="btn-outline">Logs</RouterLink>
          <UserMenu
            :username="auth.user.username || auth.user.email"
            :home-path="'/stls'"
            :notification-count="notificationCount"
            :isAuthenticated="auth.isAuthenticated"
            icon="admin"
            @logout="handleLogout"
          />
        </template>
      </template>
    </div>
  </header>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '../stores/userStore'
import { useAnalyticsStore } from '../stores/analyticsStore'
import { logoutApi } from '../services/userApi'
import UserMenu from './UserMenu.vue'
import '../assets/topbar.css'

const auth = useUserStore()
const router = useRouter()
const notificationCount = ref(0)
const analytics = useAnalyticsStore()

// Computed alerts from Pinia store
const alerts = computed(() => analytics.latestAlerts || [])

// Get the latest alert dynamically
const latestAlert = computed(() => {
  if (!alerts.value.length) return null
  return [...alerts.value].sort((a, b) => new Date(b.CreatedAt) - new Date(a.CreatedAt))[0]
})

// Fetch alerts on mount + auto-refresh every 60s
onMounted(() => {
  analytics.fetchAlerts({})
  setInterval(() => analytics.fetchAlerts({}), 60000)
})

// Logout handler
function handleLogout() {
  logoutApi()
    .catch(err => console.error('[Logout API] Failed:', err))
    .finally(() => {
      auth.logout()
      router.push('/')
    })
}

// Open Portainer in new tab
function openPortainer() {
  window.open('https://localhost:9443/#!/auth', '_blank', 'noopener')
}
</script>

<style scoped>
.middle-alerts {
  position: absolute;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  align-items: center;
  gap: 0.6rem;
  background: linear-gradient(90deg, #ff4d4f, #ff7875);
  color: #fff;
  padding: 0.4rem 1rem;
  border-radius: 1.5rem;
  font-weight: 600;
  font-size: 0.9rem;
  box-shadow: 0 0 8px rgba(0, 0, 0, 0.25);
  white-space: nowrap;
  overflow: hidden;
  animation: slideIn 0.5s ease, pulse 2s infinite;
}
.alert-badge {
  background: #fff;
  color: #ff4d4f;
  border-radius: 50%;
  padding: 0.2rem 0.5rem;
  font-size: 0.8rem;
  font-weight: bold;
}
.alert-text strong {
  text-decoration: underline;
}
@keyframes pulse {
  0% { opacity: 1; }
  50% { opacity: 0.7; }
  100% { opacity: 1; }
}
@keyframes slideIn {
  from { transform: translate(-50%, -10px); opacity: 0; }
  to { transform: translate(-50%, 0); opacity: 1; }
}
</style>
