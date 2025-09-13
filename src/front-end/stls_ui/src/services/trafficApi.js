import axios from 'axios'
const api = axios.create({ baseURL: 'http://localhost:5055/api/traffic' })

export async function controlLight(token, intersectionId, lightId, newState) {
  return (await api.post(
    `${intersectionId}/lights/${lightId}/control`,
    { newState },
    { headers: { Authorization: `Bearer ${token}` } }
  )).data
}

export async function updateLight(token, intersectionId, lightId, currentState) {
  return (await api.post(
    `${intersectionId}/lights/${lightId}/update`,
    { currentState },
    { headers: { Authorization: `Bearer ${token}` } }
  )).data
}
