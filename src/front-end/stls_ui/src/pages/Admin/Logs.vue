<script setup>
import { onMounted, ref } from 'vue'
import { useAuth } from '../../stores/users'
import { getErrorLogs } from '../../services/logsApi'

const auth = useAuth()
const logs = ref([])
const serviceName = 'TrafficLightController' // example

onMounted(async () => {
  logs.value = await getErrorLogs(auth.token, serviceName)
})
</script>

<template>
  <div class="p-4">
    <h2 class="font-bold mb-2">Service Logs</h2>
    <ul class="space-y-1">
      <li v-for="(log, idx) in logs" :key="idx" class="text-sm font-mono">
        {{ log.timestamp }} [{{ log.level }}] {{ log.message }}
      </li>
    </ul>
  </div>
</template>
