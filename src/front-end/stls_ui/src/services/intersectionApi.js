import axios from 'axios'
const api = axios.create({ baseURL: 'http://localhost:5190/api/intersections' })

export async function getStatus(token, intersectionId) {
  return (await api.get(`status/${intersectionId}`, {
    headers: { Authorization: `Bearer ${token}` }
  })).data
}

export async function getEvents(token, intersectionId) {
  return (await api.get(`events/${intersectionId}`, {
    headers: { Authorization: `Bearer ${token}` }
  })).data
}
