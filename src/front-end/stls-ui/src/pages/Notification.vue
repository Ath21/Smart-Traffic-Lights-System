<template>
  <div class="notifications-page">
    <h2>🔔 Your Notifications</h2>

    <div v-if="isLoading">Loading...</div>
    <div v-else-if="error">⚠️ Failed to load notifications.</div>

    <ul v-else-if="notifications.length > 0" class="notifications-list">
      <li v-for="(note, idx) in notifications" :key="idx">
        <div class="note-header">
          <strong>{{ note.type }}</strong>
          <small>{{ formatDate(note.sentAt) }}</small>
        </div>
        <p>{{ note.message }}</p>
      </li>
    </ul>

    <p v-else>No notifications found.</p>
  </div>
</template>

<script setup>
import { onMounted, onUnmounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useNotifications } from '../stores/notification'
import '../assets/notifications.css'   // ✅ added

const notificationsStore = useNotifications()
const { notifications, isLoading, error } = storeToRefs(notificationsStore)

function formatDate(dateStr) {
  return new Date(dateStr).toLocaleString()
}

onMounted(() => {
  notificationsStore.startPolling(10000) // auto-refresh every 10s
})

onUnmounted(() => {
  notificationsStore.stopPolling()
})
</script>
