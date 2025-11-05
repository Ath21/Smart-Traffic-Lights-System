<template>
  <div class="container">
    <!-- Intersection Header -->
    <div class="header">
      <h1>{{ coordinatorStore.selectedIntersection?.Name || 'Loading...' }}</h1>
      <p class="location">{{ coordinatorStore.selectedIntersection?.Location }}</p>
      <p class="coordinates">
        Coordinates: 
        {{ coordinatorStore.selectedIntersection?.Coordinates.Latitude }},
        {{ coordinatorStore.selectedIntersection?.Coordinates.Longitude }}
      </p>
    </div>

    <!-- Loading & Error -->
    <div v-if="coordinatorStore.loading" class="loading">Loading intersection...</div>
    <div v-if="coordinatorStore.error" class="error">{{ coordinatorStore.error }}</div>

    <!-- Traffic Lights Grid -->
    <div class="grid">
      <div
        v-for="light in coordinatorStore.selectedIntersection?.TrafficLights || []"
        :key="light.LightId"
        class="card"
      >
<!-- Light Name & Image -->
<div class="light-header">
  <img 
    :src="`/${light.LightId}.png`" 
    :alt="light.LightName" 
    class="light-img"
  />
  <h2 class="light-name">
    {{ light.LightName }} 
    <span class="direction">({{ light.Direction }})</span>
  </h2>
</div>


        <!-- Operational Status -->
        <p>
          Operational: 
          <span :class="{'green-text': light.IsOperational, 'red-text': !light.IsOperational}">
            {{ light.IsOperational ? 'Yes' : 'No' }}
          </span>
        </p>

        <!-- Current Phase with blinking circle -->
        <p class="phase">
          Current Phase: 
          <span :class="phaseClass(light.LightId)"></span>
          <span class="phase-text">
            {{ lightControllerStore.lightsData[light.LightId]?.state?.Phase || 'Loading...' }}
          </span>
        </p>

        <!-- Remaining Timer -->
        <p class="info">
          Remaining: {{ lightControllerStore.lightsData[light.LightId]?.state?.Remaining ?? '-' }}s
        </p>

        <!-- Mode -->
        <p class="info">
          Mode: {{ lightControllerStore.lightsData[light.LightId]?.state?.Mode ?? '-' }}
        </p>

        <!-- Failover Status -->
        <p>
          Failover: 
          <span :class="{
            'red-text': lightControllerStore.lightsData[light.LightId]?.failover?.Failover,
            'green-text': !lightControllerStore.lightsData[light.LightId]?.failover?.Failover
          }">
            {{ lightControllerStore.lightsData[light.LightId]?.failover?.Failover ? 'Yes' : 'No' }}
          </span>
        </p>

        <!-- Cycle Info -->
        <p class="info">
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

const phaseClass = (lightId) => {
  const phase = lightControllerStore.lightsData[lightId]?.state?.Phase;
  if (!phase) return '';
  return {
    'green-circle blink': phase === 'Green',
    'yellow-circle blink': phase === 'Yellow',
    'red-circle blink': phase === 'Red'
  };
};

async function loadIntersection() {
  const intersectionName = route.params.name;
  if (!intersectionName) return;

  let intersectionId;
  switch (intersectionName.toLowerCase()) {
    case "agiou-spyridonos": intersectionId = 1; break;
    case "anatoliki-pyli": intersectionId = 2; break;
    case "dytiki-pyli": intersectionId = 3; break;
    case "ekklisia": intersectionId = 4; break;
    case "kentriki-pyli": intersectionId = 5; break;
    default: console.warn("Unknown intersection name:", intersectionName); return;
  }

  await coordinatorStore.selectIntersection(intersectionId);

  if (coordinatorStore.selectedIntersection) {
    const lights = coordinatorStore.selectedIntersection.TrafficLights;
    const data = await lightControllerStore.fetchLightsForIntersection(lights);
    lightControllerStore.lightsData = { ...data };
  }
}

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

watch(() => route.params.name, async () => {
  await loadIntersection();
});
</script>
