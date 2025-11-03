<template>
  <div class="analytics-page">
    <h2>Traffic Summaries</h2>

    <!-- Filters -->
    <div class="filters">
      <input type="text" v-model="intersection" placeholder="Intersection" />
      <input type="number" v-model.number="intersectionId" placeholder="Intersection ID" />
      <input type="date" v-model="from" />
      <input type="date" v-model="to" />
      <button @click="loadSummaries">Load</button>
    </div>

    <!-- Loading & errors -->
    <div v-if="loading">Loading summaries...</div>
    <div v-if="error" class="error">{{ error }}</div>

    <!-- Charts container -->
    <div class="charts-container" v-if="summaries.length">
      <div class="chart-wrapper">
        <LineChart :chart-data="vehiclesChartData" title="Vehicles per Day" />
      </div>
      <div class="chart-wrapper">
        <LineChart :chart-data="pedestriansChartData" title="Pedestrians per Day" />
      </div>
      <div class="chart-wrapper">
        <LineChart :chart-data="cyclistsChartData" title="Cyclists per Day" />
      </div>
      <div class="chart-wrapper">
        <LineChart :chart-data="congestionChartData" title="Congestion Index" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, watch } from 'vue'
import { useAnalyticsStore } from '../../stores/analyticsStore'
import { storeToRefs } from 'pinia'
import LineChart from '../../components/LineChart.vue'

// Pinia store
const analytics = useAnalyticsStore()
const { summaries, loading, error } = storeToRefs(analytics)

// Filters
const intersection = ref('')
const intersectionId = ref(null)
const from = ref('')
const to = ref('')

// Fetch summaries
const loadSummaries = async () => {
  if (!intersectionId.value) return // prevent null request
  await analytics.fetchSummaries({
    intersectionId: intersectionId.value,
    intersection: intersection.value,
    from: from.value,
    to: to.value
  })
}

// Load initial data on mount
onMounted(loadSummaries)

// Initialize charts with empty but valid structure
const vehiclesChartData = reactive({ labels: [], datasets: [{ label: 'Total Vehicles', data: [] }] })
const pedestriansChartData = reactive({ labels: [], datasets: [{ label: 'Total Pedestrians', data: [] }] })
const cyclistsChartData = reactive({ labels: [], datasets: [{ label: 'Total Cyclists', data: [] }] })
const congestionChartData = reactive({ labels: [], datasets: [{ label: 'Congestion Index', data: [] }] })

// Watch summaries and update charts reactively
watch(summaries, (newSummaries) => {
  if (!newSummaries || !newSummaries.length) return

  const labels = newSummaries.map(s => new Date(s.Date).toLocaleDateString())

  vehiclesChartData.labels = labels
  vehiclesChartData.datasets[0].data = newSummaries.map(s => s.TotalVehicles)

  pedestriansChartData.labels = labels
  pedestriansChartData.datasets[0].data = newSummaries.map(s => s.TotalPedestrians)

  cyclistsChartData.labels = labels
  cyclistsChartData.datasets[0].data = newSummaries.map(s => s.TotalCyclists)

  congestionChartData.labels = labels
  congestionChartData.datasets[0].data = newSummaries.map(s => s.CongestionIndex)
}, { immediate: true })
</script>

<style scoped>
.filters input {
  margin-right: 0.5rem;
  width: 150px;
}
.filters button {
  margin-left: 0.5rem;
}
.error {
  color: red;
  margin-top: 1rem;
}
.analytics-page {
  padding: 3rem;
}

/* Charts layout */
.charts-container {
  display: flex;
  flex-wrap: wrap;
  gap: 2rem;
  margin-top: 2rem;
}
.chart-wrapper {
  flex: 1 1 45%;
  min-width: 300px;
  max-width: 600px;
  height: 300px; /* fixed height to prevent disappearing */
}
</style>
