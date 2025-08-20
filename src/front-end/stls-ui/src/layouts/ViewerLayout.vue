<script setup>
import TopBar from '../components/TopBar.vue'
import MapView from '../components/MapView.vue'
</script>


<template>
<div class="h-screen flex flex-col">
<TopBar>
<RouterLink class="px-3 py-1 rounded bg-gray-100" to="/login">Login</RouterLink>
<RouterLink class="px-3 py-1 rounded bg-gray-800 text-white" to="/register">Register</RouterLink>
</TopBar>
<MapView />
</div>
</template>