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

        <p>
          Operational:
          <span :class="{'green-text': light.IsOperational, 'red-text': !light.IsOperational}">
            {{ light.IsOperational ? 'Yes' : 'No' }}
          </span>
        </p>

        <p class="phase">
          Current Phase:
          <span :class="phaseClass(light.LightId)"></span>
          <span class="phase-text">
            {{ lightControllerStore.lightsData[light.LightId]?.state?.Phase || 'Loading...' }}
          </span>
        </p>

        <p class="info">
          Remaining: {{ lightControllerStore.lightsData[light.LightId]?.state?.Remaining ?? '-' }}s
        </p>

        <p class="info">
          Mode: {{ lightControllerStore.lightsData[light.LightId]?.state?.Mode ?? '-' }}
        </p>

        <p>
          Failover:
          <span :class="{
            'red-text': lightControllerStore.lightsData[light.LightId]?.failover?.Failover,
            'green-text': !lightControllerStore.lightsData[light.LightId]?.failover?.Failover
          }">
            {{ lightControllerStore.lightsData[light.LightId]?.failover?.Failover ? 'Yes' : 'No' }}
          </span>
        </p>

        <p class="info">
          Cycle Duration: {{ lightControllerStore.lightsData[light.LightId]?.cycle?.CycleDuration ?? '-' }}s
        </p>

        <!-- Inline Operator Controls (per light) -->
        <div v-if="userRole === 'Admin' || userRole === 'TrafficOperator'" class="light-operator-controls">
          <label>
            Override Phase:
            <select v-model="lightOverrides[light.LightId]">
              <option value="Green">Green</option>
              <option value="Yellow">Yellow</option>
              <option value="Red">Red</option>
            </select>
          </label>
          <button @click="overrideLight(light.LightId)">Override</button>
        </div>
      </div>
    </div>

    <!-- Configuration Info -->
    <div v-if="coordinatorStore.currentConfiguration" class="config-info">
      <h3 class="config-title">Configuration (Mode: {{ currentMode }})</h3>
      <p><strong>Cycle Duration:</strong> {{ coordinatorStore.currentConfiguration.CycleDurationSec }} s</p>
      <p><strong>Global Offset:</strong> {{ coordinatorStore.currentConfiguration.GlobalOffsetSec }} s</p>
      <p><strong>Purpose:</strong> {{ coordinatorStore.currentConfiguration.Purpose }}</p>
      <p><strong>Last Updated:</strong> {{ coordinatorStore.currentConfiguration.LastUpdated }}</p>

      <div class="lights-config">
        <div 
          v-for="light in coordinatorStore.selectedIntersection?.TrafficLights || []" 
          :key="light.LightId"
          class="light-config-card"
        >
          <h4>{{ light.LightName }} ({{ light.Direction }})</h4>

          <div class="phase-bar">
            <div
              v-for="(duration, phase) in safePhaseDurations"
              :key="phase"
              class="phase-segment"
              :style="{
                width: (duration / coordinatorStore.currentConfiguration.CycleDurationSec * 100) + '%',
                backgroundColor: phaseColor(phase)
              }"
            >
              {{ phase }} ({{ duration }}s)
            </div>
          </div>
        </div>
      </div>

<!-- Global Apply Mode (only operator/admin) -->
<div v-if="userRole === 'Admin' || userRole === 'TrafficOperator'" class="apply-mode">
  <label>
    Apply Mode:
    <select v-model="selectedMode">
      <option value="Standard">Standard</option>
      <option value="Peak">Peak</option>
      <option value="Night">Night</option>
      <option value="Pedestrian">Pedestrian</option>
      <option value="Cyclist">Cyclist</option>
      <option value="Emergency">Emergency</option>
      <option value="PublicTransport">Public Transport</option>
      <option value="Incident">Incident</option>
      <option value="Failover">Failover</option>
      <option value="Manual">Manual</option>
    </select>
  </label>
  <button @click="applyMode">Apply Mode</button>
</div>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, computed } from "vue";
import { useRoute } from "vue-router";
import { useCoordinatorStore } from "../../stores/coordinatorStore";
import { useLightControllerStore } from "../../stores/lightControllerStore";
import "../../assets/intersection-view.css";

const coordinatorStore = useCoordinatorStore();
const lightControllerStore = useLightControllerStore();
const route = useRoute();
const currentMode = ref(null);

// User Role (replace with actual auth store)
const userRole = ref("TrafficOperator");

// --- Apply Mode ---
const selectedMode = ref("");
const applyMode = async () => {
  if (!selectedMode.value || !coordinatorStore.selectedIntersection) return;
  await coordinatorStore.applyMode(coordinatorStore.selectedIntersection.Id, selectedMode.value);
  await coordinatorStore.fetchConfigurationByMode(selectedMode.value);
};

// --- Override Light (per light) ---
const lightOverrides = ref({});
const overrideLight = async (lightId) => {
  const phase = lightOverrides.value[lightId];
  if (!phase) return;
  await coordinatorStore.overrideLight({ LightId: lightId, Phase: phase });

  // refresh light states
  const lights = coordinatorStore.selectedIntersection?.TrafficLights;
  if (lights?.length) {
    const data = await lightControllerStore.fetchLightsForIntersection(lights);
    lightControllerStore.lightsData = { ...data };
  }
};

// Return the phase class for colored blinking circle
const phaseClass = (lightId) => {
  const phase = lightControllerStore.lightsData[lightId]?.state?.Phase;
  if (!phase) return '';
  return {
    'green-circle blink': phase === 'Green',
    'yellow-circle blink': phase === 'Yellow',
    'red-circle blink': phase === 'Red'
  };
};

// Phase color for config bars
const phaseColor = (phase) => {
  switch (phase.toLowerCase()) {
    case 'green': return '#16a34a';
    case 'yellow': return '#facc15';
    case 'red': return '#dc2626';
    default: return '#9ca3af';
  }
};

// Safe parsed PhaseDurations
const safePhaseDurations = computed(() => {
  if (!coordinatorStore.currentConfiguration?.PhaseDurations) return {};
  try { return JSON.parse(coordinatorStore.currentConfiguration.PhaseDurations); }
  catch { return {}; }
});

// Load intersection + lights + configuration
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

    const firstLightId = lights[0]?.LightId;
    const mode = lightControllerStore.lightsData[firstLightId]?.state?.Mode;
    if (mode) {
      currentMode.value = mode;
      await coordinatorStore.fetchConfigurationByMode(mode);
    }
  }
}

onMounted(() => {
  loadIntersection();
  setInterval(async () => {
    const lights = coordinatorStore.selectedIntersection?.TrafficLights;
    if (lights?.length) {
      const data = await lightControllerStore.fetchLightsForIntersection(lights);
      lightControllerStore.lightsData = { ...data };

      const firstLightId = lights[0]?.LightId;
      const mode = lightControllerStore.lightsData[firstLightId]?.state?.Mode;
      if (mode && mode !== currentMode.value) {
        currentMode.value = mode;
        await coordinatorStore.fetchConfigurationByMode(mode);
      }
    }
  }, 5000);
});

watch(() => route.params.name, async () => { await loadIntersection(); });
</script>
