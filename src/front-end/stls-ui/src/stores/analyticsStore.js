// src/stores/analyticsStore.js
import { defineStore } from 'pinia'
import { ref } from 'vue'

const API_BASE = import.meta.env.VITE_ANALYTICS_API || 'http://localhost:5208'

export const useAnalyticsStore = defineStore('analytics', () => {
  // ===============================
  // State
  // ===============================
  const latestAlerts = ref([])
  const summaries = ref([])
  const loading = ref(false)
  const error = ref(null)

  // ===============================
  // Fetch alerts
  // ===============================
  async function fetchAlerts({ type, intersection, from, to } = {}) {
    loading.value = true
    error.value = null
    try {
      const params = {}
      if (type) params.type = type
      if (intersection) params.intersection = intersection
      if (from) params.from = from
      if (to) params.to = to

      const query = new URLSearchParams(params).toString()
      const res = await fetch(`${API_BASE}/api/analytics/alerts?${query}`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('jwt')}`,
        },
      })

      if (!res.ok) {
        error.value = `[HTTP ${res.status}]`
        latestAlerts.value = []
        return
      }

      try {
        latestAlerts.value = await res.json()
        console.log('[AnalyticsStore] fetched alerts:', latestAlerts.value)
      } catch (err) {
        error.value = 'JSON parse error'
        latestAlerts.value = []
        console.error('[AnalyticsStore] fetchAlerts JSON parse error:', err)
      }
    } catch (err) {
      error.value = 'Network error'
      latestAlerts.value = []
      console.error('[AnalyticsStore] fetchAlerts network error:', err)
    } finally {
      loading.value = false
    }
  }

  // ===============================
  // Fetch summaries
  // ===============================
  async function fetchSummaries({ intersectionId, intersection, from, to } = {}) {
    loading.value = true
    error.value = null
    try {
      const params = {}
      if (intersectionId !== undefined) params.intersectionId = intersectionId
      if (intersection) params.intersection = intersection
      if (from) params.from = from
      if (to) params.to = to

      const query = new URLSearchParams(params).toString()
      const res = await fetch(`${API_BASE}/api/analytics/summaries?${query}`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('jwt')}`,
        },
      })

      if (!res.ok) {
        error.value = `[HTTP ${res.status}]`
        summaries.value = []
        return
      }

      try {
        summaries.value = await res.json() // ✅ use res.json() directly
        console.log('[AnalyticsStore] fetched summaries:', summaries.value)
      } catch (err) {
        error.value = 'JSON parse error'
        summaries.value = []
        console.error('[AnalyticsStore] fetchSummaries JSON parse error:', err)
      }
    } catch (err) {
      error.value = 'Network error'
      summaries.value = []
      console.error('[AnalyticsStore] fetchSummaries network error:', err)
    } finally {
      loading.value = false
    }
  }

  // ===============================
  // Return state & actions
  // ===============================
  return {
    latestAlerts,
    summaries,
    loading,
    error,
    fetchAlerts,
    fetchSummaries,
  }
})
