<script setup>
import { useTrafficStore } from "../stores/traffic";
import { storeToRefs } from "pinia";

const trafficStore = useTrafficStore();
const { lights } = storeToRefs(trafficStore);

function switchLight(id) {
  trafficStore.toggleLight(id);
}
</script>

<template>
  <div>
    <h2 class="text-2xl font-bold mb-4">Traffic Lights</h2>

    <div class="space-y-4">
      <div
        v-for="light in lights"
        :key="light.id"
        class="p-4 rounded-lg shadow bg-white flex justify-between items-center"
      >
        <div>
          <p class="font-semibold">{{ light.location }}</p>
          <p>Status: 
            <span 
              :class="light.status === 'green' ? 'text-green-600' : 'text-red-600'">
              {{ light.status }}
            </span>
          </p>
          <p>Timer: {{ light.timer }}s</p>
        </div>

        <button 
          @click="switchLight(light.id)" 
          class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700">
          Toggle
        </button>
      </div>
    </div>
  </div>
</template>
