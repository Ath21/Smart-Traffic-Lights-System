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
<div class="middle-alerts" v-if="alerts.length > 0">
  <span class="alert-badge">{{ alerts.length }}</span>
</div>


    <!-- Right side: Role-based Buttons / User Menu -->
    <div class="right-nav">
      <!-- Guest -->
      <template v-if="!auth.isAuthenticated">
        <RouterLink to="/" class="btn">Home</RouterLink>
        <RouterLink to="/login" class="btn-outline">Login</RouterLink>
        <RouterLink to="/register" class="btn-primary">Register</RouterLink>
      </template>

      <!-- Authenticated User -->
      <template v-else>
        <!-- User -->
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

        <!-- TrafficOperator -->
        <template v-else-if="auth.user.role === 'TrafficOperator'">
          <RouterLink to="/stls" class="btn">Home</RouterLink>
          <RouterLink to="/stls/analytics" class="btn-outline">Analytics</RouterLink>
          <RouterLink to="/stls/operator" class="btn-outline">Operator</RouterLink>
          <UserMenu
            :username="auth.user.username || auth.user.email"
            :home-path="'/stls'"
            :notification-count="notificationCount"
            :isAuthenticated="auth.isAuthenticated"
            icon="traffic-light"
            @logout="handleLogout"
          />
        </template>

        <!-- Admin -->
        <template v-else-if="auth.user.role === 'Admin'">
          <RouterLink to="/stls" class="btn">Home</RouterLink>
          <RouterLink to="/stls/users" class="btn-outline">Users</RouterLink>
          <RouterLink to="/stls/audits" class="btn-outline">User Audits</RouterLink>
          <RouterLink to="/stls/analytics" class="btn-outline">Analytics</RouterLink>
          <RouterLink to="/stls/operator" class="btn-outline">Operator</RouterLink>
          <RouterLink to="/stls/subscribe" class="btn-outline">Alert Me</RouterLink>
          
          <!-- Admin button opens Portainer -->
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
import { ref, computed, onMounted } from 'vue'  // ✅ make sure computed is imported
import { useRouter } from 'vue-router'
import { useAuth } from '../stores/userStore'
import { useAnalyticsStore } from '../stores/analyticsStore'
import { logoutApi } from '../services/userApi'
import UserMenu from './UserMenu.vue'
import '../assets/topbar.css'

const auth = useAuth()
const router = useRouter()
const notificationCount = ref(0)
const analytics = useAnalyticsStore()

// computed to guarantee alerts is always an array
const alerts = computed(() => analytics.latestAlerts || [])

// fetch alerts on mount
onMounted(() => {
  analytics.fetchAlerts({})
})

function handleLogout() {
  logoutApi()
    .catch(err => console.error('[Logout API] Failed:', err))
    .finally(() => {
      auth.logout()
      router.push('/')
    })
}

// Open Portainer in a new tab
function openPortainer() {
  window.open('https://localhost:9443/#!/auth', '_blank', 'noopener')
}
</script>

