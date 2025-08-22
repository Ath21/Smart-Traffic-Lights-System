<script setup>
import TopBar from '../components/TopBar.vue'
import MapView from '../components/MapView.vue'
import { ref } from 'vue'


const current = ref('normal')
const patterns = ['normal','rush_hour','emergency_clear','all_red_test']
function applyPattern() { alert(`Applied pattern: ${current.value} (demo)`) }
</script>


<template>
<div class="h-screen flex flex-col">
<TopBar>
<RouterLink class="px-3 py-1 rounded bg-gray-100" to="/">Viewer</RouterLink>
<RouterLink class="px-3 py-1 rounded bg-gray-100" to="/app">User</RouterLink>
<RouterLink class="px-3 py-1 rounded bg-gray-800 text-white" to="/operator">Operator</RouterLink>
</TopBar>


<div class="grid grid-cols-1 md:grid-cols-3 gap-4 p-4">
<div class="md:col-span-2 rounded-2xl shadow bg-white">
<MapView />
</div>
<div class="rounded-2xl shadow bg-white p-4">
<h2 class="font-semibold mb-3">Manual Override</h2>
<label class="block text-sm mb-1">Pattern</label>
<select v-model="current" class="w-full border rounded p-2 mb-3">
<option v-for="p in patterns" :key="p" :value="p">{{ p }}</option>
</select>
<button class="px-3 py-2 rounded bg-green-600 text-white w-full" @click="applyPattern">Apply</button>
<div class="mt-4 text-sm text-gray-500">(In real app, this calls the backend to push a pattern to selected intersections.)</div>
</div>
</div>
</div>
</template>