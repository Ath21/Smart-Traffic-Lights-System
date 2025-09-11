<script setup>
import { onMounted, ref } from 'vue'
import L from 'leaflet'
import '../assets/map.css'

const mapEl = ref(null)

// === Replace with your chosen Leaflet coordinates ===
const sites = [
  {
    name: "Ekklhsia / Edessis",
    points: [
      { label: "Edessis West", coords: [38.001575, 23.673565] },
      { label: "Edessis East", coords: [38.001593, 23.673690] },
      { label: "Dimitsanas", coords: [38.001638, 23.673619] },
    ]
  },
  {
    name: "Dytikh Pylh",
    points: [
      { label: "Dytikh Pylh", coords: [38.002632, 23.674535] },
      { label: "Dimitsanas South", coords: [38.002590, 23.674503] },
      { label: "Dimitsanas North", coords: [38.002698, 23.674467] },
    ]
  },
  {
    name: "Agiou Spyridonos",
    points: [
      { label: "Dimitsanas", coords: [38.004633, 23.676105] },
      { label: "Agiou Spyridonos", coords: [38.004679, 23.676150] },
    ]
  },
  {
    name: "Kentrikh Pylh",
    points: [
      { label: "Kentrikh Pylh", coords: [38.004443, 23.676423] },
      { label: "Agiou Spyridonos", coords: [38.004483, 23.676533] },
    ]
  },
  {
    name: "Anatolikh Pylh",
    points: [
      { label: "Anatolikh Pylh", coords: [38.003604, 23.677619] },
      { label: "Agioy Spyridonos", coords: [38.003683, 23.677760] },
    ]
  }
]

// --- Custom traffic light icons ---
function trafficIcon(color = 'green') {
  return L.divIcon({
    html: `<div class="traffic-light ${color}"></div>`,
    className: '',
    iconSize: [20, 20],
    iconAnchor: [10, 10],
    popupAnchor: [0, -10],
  })
}

// --- Utility: Get intersection center ---
function getCenter(points) {
  const lat = points.reduce((sum, p) => sum + p.coords[0], 0) / points.length
  const lng = points.reduce((sum, p) => sum + p.coords[1], 0) / points.length
  return [lat, lng]
}

onMounted(() => {
  const map = L.map(mapEl.value, { zoomControl: true })

  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 20,
    attribution: '&copy; OpenStreetMap contributors',
  }).addTo(map)

  const group = L.featureGroup().addTo(map)

  // Add all categorized points + perimeter circles
  sites.forEach(site => {
    site.points.forEach((p, idx) => {
      L.marker(p.coords, { icon: trafficIcon(idx % 2 === 0 ? 'green' : 'red') })
        .addTo(group)
        .bindPopup(
          `<b>${site.name}</b><br/>${p.label}<br/>${p.coords[0].toFixed(6)}, ${p.coords[1].toFixed(6)}`
        )
    })

    // Add perimeter circle for the whole intersection
    if (site.points.length > 1) {
      const center = getCenter(site.points)
      L.circle(center, {
        radius: 20,        // meters
        color: 'blue',
        fillColor: '#3f82ff',
        fillOpacity: 0.1,
      })
        .addTo(group)
        .bindPopup(`<b>${site.name}</b><br/>Perimeter Circle`)
    }
  })

  // Fit map to all points
  const all = sites.flatMap(s => s.points.map(p => p.coords))
  map.fitBounds(L.latLngBounds(all), { padding: [40, 40] })
})
</script>

<template>
  <div ref="mapEl" class="w-full h-[calc(100vh-56px)] border-t border-gray-300 shadow-inner"></div>
</template>
