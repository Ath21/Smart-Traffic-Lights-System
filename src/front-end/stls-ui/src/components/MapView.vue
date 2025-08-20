<script setup>
import { onMounted, ref } from 'vue'
import L from 'leaflet'

const mapEl = ref(null)

// Your campus intersections & gate points (Lat, Lng)
const sites = [
  {
    name: 'Left Intersection — Edessis',
    points: [
      [38.00152733622807, 23.67372816002177],
      [38.00165837717111, 23.673722795604068],
      [38.00162456017597, 23.673620871667698],
    ],
  },
  {
    name: 'Mid Intersection — Dimitsanas Gate',
    points: [
      [38.002491943770984, 23.674363239694117],
      [38.002686388980045, 23.674508078972114],
      [38.002610019425525, 23.674544473009455],
    ],
  },
  {
    name: 'Dimitsanas Intersection (upper‑left corner)',
    points: [
      [38.00464905032311, 23.67606783486132],
      [38.004681928278714, 23.67616266526471],
    ],
  },
  {
    name: 'Agiou Spyridonos Gate',
    points: [
      [38.00445178227973, 23.67646612255553],
      [38.0044607490205, 23.67657233260732],
    ],
  },
  {
    name: 'Milou Gate',
    points: [
      [38.00365373795255, 23.677717883925634],
    ],
  },
]

// simple 🚦 icon (no external images needed)
const lightIcon = L.divIcon({
  html: '🚦',
  className: 'stls-traffic-icon',
  iconSize: [24, 24],
  iconAnchor: [12, 12],
  popupAnchor: [0, -12],
})

onMounted(() => {
  const map = L.map(mapEl.value, {
    zoomControl: true,
  })

  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 20,
    attribution: '&copy; OpenStreetMap contributors',
  }).addTo(map)

  const group = L.featureGroup().addTo(map)

  // Plot points
  sites.forEach(site => {
    site.points.forEach((p, idx) => {
      L.marker(p, { icon: lightIcon })
        .addTo(group)
        .bindPopup(`<b>${site.name}</b><br/>Point ${idx + 1}<br/>${p[0].toFixed(6)}, ${p[1].toFixed(6)}`)
    })

    // optional: small hull/line to visualize each cluster
    if (site.points.length > 1) {
      L.polyline(site.points, { color: '#666', weight: 1, opacity: 0.6, dashArray: '4,4' }).addTo(group)
    }
  })

  // Fit to all points
  const all = sites.flatMap(s => s.points)
  map.fitBounds(L.latLngBounds(all), { padding: [40, 40] })
})
</script>

<template>
  <div ref="mapEl" class="w-full h-[calc(100vh-56px)]"></div>
</template>

<style>
/* Keep the map sized */
.leaflet-container { height: 100%; width: 100%; }
/* Optional: make the emoji icon a tiny badge */
.stls-traffic-icon { font-size: 18px; line-height: 24px; text-align: center; }
</style>
