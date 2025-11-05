// src/services/httpClients.js
import axios from "axios";
import { decodeToken } from "../utils/jwtHelper";

/**
 * Creates an authenticated Axios client.
 * @param {string} baseURL - API base URL.
 * @returns {import('axios').AxiosInstance}
 */
function createHttpClient(baseURL) {
  const client = axios.create({
    baseURL,
    headers: { "Content-Type": "application/json" },
  });

  // === Request Interceptor for Authorization ===
  client.interceptors.request.use(
    (config) => {
      const token = localStorage.getItem("stls_token");
      if (token) {
        const decoded = decodeToken(token);
        // Optional: Check token expiry before attaching
        const now = Math.floor(Date.now() / 1000);
        if (decoded && decoded.exp > now) {
          config.headers.Authorization = `Bearer ${token}`;
        } else {
          console.warn("[httpClients] JWT expired or invalid, skipping header");
        }
      }
      return config;
    },
    (error) => Promise.reject(error)
  );

  // === Response Interceptor (optional logging) ===
  client.interceptors.response.use(
    (res) => res,
    (err) => {
      console.error("[HTTP Error]", err.response?.status, err.message);
      return Promise.reject(err);
    }
  );

  return client;
}

// ===============================================
// Core Service Clients
// ===============================================
export const userApi = createHttpClient(import.meta.env.VITE_USER_API);
export const notificationApi = createHttpClient(import.meta.env.VITE_NOTIFICATION_API);
export const logApi = createHttpClient(import.meta.env.VITE_LOG_API);
export const analyticsApi = createHttpClient(import.meta.env.VITE_TRAFIC_ANALYTICS_);
export const coordinatorApi = createHttpClient(import.meta.env.VITE_TRAFFIC_LIGHT_COORDINATOR);

// ===============================================
// Intersection-Specific Clients (Edge/Fog nodes)
// ===============================================
export const intersectionClients = {
  "AGIOU-SPYRIDONOS": {
    detectionApi: createHttpClient(import.meta.env.VITE_DETECTION_API_AGIOU_SPYRIDONOS),
    sensorApi: createHttpClient(import.meta.env.VITE_SENSOR_API_AGIOU_SPYRIDONOS),
    intersectionController: createHttpClient(import.meta.env.VITE_INTERSECTION_CONTROLLER_AGIOU_SPYRIDONOS),
    lightControllers: {
      AGIOU_SPYRIDONOS_101: createHttpClient(import.meta.env.VITE_TRAFFIC_LIGHT_CONTROLLER_AGIOU_SPYRIDONOS101),
      DIMITSANAS_102: createHttpClient(import.meta.env.VITE_TRAFFIC_LIGHT_CONTROLLER_DIMITSANAS102),
    },
  },

  "ANATOLIKI-PYLI": {
    detectionApi: createHttpClient(import.meta.env.VITE_DETECTION_API_ANATOLIKI_PYLI),
    sensorApi: createHttpClient(import.meta.env.VITE_SENSOR_API_ANATOLIKI_PYLI),
    intersectionController: createHttpClient(import.meta.env.VITE_INTERSECTION_CONTROLLER_ANATOLIKI_PYLI),
    lightControllers: {
      AGIOU_SPYRIDONOS_201: createHttpClient(import.meta.env.VITE_TRAFFIC_LIGHT_CONTROLLER_AGIOU_SPYRIDONOS201),
      ANATOLIKI_PYLI_202: createHttpClient(import.meta.env.VITE_TRAFFIC_LIGHT_CONTROLLER_ANATOLIKI_PYLI202),
    },
  },

  "DYTIKI-PYLI": {
    detectionApi: createHttpClient(import.meta.env.VITE_DETECTION_API_DYTIKI_PYLI),
    sensorApi: createHttpClient(import.meta.env.VITE_SENSOR_API_DYTIKI_PYLI),
    intersectionController: createHttpClient(import.meta.env.VITE_INTERSECTION_CONTROLLER_DYTIKI_PYLI),
    lightControllers: {
      DYTIKI_PYLI_301: createHttpClient(import.meta.env.VITE_TRAFFIC_LIGHT_CONTROLLER_DYTIKI_PYLI301),
      DIMITSANAS_NORTH_302: createHttpClient(import.meta.env.VITE_TRAFFIC_LIGHT_CONTROLLER_DIMITSANAS_NORTH302),
      DIMITSANAS_SOUTH_303: createHttpClient(import.meta.env.VITE_TRAFFIC_LIGHT_CONTROLLER_DIMITSANAS_SOUTH303),
    },
  },

  "EKKLISIA": {
    detectionApi: createHttpClient(import.meta.env.VITE_DETECTION_API_EKKLISIA),
    sensorApi: createHttpClient(import.meta.env.VITE_SENSOR_API_EKKLISIA),
    intersectionController: createHttpClient(import.meta.env.VITE_INTERSECTION_CONTROLLER_EKKLISIA),
    lightControllers: {
      DIMITSANAS_401: createHttpClient(import.meta.env.VITE_TRAFFIC_LIGHT_CONTROLLER_DIMITSANAS401),
      EDESSIS_402: createHttpClient(import.meta.env.VITE_TRAFFIC_LIGHT_CONTROLLER_EDESSIS402),
      KORYTSAS_403: createHttpClient(import.meta.env.VITE_TRAFFIC_LIGHT_CONTROLLER_KORYTSAS403),
    },
  },

  "KENTRIKI-PYLI": {
    detectionApi: createHttpClient(import.meta.env.VITE_DETECTION_API_KENTRIKI_PYLI),
    sensorApi: createHttpClient(import.meta.env.VITE_SENSOR_API_KENTRIKI_PYLI),
    intersectionController: createHttpClient(import.meta.env.VITE_INTERSECTION_CONTROLLER_KENTRIKI_PYLI),
    lightControllers: {
      KENTRIKI_PYLI_501: createHttpClient(import.meta.env.VITE_TRAFFIC_LIGHT_CONTROLLER_KENTRIKI_PYLI501),
      AGIOU_SPYRIDONOS_502: createHttpClient(import.meta.env.VITE_TRAFFIC_LIGHT_CONTROLLER_AGIOU_SPYRIDONOS502),
    },
  },
};
