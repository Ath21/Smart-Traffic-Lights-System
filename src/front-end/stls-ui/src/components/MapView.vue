<script setup>
import { onMounted, ref } from 'vue'
import L from 'leaflet'
import '../assets/map.css'

const mapEl = ref(null)

// === Updated coordinates ===
const sites = [
  {
    name: 'Edessis',
    points: [
      [38.001555631630424, 23.67370752082095], // E
      [38.00161242321305, 23.673639242930516], // W
      [38.0016348409309, 23.673686658132205],  // N
    ],
  },
  {
    name: 'West Gate',
    points: [
      [38.00263542684669, 23.674509442640147], // E
      [38.00267363798658, 23.6744970376112],   // N
      [38.0026247632692, 23.674456439334644],  // S
    ],
  },
  {
    name: 'Dimitsanas',
    points: [
      [38.004671829254214, 23.676075006015218], // S
      [38.00467538368099, 23.67613928661976],   // E
    ],
  },
  {
    name: 'Central Gate',
    points: [
      [38.00445589750435, 23.676479861054602], // S
      [38.00445856333231, 23.67653399209],     // E
    ],
  },
  {
    name: 'East Gate',
    points: [
      [38.00366275759531, 23.67771229700096], // E
    ],
  },
]

// Custom CSS-based traffic light icons
function trafficIcon(color = 'green') {
  return L.divIcon({
    html: `<div class="traffic-light ${color}"></div>`,
    className: '',
    iconSize: [20, 20],
    iconAnchor: [10, 10],
    popupAnchor: [0, -10],
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
      // Alternate red / green for demo
      L.marker(p, { icon: trafficIcon(idx % 2 === 0 ? 'green' : 'red') })
        .addTo(group)
        .bindPopup(`<b>${site.name}</b><br/>Point ${idx + 1}<br/>${p[0].toFixed(6)}, ${p[1].toFixed(6)}`)
    })
  })

  // Fit to all
  const all = sites.flatMap(s => s.points)
  map.fitBounds(L.latLngBounds(all), { padding: [40, 40] })
})
</script>

<template>
  <div ref="mapEl" class="w-full h-[calc(100vh-56px)] border-t border-gray-300 shadow-inner"></div>
</template>
