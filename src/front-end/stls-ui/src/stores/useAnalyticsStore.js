import { defineStore } from 'pinia'
import { getDailySummary, getLiveEvents } from '@/services/analyticsService'

export const useAnalyticsStore = defineStore('analytics', {
  state: () => ({
    dailySummary: null,
    liveEvents: []
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
        this.liveEvents = await getLiveEvents()
      } catch (err) {
        console.error('Failed to load events', err)
      }
    }
  }
})
