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
    <div class="charts-container" v-if="summaries.length && !loading">
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

    <!-- Empty state -->
    <div v-else-if="!loading && !summaries.length" class="text-gray-500">
      No summaries found for the selected filters.
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAnalyticsStore } from '../../stores/analyticsStore'
import LineChart from '../../components/LineChart.vue'

const analytics = useAnalyticsStore()

// Filters
const intersection = ref('')
const intersectionId = ref(null)
const from = ref(new Date(Date.now() - 7*24*60*60*1000).toISOString().split('T')[0]) // 7 days ago
const to = ref(new Date().toISOString().split('T')[0])

const { summaries, loading, error } = analytics

// Load summaries with current filters
const loadSummaries = async () => {
  if (!intersectionId.value) return
  await analytics.fetchSummaries({
    intersectionId: intersectionId.value,
    intersection: intersection.value,
    from: from.value,
    to: to.value
  })
}

// === Computed chart data ===
const vehiclesChartData = computed(() => ({
  labels: summaries.value.map(s => new Date(s.Date).toLocaleDateString()),
  datasets: [{
    label: 'Total Vehicles',
    data: summaries.value.map(s => s.TotalVehicles),
    borderColor: '#1d4ed8',
    backgroundColor: '#1d4ed8AA',
    tension: 0.3
  }]
}))

const pedestriansChartData = computed(() => ({
  labels: summaries.value.map(s => new Date(s.Date).toLocaleDateString()),
  datasets: [{
    label: 'Total Pedestrians',
    data: summaries.value.map(s => s.TotalPedestrians),
    borderColor: '#059669',
    backgroundColor: '#059669AA',
    tension: 0.3
  }]
}))

const cyclistsChartData = computed(() => ({
  labels: summaries.value.map(s => new Date(s.Date).toLocaleDateString()),
  datasets: [{
    label: 'Total Cyclists',
    data: summaries.value.map(s => s.TotalCyclists),
    borderColor: '#b45309',
    backgroundColor: '#b45309AA',
    tension: 0.3
  }]
}))

const congestionChartData = computed(() => ({
  labels: summaries.value.map(s => new Date(s.Date).toLocaleDateString()),
  datasets: [{
    label: 'Congestion Index',
    data: summaries.value.map(s => s.CongestionIndex),
    borderColor: '#dc2626',
    backgroundColor: '#dc2626AA',
    tension: 0.3
  }]
}))

// Load initial summaries on mount
onMounted(loadSummaries)
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
  height: 300px;
}
</style>
