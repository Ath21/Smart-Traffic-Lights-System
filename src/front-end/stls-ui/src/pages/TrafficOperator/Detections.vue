<script setup>
import { onMounted, ref } from 'vue'
import { useAuth } from '../../stores/users'
import { getSnapshot, recordIncident } from '../../services/detectionApi'

const auth = useAuth()
const snapshot = ref(null)
const message = ref(null)
const intersectionId = '00000000-0000-0000-0000-000000000001'

onMounted(async () => {
  snapshot.value = await getSnapshot(intersectionId)
})

async function addIncident() {
  try {
    await recordIncident(auth.token, { intersectionId, description: "Accident detected" })
    message.value = "✅ Incident recorded!"
  } catch (e) {
    message.value = "❌ Failed to record incident"
  }
}
</script>

<template>
  <div class="p-4">
    <h2 class="font-bold mb-2">Detections Snapshot</h2>
    <pre>{{ snapshot }}</pre>

    <button class="btn-primary mt-4" @click="addIncident">Record Incident</button>
    <p>{{ message }}</p>
  </div>
</template>
