<script setup>
import TopBar from '../components/TopBar.vue'
import MapView from '../components/MapView.vue'
</script>

<template>
  <div class="h-screen flex flex-col">
    <TopBar>
      <RouterLink class="px-3 py-1 rounded bg-cyan-500 text-white hover:bg-cyan-600 transition" to="/login">Login</RouterLink>
      <RouterLink class="px-3 py-1 rounded bg-gray-800 text-white hover:bg-gray-700 transition" to="/register">Register</RouterLink>
    </TopBar>
    <MapView />
  </div>
</template>
