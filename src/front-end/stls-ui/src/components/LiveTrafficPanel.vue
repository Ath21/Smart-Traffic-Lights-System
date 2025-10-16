<script setup>
import { ref, watch } from 'vue'
import { LineChart, Line, CartesianGrid, XAxis, YAxis, Tooltip, ResponsiveContainer } from 'recharts'

const props = defineProps({
  metrics: Object
})

// Simulated sample data for demo (replace with analytics data)
const data = ref([
  { time: '08:00', vehicles: 120 },
  { time: '09:00', vehicles: 240 },
  { time: '10:00', vehicles: 180 },
  { time: '11:00', vehicles: 300 },
  { time: '12:00', vehicles: 250 },
])
</script>

<template>
  <div class="bg-white p-6 rounded-2xl shadow">
    <h2 class="text-lg font-semibold mb-4">Live Traffic Flow (Vehicles/hour)</h2>
    <ResponsiveContainer width="100%" height="250">
      <LineChart :data="data">
        <Line type="monotone" dataKey="vehicles" stroke="#00447C" strokeWidth="3" />
        <CartesianGrid stroke="#E5E7EB" strokeDasharray="5 5" />
        <XAxis dataKey="time" />
        <YAxis />
        <Tooltip />
      </LineChart>
    </ResponsiveContainer>
  </div>
</template>
