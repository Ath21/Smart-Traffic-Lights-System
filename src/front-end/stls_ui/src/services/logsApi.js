import axios from 'axios'
const api = axios.create({ baseURL: 'http://localhost:5005/api/logs' })

export async function getAuditLogs(token, serviceName) {
  return (await api.get(`audit/${serviceName}`, {
    headers: { Authorization: `Bearer ${token}` }
  })).data
}

export async function getErrorLogs(token, serviceName) {
  return (await api.get(`error/${serviceName}`, {
    headers: { Authorization: `Bearer ${token}` }
  })).data
}

export async function searchLogs(token, params) {
  return (await api.get('search', {
    headers: { Authorization: `Bearer ${token}` },
    params
  })).data
}
