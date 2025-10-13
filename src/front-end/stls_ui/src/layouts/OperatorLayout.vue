<script setup>
import MapView from '../components/MapView.vue'
import { ref } from 'vue'

const current = ref('normal')
const patterns = ['normal','rush_hour','emergency_clear','all_red_test']

function applyPattern() {
  alert(`Applied pattern: ${current.value} (demo)`)
}
</script>

<template>
  <div class="h-screen flex flex-col">
    <MapView />

    <!-- operator controls -->
    <div class="p-4 bg-gray-100 border-t">
      <label class="mr-2">Pattern:</label>
      <select v-model="current" class="border rounded px-2 py-1">
        <option v-for="p in patterns" :key="p" :value="p">{{ p }}</option>
      </select>
      <button
        class="ml-2 px-3 py-1 bg-blue-600 text-white rounded"
        @click="applyPattern"
      >
        Apply
      </button>
    </div>
  </div>
</template>
