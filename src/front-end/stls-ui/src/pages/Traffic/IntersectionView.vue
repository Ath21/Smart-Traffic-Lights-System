<template>
  <div class="p-4">
    <h1 class="text-2xl font-bold mb-2">
      {{ coordinatorStore.selectedIntersection?.Name || 'Loading...' }}
    </h1>
    <p class="mb-1">{{ coordinatorStore.selectedIntersection?.Location }}</p>
    <p class="mb-4">
      Coordinates: 
      {{ coordinatorStore.selectedIntersection?.Coordinates.Latitude }},
      {{ coordinatorStore.selectedIntersection?.Coordinates.Longitude }}
    </p>

    <div v-if="coordinatorStore.loading" class="text-gray-500">Loading...</div>
    <div v-if="coordinatorStore.error" class="text-red-500">{{ coordinatorStore.error }}</div>

    <!-- Traffic Lights Grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <div
        v-for="light in coordinatorStore.selectedIntersection?.TrafficLights || []"
        :key="light.LightId"
        class="p-4 border rounded shadow"
      >
        <h2 class="font-semibold">{{ light.LightName }} ({{ light.Direction }})</h2>
        <p>
          Operational: 
          <span :class="{'text-green-500': light.IsOperational, 'text-red-500': !light.IsOperational}">
            {{ light.IsOperational ? 'Yes' : 'No' }}
          </span>
        </p>

        <!-- Current Light State from LightControllerStore -->
        <p>
          Current State: 
          <span>
            {{
              lightControllerStore.lightsData[light.LightId]?.state?.currentState
              || 'Loading...'
            }}
          </span>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted } from "vue";
import { useCoordinatorStore } from "../../stores/coordinatorStore";
import { useLightControllerStore } from "../../stores/lightControllerStore";

const coordinatorStore = useCoordinatorStore();
const lightControllerStore = useLightControllerStore();

// Example: intersection ID could come from route params
const intersectionId = 1;

async function loadIntersection() {
  await coordinatorStore.selectIntersection(intersectionId);

  if (coordinatorStore.selectedIntersection) {
    // Fetch all traffic lights for this intersection
    const lights = coordinatorStore.selectedIntersection.TrafficLights;
    await lightControllerStore.fetchLightsForIntersection(lights);
  }
}

onMounted(() => {
  loadIntersection();

  // Optional: poll every 5s for live light states
  setInterval(() => {
    const lights = coordinatorStore.selectedIntersection?.TrafficLights;
    if (lights) lightControllerStore.fetchLightsForIntersection(lights);
  }, 5000);
});
</script>
