<template>
  <nav class="bg-blue-600 text-white px-6 py-4 flex justify-between items-center shadow">
    <!-- Logo -->
    <h1 class="text-xl font-bold">🚦 Smart Traffic</h1>

    <!-- Links -->
    <div class="space-x-6">
      <RouterLink to="/" class="hover:underline">Dashboard</RouterLink>
      <RouterLink to="/map" class="hover:underline">Map</RouterLink>
      <RouterLink to="/logs" class="hover:underline">Logs</RouterLink>
    </div>
  </nav>
</template>
