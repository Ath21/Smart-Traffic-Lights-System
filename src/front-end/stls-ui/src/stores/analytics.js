import { defineStore } from 'pinia'
import { ref } from 'vue'
import { getCongestion, getDailyReports } from '../services/analyticsApi'

export const useAnalytics = defineStore('analytics', () => {
  const congestion = ref(null)
  const reports = ref([])

  async function fetchCongestion(intersectionId) {
    congestion.value = await getCongestion(intersectionId)
  }

  async function fetchDailyReports(token) {
    reports.value = await getDailyReports(token)
  }

  return { congestion, reports, fetchCongestion, fetchDailyReports }
})
