import axios from 'axios'

const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

export async function getDailySummary() {
  const res = await axios.get(`${API_BASE}/analytics/daily-summary`)
  return res.data
}

export async function getLiveEvents() {
  const res = await axios.get(`${API_BASE}/events/live`)
  return res.data
}
