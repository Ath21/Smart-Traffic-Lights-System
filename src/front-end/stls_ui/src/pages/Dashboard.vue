<script setup>
import { onMounted } from "vue";
import * as L from "leaflet";
import { useTrafficStore } from "../stores/traffic";

const trafficStore = useTrafficStore();

onMounted(() => {
  // Initialize map centered on campus
  const map = L.map("map").setView([38.0029, 23.6748], 16);

  // Add OpenStreetMap tiles
  L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
    attribution: '&copy; OpenStreetMap contributors',
  }).addTo(map);

  // Custom colored circle markers for traffic lights
  trafficStore.lights.forEach((light) => {
    const color =
      light.status === "green"
        ? "green"
        : light.status === "red"
        ? "red"
        : "yellow";

    L.circleMarker(light.coords.reverse(), {
      radius: 8,
      color: "black",
      fillColor: color,
      fillOpacity: 0.9,
    })
      .addTo(map)
      .bindPopup(`<b>${light.location}</b><br>Status: ${light.status}`);
  });
});
</script>

<template>
  <div class="p-4 h-[calc(100vh-4rem)]">
    <h2 class="text-2xl font-bold mb-4">Campus Traffic Dashboard</h2>
    <div id="map" class="w-full h-[600px] rounded-lg shadow"></div>
  </div>
</template>
