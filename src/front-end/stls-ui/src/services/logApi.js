import axios from "axios"

const LOG_API = import.meta.env.VITE_LOG_API || "http://localhost:5005"

const api = axios.create({
  baseURL: LOG_API,
  headers: { "Content-Type": "application/json" }
})

api.interceptors.request.use(config => {
  const token = localStorage.getItem("stls_token")
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// === Fetch logs with optional filters ===
export async function searchLogs({ Layer, Service, Type, From, To } = {}) {
  const { data } = await api.get("/api/logs/search", {
    params: { Layer, Service, Type, From, To }
  })
  return data
}

// === Export logs in a specific format ===
export async function exportLogs(filters = {}, format = "csv") {
  const { data } = await api.post(`/api/logs/export?format=${format}`, filters, {
    responseType: "blob"
  })
  return data
}

// === Health checks ===
export async function checkHealth() {
  const { data } = await api.get("/log-service/health")
  return data
}

export async function checkReady() {
  const { data } = await api.get("/log-service/ready")
  return data
}
