<script setup>
import { onMounted, onBeforeUnmount, ref } from 'vue'
import L from 'leaflet'
import '../assets/map.css'

import { useAuth } from '../stores/users'
import { getStates } from '../services/trafficControllerApi'

const mapEl = ref(null)
const auth = useAuth()
let map, group, poller

// === Intersections from DB (real UUIDs + coordinates) ===
const intersections = [
  {
    id: 'a1b2c3d4-1111-1111-1111-aaaaaaaaaaaa',
    name: 'Ekklhsia',
    center: [38.001580, 23.673638]
  },
  {
    id: 'a1b2c3d4-2222-2222-2222-bbbbbbbbbbbb',
    name: 'Dytikh Pylh',
    center: [38.002644, 23.674499]
  },
  {
    id: 'a1b2c3d4-3333-3333-3333-cccccccccccc',
    name: 'Agiou Spyridonos',
    center: [38.004677, 23.676086]
  },
  {
    id: 'a1b2c3d4-4444-4444-4444-dddddddddddd',
    name: 'Kentrikh Pylh',
    center: [38.004456, 23.676483]
  },
  {
    id: 'a1b2c3d4-5555-5555-5555-eeeeeeeeeeee',
    name: 'Anatolikh Pylh',
    center: [38.003558, 23.678042]
  }
]

function trafficIcon(state = 'red') {
  return L.divIcon({
    html: `<div class="traffic-light ${state.toLowerCase()}"></div>`,
    className: '',
    iconSize: [20, 20],
    iconAnchor: [10, 10],
    popupAnchor: [0, -10],
  })
}

async function loadStates() {
  group.clearLayers()

  for (const inter of intersections) {
    try {
      const lights = await getStates(auth.token, inter.id)

      lights.forEach(light => {
        const state = (light.currentState || 'red').toLowerCase()
        const coords = [light.latitude, light.longitude]

        L.marker(coords, { icon: trafficIcon(state) })
          .addTo(group)
          .bindPopup(
            `<b>${inter.name}</b><br/>${light.label || light.lightId}<br/>State: ${state}`
          )
      })

      // Add perimeter + label
      L.circle(inter.center, {
        radius: 20,
        color: 'blue',
        fillColor: '#3f82ff',
        fillOpacity: 0.1,
      }).addTo(group)

      L.tooltip({
        permanent: true,
        direction: 'top',
        offset: [0, -25],
        className: 'intersection-label'
      })
        .setContent(`<b>${inter.name}</b>`)
        .setLatLng(inter.center)
        .addTo(group)

    } catch (err) {
      console.error(`❌ Failed to load states for ${inter.name}`, err)
    }
  }
}

onMounted(async () => {
  map = L.map(mapEl.value, { zoomControl: true }).setView([38.0035, 23.675], 16)

  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 20,
    attribution: '&copy; OpenStreetMap contributors',
  }).addTo(map)

  group = L.featureGroup().addTo(map)

  await loadStates()
  map.fitBounds(L.latLngBounds(intersections.map(i => i.center)), { padding: [40, 40] })

  // 🔄 Auto-refresh every 5s
  poller = setInterval(loadStates, 5000)
})

onBeforeUnmount(() => {
  if (poller) clearInterval(poller)
})
</script>

<template>
  <div ref="mapEl" class="w-full h-[calc(100vh-56px)] border-t border-gray-300 shadow-inner"></div>
</template>
