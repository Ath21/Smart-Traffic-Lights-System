<template>
  <div class="chart-container">
    <h3>{{ title }}</h3>
    <Line v-if="chartData" :data="chartData" :options="chartOptions" />
    <div v-else class="no-data">No data available</div>
  </div>
</template>

<script setup>
import { defineProps } from "vue"
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  LineElement,
  CategoryScale,
  LinearScale,
  PointElement
} from "chart.js"
import { Line } from "vue-chartjs"

// Register chart.js components
ChartJS.register(Title, Tooltip, Legend, LineElement, CategoryScale, LinearScale, PointElement)

// Props
const props = defineProps({
  chartData: { type: Object, required: true },
  title: { type: String, default: "" }
})

// Chart options
const chartOptions = {
  responsive: true,
  maintainAspectRatio: false, // fill parent
  plugins: {
    legend: { position: "top" },
    title: { display: false }
  },
  scales: {
    x: { ticks: { autoSkip: false } },
    y: { beginAtZero: true }
  }
}
</script>

<style scoped>
.chart-container {
  width: 100%;
  height: 100%;
  min-height: 300px; /* ensure chart shows */
  display: flex;
  flex-direction: column;
}
.chart-container h3 {
  text-align: center;
  margin-bottom: 0.5rem;
}
.no-data {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #888;
  font-style: italic;
}
</style>
