<script setup>
import { onMounted, ref } from 'vue'
import { useAuth } from '../../stores/users'
import { getSnapshot as getSensorSnapshot } from '../../services/sensorApi'

const auth = useAuth()
const snapshot = ref(null)
const intersectionId = '00000000-0000-0000-0000-000000000001'

onMounted(async () => {
  snapshot.value = await getSensorSnapshot(intersectionId)
})
</script>

<template>
  <div class="p-4">
    <h2 class="font-bold mb-2">Sensor Snapshot</h2>
    <pre>{{ snapshot }}</pre>
  </div>
</template>
