<script setup>
import TopBar from '../components/TopBar.vue'
import { useAuth } from '../stores/auth'
import { useRouter } from 'vue-router'

const auth = useAuth()
const router = useRouter()

const logs = [
  { ts: '2025-08-19 12:01', level: 'INFO', msg: 'Operator applied pattern rush_hour' },
  { ts: '2025-08-19 12:05', level: 'WARN', msg: 'Intersection Gate B offline 30s' },
]

function logout() { auth.logout(); router.push('/login') }
</script>

<template>
  <div class="h-screen flex flex-col">
    <TopBar>
      <span class="text-sm text-gray-600 mr-4">
        Admin: {{ auth.user?.username }}
      </span>
      <RouterLink class="px-3 py-1 rounded bg-gray-100" to="/">Viewer</RouterLink>
      <RouterLink class="px-3 py-1 rounded bg-gray-100" to="/operator">Operator</RouterLink>
      <button class="px-3 py-1 rounded bg-red-500 text-white ml-2" @click="logout">Logout</button>
    </TopBar>
  </div>
</template>