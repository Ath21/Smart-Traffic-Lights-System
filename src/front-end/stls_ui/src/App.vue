<script setup>
</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <!-- Navbar -->
    <nav class="bg-blue-600 text-white px-6 py-4 flex justify-between">
      <h1 class="text-xl font-bold">🚦 Smart Traffic</h1>
      <div class="space-x-4">
        <RouterLink to="/" class="hover:underline">Home</RouterLink>
        <RouterLink to="/map" class="hover:underline">Map</RouterLink>
      </div>
    </nav>

    <!-- Page content -->
    <main class="p-6">
      <RouterView />
    </main>
  </div>
</template>
