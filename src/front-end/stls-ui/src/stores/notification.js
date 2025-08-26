// src/stores/notifications.js
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { getUserNotificationsApi } from '../services/notificationApi'
import { useAuth } from './auth'

export const useNotifications = defineStore('notifications', () => {
  const notifications = ref([])
  const isLoading = ref(false)
  const error = ref(null)
  let poller = null

  const auth = useAuth()

  async function fetchNotifications() {
    if (!auth.token || !auth.user?.userId) return
    isLoading.value = true
    error.value = null

    try {
      notifications.value = await getUserNotificationsApi(auth.token, auth.user.userId)
    } catch (err) {
      console.error("❌ Failed to fetch notifications:", err)
      error.value = err.message || "Failed to load"
    } finally {
      isLoading.value = false
    }
  }

  function startPolling(intervalMs = 10000) {
    if (poller) clearInterval(poller)
    fetchNotifications()
    poller = setInterval(fetchNotifications, intervalMs)
  }

  function stopPolling() {
    if (poller) clearInterval(poller)
    poller = null
  }

  return { notifications, isLoading, error, fetchNotifications, startPolling, stopPolling }
})
