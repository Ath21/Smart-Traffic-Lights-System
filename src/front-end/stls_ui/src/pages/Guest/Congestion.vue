<script setup>
import { onMounted, ref } from 'vue'
import { getCongestion } from '../../services/analyticsApi'

const congestion = ref(null)
const error = ref(null)

// Demo intersection ID (replace with real one from DB)
const intersectionId = '00000000-0000-0000-0000-000000000001'

onMounted(async () => {
  try {
    congestion.value = await getCongestion(intersectionId)
  } catch (e) {
    error.value = e.message
  }
})
</script>

<template>
  <div class="p-4">
    <h2 class="text-xl font-bold mb-2">Live Congestion</h2>
    <pre v-if="congestion">{{ congestion }}</pre>
    <p v-if="error" class="text-red-500">{{ error }}</p>
  </div>
</template>
