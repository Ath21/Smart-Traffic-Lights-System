<template>
  <div class="notifications-page">
    <h2>🔔 Your Notifications</h2>

    <button 
      v-if="notifications.length > 0 && unreadCount > 0" 
      class="mark-all-btn"
      @click="notificationsStore.markAllAsRead"
    >
      Mark all as read
    </button>

    <div v-if="isLoading">Loading...</div>
    <div v-else-if="error">⚠️ Failed to load notifications.</div>

    <ul v-else-if="notifications.length > 0" class="notifications-list">
      <li
        v-for="note in notifications"
        :key="note.NotificationId"
        :class="{ unread: !note.isRead }"
        @click="markRead(note)"
      >
        <div class="note-header">
          <strong>{{ note.Type }}</strong>
          <small>{{ formatDate(note.CreatedAt) }}</small>
        </div>
        <p>{{ note.Message }}</p>
      </li>

    </ul>

    <p v-else>No notifications found.</p>
  </div>
</template>

<script setup>
import { onMounted, onUnmounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useNotifications } from '../stores/notifications.js'
import '../assets/notifications.css'   // ✅ external styles

const notificationsStore = useNotifications()
const { notifications, isLoading, error, unreadCount } = storeToRefs(notificationsStore)

function formatDate(dateStr) {
  if (!dateStr) return "—"
  return new Date(dateStr).toLocaleString()
}

function markRead(note) {
  if (!note.isRead) {
    notificationsStore.markAsRead(note.notificationId)
  }
}

onMounted(() => {
  notificationsStore.startPolling(10000)
})

onUnmounted(() => {
  notificationsStore.stopPolling()
})
</script>
