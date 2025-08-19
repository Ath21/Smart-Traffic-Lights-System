<script setup>
import TrafficLights from "../components/TrafficLights.vue";
import Sensors from "../components/Sensors.vue";
import Analytics from "../components/Analytics.vue";
import Logs from "../components/Logs.vue";
import Notifications from "../components/Notifications.vue";
</script>

<template>
  <div class="p-6 grid grid-cols-1 lg:grid-cols-3 gap-6">
    <!-- Left column -->
    <div class="lg:col-span-2 space-y-6">
      <TrafficLights />
      <Sensors />
      <Analytics />
    </div>

    <!-- Right column -->
    <div class="space-y-6">
      <Notifications />
      <Logs />
    </div>
  </div>
</template>
