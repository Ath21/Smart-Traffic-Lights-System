<script setup>
import MapView from '../components/MapView.vue'
import Topbar from '../components/TopBar.vue'
</script>

<template>
  <div class="h-screen flex flex-col">
    <!-- Top navigation -->

    <!-- Main map area -->
    <main class="flex-1">
      <MapView />
    </main>
  </div>
</template>
