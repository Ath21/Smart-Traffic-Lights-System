// src/stores/notifications.js
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { getUserNotificationsApi } from '../services/authApi'
import { useAuth } from './auth'

export const useNotifications = defineStore('notifications', () => {
  const notifications = ref([])
  const isLoading = ref(false)
  const error = ref(null)
  let poller = null

  const auth = useAuth()

  // Fetch notifications from API
  async function fetchNotifications() {
    if (!auth.token || !auth.user?.userId) return
    isLoading.value = true
    error.value = null

    try {
      notifications.value = await getUserNotificationsApi(auth.token, auth.user.userId)
    } catch (err) {
      console.error("❌ Failed to fetch notifications:", err)
      error.value = err
    } finally {
      isLoading.value = false
    }
  }

  // Start auto-refresh (polling every X seconds)
  function startPolling(intervalMs = 10000) {
    if (poller) clearInterval(poller)
    fetchNotifications()
    poller = setInterval(fetchNotifications, intervalMs)
  }

  // Stop polling
  function stopPolling() {
    if (poller) clearInterval(poller)
    poller = null
  }

  return { notifications, isLoading, error, fetchNotifications, startPolling, stopPolling }
})
