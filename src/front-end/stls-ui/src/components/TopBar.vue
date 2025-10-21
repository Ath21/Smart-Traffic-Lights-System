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
        <template v-if="auth.user.role === 'User'">
          <RouterLink to="/stls" class="btn">Home</RouterLink>
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

        <template v-else-if="auth.user.role === 'Admin'">
          <RouterLink to="/stls" class="btn">Home</RouterLink>
          <RouterLink to="/stls/logs" class="btn-outline">Logs</RouterLink>
          <RouterLink to="/stls/dashboard" class="btn-outline">Admin</RouterLink>
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
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuth } from '../stores/userStore'
import UserMenu from './UserMenu.vue'
import '../assets/topbar.css'

const auth = useAuth()
const router = useRouter()
const notificationCount = ref(0)

function handleLogout() {
  auth.logout()
  router.push('/')
}
</script>
