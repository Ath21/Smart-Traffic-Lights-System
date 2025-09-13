import axios from 'axios'
const api = axios.create({ baseURL: 'http://localhost:5208/api/traffic/analytics' })

export async function getCongestion(intersectionId) {
  return (await api.get(`congestion/${intersectionId}`)).data
}

export async function getIncidents(token, intersectionId) {
  return (await api.get(`incidents/${intersectionId}`, {
    headers: { Authorization: `Bearer ${token}` }
  })).data
}

export async function getSummary(token, intersectionId, date) {
  return (await api.get(`summary/${intersectionId}/${date}`, {
    headers: { Authorization: `Bearer ${token}` }
  })).data
}

export async function getDailyReports(token) {
  return (await api.get('reports/daily', {
    headers: { Authorization: `Bearer ${token}` }
  })).data
}
