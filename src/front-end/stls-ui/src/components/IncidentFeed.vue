<script setup>
defineProps({
  events: {
    type: Array,
    default: () => []
  }
})
</script>

<template>
  <div class="bg-white p-6 rounded-2xl shadow">
    <h2 class="text-lg font-semibold mb-4">Recent Events</h2>

    <div v-if="!events.length" class="text-gray-400 text-sm">No recent events</div>

    <ul v-else class="space-y-2 max-h-64 overflow-y-auto">
      <li
        v-for="(event, idx) in events"
        :key="idx"
        class="border-l-4 pl-3 py-1"
        :class="{
          'border-red-500': event.priorityLevel === 3,
          'border-yellow-400': event.priorityLevel === 2,
          'border-green-500': event.priorityLevel === 1
        }"
      >
        <p class="text-sm font-medium">{{ event.description }}</p>
        <p class="text-xs text-gray-400">{{ event.timestamp }}</p>
      </li>
    </ul>
  </div>
</template>
