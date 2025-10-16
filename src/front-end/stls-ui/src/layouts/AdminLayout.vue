<script setup>
const logs = [
  { ts: '2025-08-19 12:01', level: 'INFO', msg: 'Operator applied pattern rush_hour' },
  { ts: '2025-08-19 12:05', level: 'WARN', msg: 'Intersection Gate B offline 30s' },
]
</script>

<template>
  <div class="h-screen flex flex-col">
    <!-- admin panel -->
    <div class="p-4">
      <h2 class="font-bold mb-2">Logs</h2>
      <ul class="space-y-1">
        <li
          v-for="(log, idx) in logs"
          :key="idx"
          class="text-sm font-mono"
        >
          {{ log.ts }} [{{ log.level }}] - {{ log.msg }}
        </li>
      </ul>
    </div>
  </div>
</template>
