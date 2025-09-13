import axios from 'axios'
const api = axios.create({ baseURL: 'http://localhost:5261/api/traffic/controller' })

export async function forceStateChange(token, intersectionId, lightId, currentState) {
  return (await api.post(
    `lights/${intersectionId}/${lightId}/state`,
    { currentState },
    { headers: { Authorization: `Bearer ${token}` } }
  )).data
}

export async function getStates(token, intersectionId) {
  return (await api.get(`lights/${intersectionId}`, {
    headers: { Authorization: `Bearer ${token}` }
  })).data
}

export async function getEvents(token, intersectionId) {
  return (await api.get(`events/${intersectionId}`, {
    headers: { Authorization: `Bearer ${token}` }
  })).data
}
