import axios from 'axios'
const api = axios.create({ baseURL: 'http://localhost:5122/api/detections' })

export async function getSnapshot(intersectionId) {
  return (await api.get(`${intersectionId}`)).data
}

export async function getHistory(token, intersectionId) {
  return (await api.get(`${intersectionId}/history`, {
    headers: { Authorization: `Bearer ${token}` }
  })).data
}

export async function recordEmergency(token, data) {
  return (await api.post('emergency', data, {
    headers: { Authorization: `Bearer ${token}` }
  })).data
}

export async function recordPublicTransport(token, data) {
  return (await api.post('public-transport', data, {
    headers: { Authorization: `Bearer ${token}` }
  })).data
}

export async function recordIncident(token, data) {
  return (await api.post('incident', data, {
    headers: { Authorization: `Bearer ${token}` }
  })).data
}
