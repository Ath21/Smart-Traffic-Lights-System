<template>
  <div class="map-wrapper">
    <!-- === Map Layer === -->
    <div ref="mapEl" class="leaflet-wrapper"></div>

    <!-- === Legend (Bottom Left) === -->
<div class="legend-box">
  <div><span class="traffic-light red"></span> Stop</div>
  <div><span class="traffic-light yellow"></span> Caution</div>
  <div><span class="traffic-light green"></span> Go</div>
  <div><span class="traffic-light orange"></span> Out of Service</div>
  <hr class="border-t border-gray-500 my-1">
  <div><span class="legend-icon low"></span> Low Congestion</div>
  <div><span class="legend-icon medium"></span> Medium Congestion</div>
  <div><span class="legend-icon high"></span> High Congestion</div>
</div>
  </div>

  
</template>

<script setup>
import { onMounted, onBeforeUnmount, ref } from "vue";
import L from "leaflet";
import "../assets/map.css";
import "../assets/legend-lights.css";

const mapEl = ref(null);
let map, group, intervalId;

// ===============================
// Intersections & lights
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
// Helpers
// ===============================
function trafficIcon(state = "red") {
  return L.divIcon({
    html: `<div class="traffic-light ${state.toLowerCase()}"></div>`,
    className: "",
    iconSize: [20, 20],
    iconAnchor: [10, 10],
    popupAnchor: [0, -10]
  });
}

// Random next state for simulation
function randomState(current) {
  const states = ["red", "yellow", "green"];
  const next = states[Math.floor(Math.random() * states.length)];
  return next === current ? randomState(current) : next;
}

// Determine congestion level based on lights
function getCongestion(inter) {
  const redCount = inter.lights.filter(l => l.state === "red").length;
  const total = inter.lights.length;
  const ratio = redCount / total;

  if (ratio === 0) return "low";
  if (ratio < 0.5) return "medium";
  return "high";
}

// Render map with dynamic labels
function renderMap() {
  group.clearLayers();

  intersections.forEach(inter => {
    inter.lights.forEach(light => {
      L.marker(light.coords, { icon: trafficIcon(light.state) })
        .addTo(group)
        .bindPopup(
          `<b>${inter.name}</b><br/>
           [${light.id}] ${light.label}<br/>
           State: <b>${light.state.toUpperCase()}</b>`
        );
    });

    // Circle for intersection center
    L.circle(inter.center, {
      radius: 20,
      color: "blue",
      fillColor: "#3f82ff",
      fillOpacity: 0.1
    }).addTo(group);

    // Dynamic label with congestion class
    const congestion = getCongestion(inter);
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
// Simulation
// ===============================
function startSimulation() {
  intervalId = setInterval(() => {
    intersections.forEach(inter => {
      inter.lights.forEach(light => {
        light.state = randomState(light.state);
      });
    });
    renderMap();
  }, 5000);
}

function stopSimulation() {
  if (intervalId) clearInterval(intervalId);
}

// ===============================
// Lifecycle
// ===============================
onMounted(() => {
  map = L.map(mapEl.value, { zoomControl: true }).setView([38.0035, 23.675], 16);

  L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
    maxZoom: 20,
    attribution: "&copy; OpenStreetMap contributors"
  }).addTo(map);

  group = L.featureGroup().addTo(map);
  renderMap();
  map.fitBounds(L.latLngBounds(intersections.map(i => i.center)), { padding: [40, 40] });

  setTimeout(() => map.invalidateSize(), 400);

  startSimulation();
});

onBeforeUnmount(() => {
  stopSimulation();
  map?.remove();
});
</script>
