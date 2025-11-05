import { logApi } from "./httpClients"; // centralized client

// === Fetch logs with optional filters ===
export async function searchLogs({ Layer, Service, Type, From, To } = {}) {
  const { data } = await logApi.get("/api/logs/search", {
    params: { Layer, Service, Type, From, To }
  });
  return data;
}

// === Export logs in a specific format ===
export async function exportLogs(filters = {}, format = "csv") {
  const { data } = await logApi.post(`/api/logs/export?format=${format}`, filters, {
    responseType: "blob"
  });
  return data;
}

// === Health checks ===
export async function checkHealth() {
  const { data } = await logApi.get("/log-service/health");
  return data;
}

export async function checkReady() {
  const { data } = await logApi.get("/log-service/ready");
  return data;
}
