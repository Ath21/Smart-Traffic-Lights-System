<script setup>
import TopBar from '../components/TopBar.vue'
import MapView from '../components/MapView.vue'
import { useAuth } from '../stores/auth'
import { useRouter } from 'vue-router'

const auth = useAuth()
const router = useRouter()

function alertMe() {
  alert('Alerts enabled for incidents (demo).')
}

function logout() {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <div class="h-screen flex flex-col">
    <TopBar>
      <span class="text-sm text-gray-600 mr-4">
        Logged in as {{ auth.user?.username }} ({{ auth.user?.role }})
      </span>

      <button class="px-3 py-1 rounded bg-blue-600 text-white" @click="alertMe">
        Alert me
      </button>

      <RouterLink class="px-3 py-1 rounded bg-gray-100" to="/">Viewer</RouterLink>
      <button class="px-3 py-1 rounded bg-red-500 text-white ml-2" @click="logout">Logout</button>
    </TopBar>

    <MapView />
  </div>
</template>
