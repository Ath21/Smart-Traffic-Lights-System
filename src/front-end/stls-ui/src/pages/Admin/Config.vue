<script setup>
import { ref } from 'vue'
import { useAuth } from '../../stores/users'
import { upsertConfig } from '../../services/trafficCoordinatorApi'

const auth = useAuth()
const pattern = ref('')
const message = ref('')
const intersectionId = '00000000-0000-0000-0000-000000000001'

async function save() {
  try {
    await upsertConfig(auth.token, intersectionId, pattern.value)
    message.value = "✅ Config saved!"
  } catch (e) {
    message.value = "❌ Failed to save config"
  }
}
</script>

<template>
  <div class="p-4">
    <h2 class="font-bold mb-2">Intersection Config</h2>
    <textarea v-model="pattern" class="w-full border p-2" rows="4"></textarea>
    <button class="btn-primary mt-2" @click="save">Save Config</button>
    <p class="mt-2">{{ message }}</p>
  </div>
</template>
