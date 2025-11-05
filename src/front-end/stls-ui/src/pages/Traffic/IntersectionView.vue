<template>
  <div class="p-6">
    <h2 class="text-2xl font-bold mb-4">
      Intersection [{{ intersectionName }}]
    </h2>

    <div v-if="loadingCoordinator || loadingLights" class="text-gray-500">Loading data...</div>

    <div v-if="errorCoordinator" class="text-red-500">{{ errorCoordinator }}</div>
    <div v-if="errorLights" class="text-red-500">{{ errorLights }}</div>

    <div v-if="!loadingCoordinator && !errorCoordinator">
      <!-- === Coordinator Data === -->
      <div class="bg-white shadow-md rounded-2xl p-4 mb-6">
        <h3 class="text-xl font-semibold mb-2">Coordinator Data</h3>
        <pre class="text-sm bg-gray-50 p-3 rounded overflow-x-auto">
{{ coordinatorData }}
        </pre>
      </div>

      <!-- === Light Controller Data === -->
      <div class="bg-white shadow-md rounded-2xl p-4">
        <h3 class="text-xl font-semibold mb-2">Traffic Lights State</h3>

        <div v-if="Object.keys(lightControllerData).length === 0" class="text-gray-500">
          No traffic light data found.
        </div>

        <div
          v-for="(data, id) in lightControllerData"
          :key="id"
          class="mb-4 border rounded-xl p-3 bg-gray-50"
        >
          <h4 class="font-semibold mb-1">Light ID: {{ id }}</h4>
          <pre class="text-sm overflow-x-auto">{{ data }}</pre>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, computed } from "vue";
import { useRoute } from "vue-router";
import { useCoordinatorStore } from "../../stores/coordinatorStore";
import { useLightControllerStore } from "../../stores/lightControllerStore";

const route = useRoute();
const intersectionName = route.params.name;

const coordinatorStore = useCoordinatorStore();
const lightControllerStore = useLightControllerStore();

// Coordinator loading/error
const loadingCoordinator = computed(() => coordinatorStore.loading);
const errorCoordinator = computed(() => coordinatorStore.error);

// Lights loading/error for this intersection
const loadingLights = computed(() => {
  const ids = coordinatorStore.selectedIntersection?.trafficLights?.map(l => l.id) || [];
  return ids.some(id => !lightControllerStore.lightsData[id]);
});

const errorLights = computed(() => lightControllerStore.error);

// Coordinator data
const coordinatorData = computed(() => coordinatorStore.selectedIntersection);

// Lights for this intersection
const lightControllerData = computed(() => {
  const lights = coordinatorData.value?.trafficLights || [];
  return lights.reduce((acc, l) => {
    if (lightControllerStore.lightsData[l.id]) {
      acc[l.id] = lightControllerStore.lightsData[l.id];
    }
    return acc;
  }, {});
});

onMounted(async () => {
  if (!coordinatorStore.intersections.length) {
    await coordinatorStore.fetchIntersections();
  }

  const found = coordinatorStore.intersections.find(
    i => i.name.toLowerCase() === intersectionName.toLowerCase()
  );

  if (!found) {
    coordinatorStore.error = `Intersection '${intersectionName}' not found.`;
    return;
  }

  await coordinatorStore.selectIntersection(found.id);

  // Fetch all lights for this intersection
  if (coordinatorData.value?.trafficLights?.length) {
    await lightControllerStore.fetchLightsForIntersection(coordinatorData.value.trafficLights);
  }
});
</script>


<style scoped>
pre {
  font-family: "Fira Code", monospace;
}
</style>
