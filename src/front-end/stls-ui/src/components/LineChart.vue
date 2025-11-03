<template>
  <div class="chart-container" v-if="chartData && chartData.labels?.length">
    <h3>{{ title }}</h3>
    <!-- Pass as `data` instead of `chart-data` -->
    <Line :data="chartData" :options="chartOptions" />
  </div>
</template>


<script setup>
import { defineProps } from 'vue'
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  LineElement,
  CategoryScale,
  LinearScale,
  PointElement
} from 'chart.js'
import { Line } from 'vue-chartjs'

// Register chart.js components
ChartJS.register(Title, Tooltip, Legend, LineElement, CategoryScale, LinearScale, PointElement)

// Props
const props = defineProps({
  chartData: { type: Object, required: true },
  title: { type: String, default: '' }
})

// Options: fill parent and maintain aspect ratio automatically
const chartOptions = {
  responsive: true,
  maintainAspectRatio: false, // allow chart to fill container
  plugins: {
    legend: { position: 'top' },
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
}
.chart-container h3 {
  margin-bottom: 0.5rem;
  text-align: center;
}
</style>
