<template>
  <div class="map-wrapper">
    <div ref="mapEl" class="leaflet-wrapper"></div>

    <!-- === Legend === -->
    <div class="legend-box">
      <div><span class="traffic-light red"></span> Stop</div>
      <div><span class="traffic-light yellow"></span> Caution</div>
      <div><span class="traffic-light green"></span> Go</div>
      <div><span class="traffic-light orange"></span> Out of Service</div>
      <hr class="border-t border-gray-500 my-1" />
      <div><span class="legend-icon low"></span> Low Congestion</div>
      <div><span class="legend-icon medium"></span> Medium Congestion</div>
      <div><span class="legend-icon high"></span> High Congestion</div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, nextTick } from "vue";
import { useRouter } from "vue-router";
import L from "leaflet";
import "../assets/map.css";
import "../assets/legend-lights.css";

import { useLightControllerStore } from "../stores/lightControllerStore";
import { useAnalyticsStore } from "../stores/analyticsStore";

const router = useRouter();
const mapEl = ref(null);
let map, group, refreshIntervalId;

const lightStore = useLightControllerStore();
const analyticsStore = useAnalyticsStore();

// ===============================
// Static intersection definitions
// ===============================
const intersections = [
  {
    id: 4,
    name: "Ekklisia",
    center: [38.00158, 23.673638],
    lights: [
      { id: 401, label: "Dimitsanas", coords: [38.001626, 23.673627], state: "green" },
      { id: 402, label: "Edessis", coords: [38.001583, 23.673566], state: "red" },
      { id: 403, label: "Korytsas", coords: [38.001596, 23.673686], state: "yellow" }
    ]
  },
  {
    id: 3,
    name: "Dytiki Pyli",
    center: [38.002644, 23.674499],
    lights: [
      { id: 301, label: "Dytiki Pyli", coords: [38.002648, 23.674531], state: "green" },
      { id: 303, label: "Dimitsanas South", coords: [38.002606, 23.674487], state: "red" },
      { id: 302, label: "Dimitsanas North", coords: [38.002696, 23.674498], state: "yellow" }
    ]
  },
  {
    id: 1,
    name: "Agiou Spyridonos",
    center: [38.004677, 23.676086],
    lights: [
      { id: 102, label: "Dimitsanas", coords: [38.00464, 23.676094], state: "green" },
      { id: 101, label: "Agiou Spyridonos", coords: [38.004685, 23.676139], state: "red" }
    ]
  },
  {
    id: 5,
    name: "Kentriki Pyli",
    center: [38.004456, 23.676483],
    lights: [
      { id: 501, label: "Kentriki Pyli", coords: [38.004447, 23.676453], state: "green" },
      { id: 502, label: "Agiou Spyridonos", coords: [38.004467, 23.676528], state: "red" }
    ]
  },
  {
    id: 2,
    name: "Anatoliki Pyli",
    center: [38.003558, 23.678042],
    lights: [
      { id: 201, label: "Anatoliki Pyli", coords: [38.003549, 23.677997], state: "green" },
      { id: 202, label: "Agiou Spyridonos", coords: [38.00357, 23.678093], state: "red" }
    ]
  }
];

// ===============================
// Leaflet icon generator
// ===============================
function trafficIcon(state = "red") {
  const safe = String(state).toLowerCase();
  return L.divIcon({
    html: `<div class="traffic-light ${safe}"></div>`,
    className: "traffic-icon-wrapper",
    iconSize: [20, 20],
    iconAnchor: [10, 10],
    popupAnchor: [0, -10]
  });
}

// ===============================
// Congestion helper
// ===============================
function getCongestion(inter) {
  const summary = analyticsStore.summaries.find(s => s.intersectionId === inter.id);
  if (summary?.congestionLevel) return summary.congestionLevel.toLowerCase();

  const redCount = inter.lights.filter(l => l.state === "red").length;
  const ratio = redCount / inter.lights.length;
  return ratio === 0 ? "low" : ratio < 0.5 ? "medium" : "high";
}

// ===============================
// Render the intersections
// ===============================
function renderMap() {
  if (!map) return;

  if (group) map.removeLayer(group);
  group = L.featureGroup().addTo(map);

  intersections.forEach(inter => {
    inter.lights.forEach(light => {
      L.marker(light.coords, { icon: trafficIcon(light.state) })
        .addTo(group)
        .bindPopup(`<b>${inter.name}</b><br/>[${light.id}] ${light.label}<br/>State: <b>${light.state.toUpperCase()}</b>`);
    });

    const congestion = getCongestion(inter);
    const circle = L.circle(inter.center, {
      radius: 25,
      color: "blue",
      fillColor: "#3f82ff",
      fillOpacity: 0.1
    }).addTo(group);

    circle.on("click", () => router.push(`/stls/${encodeURIComponent(inter.name)}`));

    L.tooltip({
      permanent: true,
      direction: "top",
      offset: [0, -25],
      className: `intersection-label ${congestion}`
    })
      .setContent(`<b>[${inter.id}] ${inter.name}</b>`)
      .setLatLng(inter.center)
      .addTo(group);
  });
}

// ===============================
// Backend data + refresh
// ===============================
async function refreshMapData() {
  try {
    const results = await Promise.all(
      intersections.map(inter =>
        lightStore.fetchLightsForIntersection(inter.lights.map(l => ({ LightId: l.id })))
      )
    );

    intersections.forEach((inter, i) => {
      const data = results[i];
      inter.lights.forEach(l => {
        const light = data[l.id];
        l.state = light?.state ? String(light.state).toLowerCase() : "red";
      });
    });

    await analyticsStore.fetchSummaries();
    await nextTick();
    renderMap();
  } catch (err) {
    console.error("Map refresh error:", err);
  }
}

function startPolling() {
  refreshMapData();
  refreshIntervalId = setInterval(refreshMapData, 5000);
}
function stopPolling() {
  if (refreshIntervalId) clearInterval(refreshIntervalId);
}

// ===============================
// Lifecycle
// ===============================
onMounted(async () => {
  await nextTick();
  map = L.map(mapEl.value).setView([38.0035, 23.675], 16);

  L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
    maxZoom: 20,
    attribution: "&copy; OpenStreetMap contributors"
  }).addTo(map);

  renderMap();
  map.fitBounds(L.latLngBounds(intersections.map(i => i.center)), { padding: [40, 40] });
  setTimeout(() => map.invalidateSize(), 300);

  startPolling();
});

onBeforeUnmount(() => {
  stopPolling();
  map?.remove();
});
</script>
