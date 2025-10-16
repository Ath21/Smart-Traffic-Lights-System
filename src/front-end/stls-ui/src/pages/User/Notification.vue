<template>
  <div class="notifications-page">
    <h2>🔔 Your Notifications</h2>

    <button
      v-if="unreadCount > 0"
      class="mark-all-btn"
      @click="markAll"
    >
      Mark all as read
    </button>

    <div v-if="isLoading">Loading...</div>
    <div v-else-if="error">⚠️ Failed to load notifications.</div>

    <ul v-else-if="notifications.length > 0" class="notifications-list">
      <li
        v-for="note in notifications"
        :key="note.id"
        :class="{ unread: !note.isRead }"
        @click="markSingleRead(note.id)"
      >
        <div class="note-header">
          <strong>{{ note.type }}</strong>
          <small>{{ formatDate(note.createdAt) }}</small>
        </div>
        <p>{{ note.message }}</p>
        <small class="note-recipient">{{ note.recipientEmail }}</small>
      </li>
    </ul>

    <p v-else>No notifications found.</p>
  </div>
</template>

<script setup>
import { onMounted, onUnmounted } from "vue"
import { storeToRefs } from "pinia"
import { useNotifications } from "../../stores/notifications"
import "../../assets/notifications.css"

const notificationsStore = useNotifications()
const { notifications, isLoading, error, unreadCount } =
  storeToRefs(notificationsStore)

function formatDate(dateStr) {
  return new Date(dateStr).toLocaleString()
}

function markSingleRead(id) {
  notificationsStore.markAsRead(id)
}

function markAll() {
  notificationsStore.markAllAsRead()
}

onMounted(() => {
  notificationsStore.startPolling(10000)
})

onUnmounted(() => {
  notificationsStore.stopPolling()
})
</script>
