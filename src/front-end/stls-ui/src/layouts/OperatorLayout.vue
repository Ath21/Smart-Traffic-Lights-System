<script setup>
import TopBar from '../components/TopBar.vue'
import MapView from '../components/MapView.vue'
import { ref } from 'vue'
import { useAuth } from '../stores/auth'
import { useRouter } from 'vue-router'

const auth = useAuth()
const router = useRouter()

const current = ref('normal')
const patterns = ['normal','rush_hour','emergency_clear','all_red_test']

function applyPattern() { alert(`Applied pattern: ${current.value} (demo)`) }
function logout() { auth.logout(); router.push('/login') }
</script>

<template>
  <div class="h-screen flex flex-col">
    <TopBar>
      <span class="text-sm text-gray-600 mr-4">
        Operator: {{ auth.user?.username }}
      </span>
      <RouterLink class="px-3 py-1 rounded bg-gray-100" to="/">Viewer</RouterLink>
      <RouterLink class="px-3 py-1 rounded bg-gray-100" to="/app">User</RouterLink>
      <RouterLink class="px-3 py-1 rounded bg-gray-800 text-white" to="/operator">Operator</RouterLink>
      <button class="px-3 py-1 rounded bg-red-500 text-white ml-2" @click="logout">Logout</button>
    </TopBar>
  </div>
</template>