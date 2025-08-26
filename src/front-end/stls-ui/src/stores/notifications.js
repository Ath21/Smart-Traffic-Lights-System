// src/stores/notifications.js
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { 
  getUserNotificationsApi, 
  markAsReadApi, 
  markAllAsReadApi 
} from '../services/notificationApi'
import { useAuth } from './users'

export const useNotifications = defineStore('notifications', () => {
  const notifications = ref([])
  const isLoading = ref(false)
  const error = ref(null)
  let poller = null

  const auth = useAuth()

  async function fetchNotifications() {
    if (!auth.token || !auth.user?.email) return
    isLoading.value = true
    error.value = null

    try {
      const data = await getUserNotificationsApi(auth.token, auth.user.email)

      // normalize backend PascalCase → frontend camelCase
      notifications.value = data.map(n => ({
        id: n.NotificationId,       // ✅ normalized
        type: n.Type,
        title: n.Title,
        message: n.Message,
        targetAudience: n.TargetAudience,
        recipientEmail: n.RecipientEmail,
        status: n.Status,
        createdAt: n.CreatedAt,
        isRead: n.Status === "Read"
      }))
    } catch (err) {
      console.error("❌ Failed to fetch notifications:", err)
      error.value = err.message || "Failed to load"
    } finally {
      isLoading.value = false
    }
  }

  // ✅ Mark one notification as read
  async function markAsRead(notificationId) {
    try {
      await markAsReadApi(auth.token, notificationId)
      const note = notifications.value.find(n => n.id === notificationId)
      if (note) note.isRead = true
    } catch (err) {
      console.error("❌ Failed to mark as read:", err)
    }
  }

  // ✅ Mark all as read
  async function markAllAsRead() {
    try {
      await markAllAsReadApi(auth.token)
      notifications.value.forEach(n => { n.isRead = true })
    } catch (err) {
      console.error("❌ Failed to mark all as read:", err)
    }
  }

  const unreadCount = computed(() =>
    notifications.value.filter(n => !n.isRead).length
  )

  function startPolling(intervalMs = 10000) {
    if (poller) clearInterval(poller)
    fetchNotifications()
    poller = setInterval(fetchNotifications, intervalMs)
  }

  function stopPolling() {
    if (poller) clearInterval(poller)
    poller = null
  }

  return { 
    notifications, 
    isLoading, 
    error, 
    fetchNotifications, 
    startPolling, 
    stopPolling, 
    markAsRead, 
    markAllAsRead, 
    unreadCount 
  }
})
