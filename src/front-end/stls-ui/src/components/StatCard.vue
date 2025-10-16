<script setup>
defineProps({
  title: String,
  value: [String, Number],
  icon: String
})
</script>

<template>
  <div class="bg-white rounded-2xl shadow p-4 flex items-center justify-between transition hover:shadow-md">
    <div>
      <p class="text-sm text-gray-500">{{ title }}</p>
      <p class="text-2xl font-bold text-gray-900 mt-1">
        {{ value ?? '—' }}
      </p>
    </div>
    <div class="text-3xl">
      {{ icon }}
    </div>
  </div>
</template>
