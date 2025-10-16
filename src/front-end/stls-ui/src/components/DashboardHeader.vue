<template>
  <header class="flex flex-col sm:flex-row items-start sm:items-center justify-between">
    <div>
      <h1 class="text-3xl font-bold text-gray-800">UNIWA STLS Dashboard</h1>
      <p class="text-gray-500">Real-time monitoring of campus traffic network</p>
    </div>
    <div class="mt-2 sm:mt-0 text-sm text-gray-400">
      {{ new Date().toLocaleString() }}
    </div>
  </header>
</template>
