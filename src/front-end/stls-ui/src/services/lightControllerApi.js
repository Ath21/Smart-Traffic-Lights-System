import axios from 'axios';

function getControllerClient(intersectionIndex) {
  const base = import.meta.env.VITE_TRAFFIC_LIGHT_CONTROLLER_BASE;
  const startPort = Number(import.meta.env.VITE_TRAFFIC_LIGHT_CONTROLLER_PORT_START);
  const port = startPort + intersectionIndex; // index 0 -> 5261
  const client = axios.create({
    baseURL: `${base}:${port}`,
    headers: { 'Content-Type': 'application/json' },
  });

  client.interceptors.request.use(config => {
    const token = localStorage.getItem('jwt');
    if (token) config.headers.Authorization = `Bearer ${token}`;
    return config;
  });

  return client;
}

export const trafficControllerService = {
  health: (index) => getControllerClient(index).get('/traffic-light-controller/health'),
  ready: (index) => getControllerClient(index).get('/traffic-light-controller/ready'),
  getState: (index) => getControllerClient(index).get('/api/traffic-lights/state'),
  getCycle: (index) => getControllerClient(index).get('/api/traffic-lights/cycle'),
  getFailover: (index) => getControllerClient(index).get('/api/traffic-lights/failover'),
};
