import { defineStore } from 'pinia'
import { getDailySummary, getLiveEvents } from '@/services/analyticsService'

let refreshInterval = null
let wsConnection = null

export const useAnalyticsStore = defineStore('analytics', {
  state: () => ({
    dailySummary: null,
    liveEvents: [],
    isRealtimeConnected: false,
  }),

  actions: {
    async fetchDailySummary() {
      try {
        this.dailySummary = await getDailySummary()
      } catch (err) {
        console.error('Failed to load summary', err)
      }
    },

    async fetchLiveEvents() {
      try {
        const data = await getLiveEvents()
        this.liveEvents = data ?? []
      } catch (err) {
        console.error('Failed to load events', err)
      }
    },

    /** 🕐 Start auto-refresh polling every 10 seconds */
    startPolling(interval = 10000) {
      this.stopPolling()
      refreshInterval = setInterval(() => {
        this.fetchDailySummary()
      }, interval)
    },

    stopPolling() {
      if (refreshInterval) clearInterval(refreshInterval)
      refreshInterval = null
    },

    /** 🌐 WebSocket real-time updates (if supported by backend) */
    connectWebSocket() {
      if (wsConnection) return
      const url = import.meta.env.VITE_WS_URL || 'ws://localhost:5000/ws/traffic'

      wsConnection = new WebSocket(url)
      wsConnection.onopen = () => {
        this.isRealtimeConnected = true
        console.log('✅ Realtime connected')
      }

      wsConnection.onmessage = (msg) => {
        try {
          const data = JSON.parse(msg.data)
          if (data.type === 'dailySummary') this.dailySummary = data.payload
          if (data.type === 'event') this.liveEvents.unshift(data.payload)
        } catch (err) {
          console.error('WebSocket parse error', err)
        }
      }

      wsConnection.onclose = () => {
        this.isRealtimeConnected = false
        console.warn('🔌 Realtime disconnected — retrying...')
        wsConnection = null
        setTimeout(() => this.connectWebSocket(), 5000)
      }
    },
  },
})
