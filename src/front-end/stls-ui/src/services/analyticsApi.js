import { analyticsApi } from "./httpClients"; // centralized client

// === Analytics API ===

// Fetch traffic summaries
export const fetchSummariesApi = (params = {}) =>
  analyticsApi.get("/api/analytics/summaries", { params });

// Fetch traffic alerts
export const fetchAlertsApi = (params = {}) =>
  analyticsApi.get("/api/analytics/alerts", { params });
