import axios from 'axios'
const api = axios.create({ baseURL: 'http://localhost:5070/api/sensors' })

export async function getSnapshot(intersectionId) {
  return (await api.get(`${intersectionId}`)).data
}

export async function getHistory(token, intersectionId) {
  return (await api.get(`${intersectionId}/history`, {
    headers: { Authorization: `Bearer ${token}` }
  })).data
}

export async function updateSensor(token, snapshot) {
  return (await api.post('update', snapshot, {
    headers: { Authorization: `Bearer ${token}` }
  })).data
}
