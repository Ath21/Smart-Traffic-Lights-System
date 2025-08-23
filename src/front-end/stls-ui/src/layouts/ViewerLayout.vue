<script setup>
import TopBar from '../components/TopBar.vue'
import MapView from '../components/MapView.vue'
import { useAuth } from '../stores/auth'

const auth = useAuth()
</script>

<template>
  <div class="h-screen flex flex-col">
    <TopBar>
      <template v-if="!auth.isAuthenticated">
        <RouterLink class="px-3 py-1 rounded bg-cyan-500 text-white hover:bg-cyan-600 transition" to="/login">Login</RouterLink>
        <RouterLink class="px-3 py-1 rounded bg-gray-800 text-white hover:bg-gray-700 transition" to="/register">Register</RouterLink>
      </template>
      <template v-else>
        <span class="text-sm text-gray-600 mr-4">Logged in as {{ auth.user?.username }} ({{ auth.user?.role }})</span>
        <RouterLink v-if="auth.user?.role === 'user'" class="px-3 py-1 rounded bg-gray-100" to="/app">User</RouterLink>
        <RouterLink v-if="auth.user?.role === 'operator'" class="px-3 py-1 rounded bg-gray-100" to="/operator">Operator</RouterLink>
        <RouterLink v-if="auth.user?.role === 'admin'" class="px-3 py-1 rounded bg-gray-100" to="/admin">Admin</RouterLink>
      </template>
    </TopBar>
    <MapView />
  </div>
</template>
