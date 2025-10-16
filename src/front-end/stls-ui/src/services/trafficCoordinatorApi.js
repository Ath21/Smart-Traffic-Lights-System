import axios from 'axios'
const api = axios.create({ baseURL: 'http://localhost:5020/api/traffic/coordinator' })

export async function getConfig(token, intersectionId) {
  return (await api.get(`config-read/${intersectionId}`, {
    headers: { Authorization: `Bearer ${token}` }
  })).data
}

export async function upsertConfig(token, intersectionId, pattern) {
  return (await api.post(
    `config-create/${intersectionId}`,
    { pattern },
    { headers: { Authorization: `Bearer ${token}` } }
  )).data
}

export async function overridePriority(token, { intersectionId, type, reason }) {
  return (await api.post(
    'priority/override',
    { intersectionId, type, reason },
    { headers: { Authorization: `Bearer ${token}` } }
  )).data
}
