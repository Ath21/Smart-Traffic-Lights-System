<script setup>
import TopBar from '../components/TopBar.vue'
const logs = [
{ ts: '2025-08-19 12:01', level: 'INFO', msg: 'Operator applied pattern rush_hour' },
{ ts: '2025-08-19 12:05', level: 'WARN', msg: 'Intersection Gate B offline 30s' },
]
</script>


<template>
<div class="h-screen flex flex-col">
<TopBar>
<RouterLink class="px-3 py-1 rounded bg-gray-100" to="/">Viewer</RouterLink>
<RouterLink class="px-3 py-1 rounded bg-gray-100" to="/operator">Operator</RouterLink>
</TopBar>


<div class="p-4 grid gap-4">
<section class="rounded-2xl bg-white shadow p-4">
<h2 class="font-semibold mb-2">System Logs</h2>
<table class="w-full text-sm">
<thead class="text-left text-gray-500">
<tr><th class="py-2">Time</th><th>Level</th><th>Message</th></tr>
</thead>
<tbody>
<tr v-for="l in logs" :key="l.ts" class="border-t">
<td class="py-2">{{ l.ts }}</td>
<td><span class="px-2 py-1 rounded bg-gray-100">{{ l.level }}</span></td>
<td>{{ l.msg }}</td>
</tr>
</tbody>
</table>
</section>


<section class="rounded-2xl bg-white shadow p-4">
<h2 class="font-semibold mb-2">Stats</h2>
<div class="grid grid-cols-2 md:grid-cols-4 gap-3">
<div class="rounded-xl border p-3"><div class="text-xs text-gray-500">Active intersections</div><div class="text-2xl font-semibold">12</div></div>
<div class="rounded-xl border p-3"><div class="text-xs text-gray-500">Incidents (24h)</div><div class="text-2xl font-semibold">3</div></div>
<div class="rounded-xl border p-3"><div class="text-xs text-gray-500">Avg latency</div><div class="text-2xl font-semibold">120ms</div></div>
<div class="rounded-xl border p-3"><div class="text-xs text-gray-500">Uptime</div><div class="text-2xl font-semibold">99.95%</div></div>
</div>
</section>
</div>
</div>
</template>