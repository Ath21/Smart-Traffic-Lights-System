<script setup>
import { onMounted } from 'vue'
import { useAnalyticsStore } from '@/stores/useAnalyticsStore'
import DashboardHeader from '@/components/DashboardHeader.vue'
import StatCard from '@/components/StatCard.vue'
import LiveTrafficPanel from '@/components/LiveTrafficPanel.vue'
import IncidentFeed from '@/components/IncidentFeed.vue'

const analytics = useAnalyticsStore()

onMounted(() => {
  analytics.fetchDailySummary()
  analytics.fetchLiveEvents()
})
</script>

<template>
  <div class="min-h-screen bg-gray-50 p-6">
    <DashboardHeader />

    <!-- Stat Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 my-6">
      <StatCard title="Vehicles Today" icon="🚗" :value="analytics.dailySummary?.vehicles" />
      <StatCard title="Pedestrians" icon="🚶‍♂️" :value="analytics.dailySummary?.pedestrians" />
      <StatCard title="Cyclists" icon="🚴‍♀️" :value="analytics.dailySummary?.cyclists" />
      <StatCard title="Current Mode" icon="⚙️" :value="analytics.dailySummary?.mode" />
    </div>

    <!-- Panels -->
    <div class="grid grid-cols-1 xl:grid-cols-2 gap-6">
      <LiveTrafficPanel :metrics="analytics.dailySummary" />
      <IncidentFeed :events="analytics.liveEvents" />
    </div>
  </div>
</template>
