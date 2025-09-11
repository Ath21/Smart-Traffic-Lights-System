<script setup>
import { onMounted, ref } from 'vue'
import L from 'leaflet'
import '../assets/map.css'

const mapEl = ref(null)

// === Intersections with lights ===
const intersections = [
  {
    name: "Ekklhsia / Edessis",
    lights: [
      { label: "Edessis West", coords: [38.001575, 23.673565] },
      { label: "Edessis East", coords: [38.001593, 23.673690] },
      { label: "Dimitsanas", coords: [38.001638, 23.673619] },
    ],
    center: [38.001602, 23.673625]
  },
  {
    name: "Dytikh Pylh",
    lights: [
      { label: "Dytikh Pylh", coords: [38.002632, 23.674535] },
      { label: "Dimitsanas South", coords: [38.002590, 23.674503] },
      { label: "Dimitsanas North", coords: [38.002698, 23.674467] },
    ],
    center: [38.002640, 23.674500]
  },
  {
    name: "Agiou Spyridonos",
    lights: [
      { label: "Dimitsanas", coords: [38.004633, 23.676105] },
      { label: "Agiou Spyridonos", coords: [38.004679, 23.676150] },
    ],
    center: [38.004660, 23.676130]
  },
  {
    name: "Kentrikh Pylh",
    lights: [
      { label: "Kentrikh Pylh", coords: [38.004443, 23.676423] },
      { label: "Agiou Spyridonos", coords: [38.004483, 23.676533] },
    ],
    center: [38.004460, 23.676480]
  },
  {
    name: "Anatolikh Pylh",
    lights: [
      { label: "Anatolikh Pylh", coords: [38.003604, 23.677619] },
      { label: "Agioy Spyridonos", coords: [38.003683, 23.677760] },
    ],
    center: [38.003644, 23.677700]
  }
]

// --- Traffic light icon ---
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
    .setView([38.0035, 23.675], 16)

  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 20,
    attribution: '&copy; OpenStreetMap contributors',
  }).addTo(map)

  const group = L.featureGroup().addTo(map)

  intersections.forEach(intersection => {
    // Add traffic lights
    intersection.lights.forEach((p, idx) => {
      L.marker(p.coords, { icon: trafficIcon(idx % 2 === 0 ? 'green' : 'red') })
        .addTo(group)
        .bindPopup(`<b>${intersection.name}</b><br/>${p.label}<br/>${p.coords}`)
    })

    // Add perimeter circle
    const circleRadius = 20
    L.circle(intersection.center, {
      radius: circleRadius,
      color: 'blue',
      fillColor: '#3f82ff',
      fillOpacity: 0.1,
    }).addTo(group)

    // Add intersection name
    L.tooltip({
      permanent: true,
      direction: 'top',
      offset: [0, -25],
      className: 'intersection-label'
    })
      .setContent(`<b>${intersection.name}</b>`)
      .setLatLng(intersection.center)
      .addTo(group)
  })

  // Fit to all
  const all = intersections.flatMap(i => [
    i.center,
    ...i.lights.map(l => l.coords)
  ])
  map.fitBounds(L.latLngBounds(all), { padding: [40, 40] })
})
</script>

<template>
  <div ref="mapEl" class="w-full h-[calc(100vh-56px)] border-t border-gray-300 shadow-inner"></div>
</template>

<style>
.intersection-label {
  background: white;
  border-radius: 4px;
  padding: 2px 6px;
  font-size: 12px;
  font-weight: bold;
  color: #222;
  border: 1px solid #333;
  box-shadow: 0 1px 2px rgba(0,0,0,0.3);
}
</style>
