import axios from 'axios';

const COORDINATOR_API = import.meta.env.VITE_COORDINATOR_API || 'http://localhost:5020'

const apiClient = axios.create({
  baseURL: COORDINATOR_API,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Optional: add JWT interceptor
apiClient.interceptors.request.use(config => {
  const token = localStorage.getItem('jwt');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

export const trafficCoordinatorService = {
  // Health & readiness
  health: () => apiClient.get('/traffic-light-coordinator/health'),
  ready: () => apiClient.get('/traffic-light-coordinator/ready'),

  // Intersections
  getAllIntersections: () => apiClient.get('/api/intersections/all'),
  getIntersection: (id) => apiClient.get(`/api/intersections/${id}`),

  // Traffic lights
  getAllTrafficLights: () => apiClient.get('/api/traffic-lights/all'),
  getTrafficLight: (id) => apiClient.get(`/api/traffic-lights/${id}`),

  // Traffic configurations
  getAllConfigurations: () => apiClient.get('/api/configurations/all'),
  getConfigurationsByMode: (mode) => apiClient.get('/api/configurations/mode', { params: { mode } }),

  // Traffic operator actions
  applyMode: (payload) => apiClient.post('/api/traffic-operator/apply-mode', payload),
  overrideLight: (payload) => apiClient.post('/api/traffic-operator/override-light', payload),
};
