import axios from "axios";

const userApi = axios.create({
  baseURL: import.meta.env.VITE_USER_API,
  headers: { "Content-Type": "application/json" }
});

const notificationApi = axios.create({
  baseURL: import.meta.env.VITE_NOTIFICATION_API,
  headers: { "Content-Type": "application/json" }
});

userApi.interceptors.request.use(config => {
  const token = localStorage.getItem("stls_token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

notificationApi.interceptors.request.use(config => {
  const token = localStorage.getItem("stls_token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

export { userApi, notificationApi };
