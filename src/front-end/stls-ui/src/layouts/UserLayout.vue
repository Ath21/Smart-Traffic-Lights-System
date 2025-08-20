<script setup>
import TopBar from '../components/TopBar.vue'
import MapView from '../components/MapView.vue'
import { useAuth } from '../stores/auth'


const auth = useAuth()
function alertMe() {
alert('Alerts enabled for incidents (demo).')
}
</script>


<template>
<div class="h-screen flex flex-col">
<TopBar>
<<span class="text-sm text-gray-600">
  Logged in as {{ (auth.user?.username || auth.user?.email) }}
</span>

<button class="px-3 py-1 rounded bg-blue-600 text-white" @click="alertMe">Alert me</button>
<RouterLink class="px-3 py-1 rounded bg-gray-100" to="/">Viewer</RouterLink>
</TopBar>
<MapView />
</div>
</template>