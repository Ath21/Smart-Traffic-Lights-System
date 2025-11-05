<template>
  <div class="p-6 max-w-6xl mx-auto">
    <!-- Intersection Header -->
    <div class="mb-6">
      <h1 class="text-3xl font-bold mb-1">
        {{ coordinatorStore.selectedIntersection?.Name || 'Loading...' }}
      </h1>
      <p class="text-gray-700 mb-1">{{ coordinatorStore.selectedIntersection?.Location }}</p>
      <p class="text-gray-500 text-sm">
        Coordinates: 
        {{ coordinatorStore.selectedIntersection?.Coordinates.Latitude }},
        {{ coordinatorStore.selectedIntersection?.Coordinates.Longitude }}
      </p>
    </div>

    <!-- Loading & Error -->
    <div v-if="coordinatorStore.loading" class="text-gray-500 font-medium mb-4">Loading intersection...</div>
    <div v-if="coordinatorStore.error" class="text-red-500 font-medium mb-4">{{ coordinatorStore.error }}</div>

    <!-- Traffic Lights Grid -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      <div
        v-for="light in coordinatorStore.selectedIntersection?.TrafficLights || []"
        :key="light.LightId"
        class="p-4 border rounded-lg shadow-md bg-white hover:shadow-lg transition-shadow duration-200 card-shadow"
      >
        <!-- Light Name & Direction -->
        <h2 class="font-semibold text-lg mb-2">
          {{ light.LightName }} 
          <span class="text-gray-500 text-sm">({{ light.Direction }})</span>
        </h2>

        <!-- Operational Status -->
        <p class="mb-1">
          Operational: 
          <span 
            :class="{'text-green-600 font-bold': light.IsOperational, 'text-red-600 font-bold': !light.IsOperational}">
            {{ light.IsOperational ? 'Yes' : 'No' }}
          </span>
        </p>

        <!-- Current Phase with blinking circle -->
        <p class="mb-1 flex items-center gap-2">
          Current Phase: 
          <span 
            class="w-4 h-4 rounded-full transition-colors duration-500"
            :class="phaseClass(light.LightId)"
          ></span>
          <span class="font-semibold">
            {{ lightControllerStore.lightsData[light.LightId]?.state?.Phase || 'Loading...' }}
          </span>
        </p>

        <!-- Remaining Timer -->
        <p class="mb-1 text-sm text-gray-600">
          Remaining: {{ lightControllerStore.lightsData[light.LightId]?.state?.Remaining ?? '-' }}s
        </p>

        <!-- Mode -->
        <p class="mb-1 text-sm text-gray-600">
          Mode: {{ lightControllerStore.lightsData[light.LightId]?.state?.Mode ?? '-' }}
        </p>

        <!-- Failover Status -->
        <p class="mb-1">
          Failover: 
          <span 
            :class="{'text-red-600 font-bold': lightControllerStore.lightsData[light.LightId]?.failover?.Failover, 
                     'text-green-600 font-bold': !lightControllerStore.lightsData[light.LightId]?.failover?.Failover}">
            {{ lightControllerStore.lightsData[light.LightId]?.failover?.Failover ? 'Yes' : 'No' }}
          </span>
        </p>

        <!-- Cycle Info -->
        <p class="text-sm text-gray-500">
          Cycle Duration: {{ lightControllerStore.lightsData[light.LightId]?.cycle?.CycleDuration ?? '-' }}s
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, watch } from "vue";
import { useRoute } from "vue-router";
import { useCoordinatorStore } from "../../stores/coordinatorStore";
import { useLightControllerStore } from "../../stores/lightControllerStore";
import "../../assets/intersection-view.css";

const coordinatorStore = useCoordinatorStore();
const lightControllerStore = useLightControllerStore();
const route = useRoute();

// Compute the class for blinking circle
const phaseClass = (lightId) => {
  const phase = lightControllerStore.lightsData[lightId]?.state?.Phase;
  if (!phase) return '';
  return {
    'bg-green-500 blink': phase === 'Green',
    'bg-yellow-400 blink': phase === 'Yellow',
    'bg-red-500 blink': phase === 'Red'
  };
};

// Load intersection by name from route
// Load intersection by name from route
async function loadIntersection() {
  const intersectionName = route.params.name;
  if (!intersectionName) return;

  // Map intersection names to IDs
  let intersectionId;
  switch (intersectionName.toLowerCase()) {
    case "agiou-spyridonos":
      intersectionId = 1;
      break;
    case "anatoliki-pyli":
      intersectionId = 2;
      break;
    case "dytiki-pyli":
      intersectionId = 3;
      break;
    case "ekklisia":
      intersectionId = 4;
      break;
    case "kentriki-pyli":
      intersectionId = 5;
      break;
    // Add more mappings here
    default:
      console.warn("Unknown intersection name:", intersectionName);
      return;
  }

  // Call existing store method
  await coordinatorStore.selectIntersection(intersectionId);

  if (coordinatorStore.selectedIntersection) {
    const lights = coordinatorStore.selectedIntersection.TrafficLights;
    const data = await lightControllerStore.fetchLightsForIntersection(lights);
    lightControllerStore.lightsData = { ...data };
  }
}


// Initial load
onMounted(() => {
  loadIntersection();

  setInterval(async () => {
    const lights = coordinatorStore.selectedIntersection?.TrafficLights;
    if (lights) {
      const data = await lightControllerStore.fetchLightsForIntersection(lights);
      lightControllerStore.lightsData = { ...data };
    }
  }, 5000);
});

// React to route changes (when user navigates to a different intersection)
watch(
  () => route.params.name,
  async () => {
    await loadIntersection();
  }
);
</script>
