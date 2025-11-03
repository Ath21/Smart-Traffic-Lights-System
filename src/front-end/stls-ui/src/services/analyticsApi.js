import axios from 'axios'

const ANALYTICS_API = import.meta.env.VITE_ANALYTICS_API || 'http://localhost:5208'

const api = axios.create({
  baseURL: ANALYTICS_API,
  headers: { 'Content-Type': 'application/json' },
})

// Attach JWT automatically
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('stls_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Fetch summaries
export const fetchSummariesApi = (params) => api.get('/api/analytics/summaries', { params })
export const fetchAlertsApi = (params) => api.get('/api/analytics/alerts', { params })
