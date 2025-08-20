<script setup>
import { onMounted, ref } from 'vue'
import L from 'leaflet'
import '../assets/map.css'


const mapEl = ref(null)

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
]

// custom traffic light icon with CSS classes
function trafficIcon(color = 'green') {
  return L.divIcon({
    html: `<div class="traffic-light ${color}"></div>`,
    className: '',
    iconSize: [24, 24],
    iconAnchor: [12, 12],
    popupAnchor: [0, -12],
  })
}

onMounted(() => {
  const map = L.map(mapEl.value, { zoomControl: true })

  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 20,
    attribution: '&copy; OpenStreetMap contributors',
  }).addTo(map)

  const group = L.featureGroup().addTo(map)

  sites.forEach(site => {
    site.points.forEach((p, idx) => {
      L.marker(p, { icon: trafficIcon(idx % 2 === 0 ? 'green' : 'red') })
        .addTo(group)
        .bindPopup(`<b>${site.name}</b><br/>Point ${idx + 1}<br/>${p[0].toFixed(6)}, ${p[1].toFixed(6)}`)
    })
  })

  const all = sites.flatMap(s => s.points)
  map.fitBounds(L.latLngBounds(all), { padding: [40, 40] })
})
</script>

<template>
  <div ref="mapEl" class="w-full h-[calc(100vh-56px)] border-t border-gray-300 shadow-inner"></div>
</template>

<style>
.leaflet-container {
  height: 100%;
  width: 100%;
}

.traffic-light {
  width: 18px;
  height: 18px;
  border-radius: 50%;
  border: 2px solid white;
  box-shadow: 0 0 6px rgba(0, 0, 0, 0.5);
}

.traffic-light.green {
  background: #00c853;
}

.traffic-light.red {
  background: #d50000;
}
</style>
