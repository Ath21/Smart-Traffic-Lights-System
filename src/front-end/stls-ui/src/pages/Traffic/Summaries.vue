<template>
  <div class="analytics-page">
    <h2>Traffic Summaries</h2>

    <!-- Filters -->
    <div class="filters">
      <input type="text" v-model="intersection" placeholder="Intersection" />
      <input type="number" v-model.number="intersectionId" placeholder="Intersection ID" />
      <input type="date" v-model="from" />
      <input type="date" v-model="to" />
    </div>

    <!-- Loading & errors -->
    <div v-if="loading">Loading summaries...</div>
    <div v-if="error" class="error">{{ error }}</div>

    <!-- Charts container -->
    <div v-if="!loading" class="charts-container">
      <div class="chart-wrapper">
        <LineChart 
          :chart-data="vehiclesChartData" 
          title="Vehicles per Day" 
          :key="vehiclesChartData.labels.join('-')" 
        />
      </div>
      <div class="chart-wrapper">
        <LineChart 
          :chart-data="pedestriansChartData" 
          title="Pedestrians per Day" 
          :key="pedestriansChartData.labels.join('-')" 
        />
      </div>
      <div class="chart-wrapper">
        <LineChart 
          :chart-data="cyclistsChartData" 
          title="Cyclists per Day" 
          :key="cyclistsChartData.labels.join('-')" 
        />
      </div>
      <div class="chart-wrapper">
        <LineChart 
          :chart-data="congestionChartData" 
          title="Average Wait Time (sec)" 
          :key="congestionChartData.labels.join('-')" 
        />
      </div>
    </div>

    <!-- Empty state -->
    <div v-if="!loading && !summaries.length" class="text-gray-500">
      No summaries found for the selected filters.
    </div>
  </div>
</template>

<script setup>
import { ref, watchEffect, onMounted } from "vue"
import { useAnalyticsStore } from "../../stores/analyticsStore"
import LineChart from "../../components/LineChart.vue"

const analytics = useAnalyticsStore()

// Filters
const intersection = ref("")
const intersectionId = ref(4) // default to a valid ID
const from = ref(new Date(Date.now() - 7*24*60*60*1000).toISOString().split("T")[0])
const to = ref(new Date().toISOString().split("T")[0])

const { summaries, loading, error, fetchSummaries } = analytics

// Chart data refs
const vehiclesChartData = ref({ labels: [], datasets: [] })
const pedestriansChartData = ref({ labels: [], datasets: [] })
const cyclistsChartData = ref({ labels: [], datasets: [] })
const congestionChartData = ref({ labels: [], datasets: [] })

// Fetch summaries when filters change
watchEffect(() => {
  if (!intersectionId.value) return
  fetchSummaries({
    intersectionId: intersectionId.value,
    intersection: intersection.value,
    from: from.value,
    to: to.value
  })
})

// Update charts whenever summaries change
watchEffect(() => {
  const data = summaries.value || []
  const labels = data.map(s => new Date(s.Date).toLocaleDateString())

  vehiclesChartData.value = {
    labels,
    datasets: [{
      label: "Total Vehicles",
      data: data.map(s => s.TotalVehicles || 0),
      borderColor: "#1d4ed8",
      backgroundColor: "#1d4ed8AA",
      tension: 0.3,
      fill: true
    }]
  }

  pedestriansChartData.value = {
    labels,
    datasets: [{
      label: "Total Pedestrians",
      data: data.map(s => s.TotalPedestrians || 0),
      borderColor: "#059669",
      backgroundColor: "#059669AA",
      tension: 0.3,
      fill: true
    }]
  }

  cyclistsChartData.value = {
    labels,
    datasets: [{
      label: "Total Cyclists",
      data: data.map(s => s.TotalCyclists || 0),
      borderColor: "#b45309",
      backgroundColor: "#b45309AA",
      tension: 0.3,
      fill: true
    }]
  }

  congestionChartData.value = {
    labels,
    datasets: [{
      label: "Average Wait Time (sec)",
      data: data.map(s => s.AverageWaitTimeSec || 0),
      borderColor: "#dc2626",
      backgroundColor: "#dc2626AA",
      tension: 0.3,
      fill: true
    }]
  }
})

// Fetch initial data on mount
onMounted(() => {
  if (intersectionId.value) {
    fetchSummaries({
      intersectionId: intersectionId.value,
      intersection: intersection.value,
      from: from.value,
      to: to.value
    })
  }
})
</script>

<style scoped>
.filters input {
  margin-right: 0.5rem;
  width: 150px;
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
  height: 350px; /* required for chart.js */
  display: flex;
  flex-direction: column;
}
</style>
