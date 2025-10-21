import axios from "axios";

// ===============================
// Base Configuration
// ===============================
const USER_API = import.meta.env.VITE_USER_API || "http://localhost:5055";

const api = axios.create({
  baseURL: USER_API,
  headers: { "Content-Type": "application/json" },
});

// ===============================
// Request Interceptor (JWT attach)
// ===============================
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("stls_token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// ===============================
// AUTHENTICATION
// ===============================

// LOGIN
export async function loginApi({ email, password }) {
  const { data } = await api.post("/api/users/login", {
    Email: email,
    Password: password,
  });

  // Persist token & expiry
  if (data?.Token) {
    localStorage.setItem("stls_token", data.Token);
    localStorage.setItem("stls_token_exp", data.ExpiresAt);
  }

  return {
    token: data.Token,
    expiresAt: data.ExpiresAt,
  };
}

// REGISTER
export async function registerApi({ email, username, password, confirmPassword }) {
  const { data } = await api.post("/api/users/register", {
    Email: email,
    Username: username,
    Password: password,
    ConfirmPassword: confirmPassword,
  });
  return data;
}

// LOGOUT
export async function logoutApi() {
  try {
    await api.post("/api/users/logout");
  } finally {
    // Always clear local storage
    localStorage.removeItem("stls_token");
    localStorage.removeItem("stls_token_exp");
  }
}

// ===============================
// PROFILE MANAGEMENT
// ===============================

// GET PROFILE
export async function getProfileApi() {
  const { data } = await api.get("/api/users/profile");
  return data;
}

// UPDATE PROFILE
export async function updateProfileApi(token, form) {
  const payload = {
    Email: form.email,
    Username: form.username,
    Password: form.password || null,
    ConfirmPassword: form.confirmPassword || null,
  };

  const { data } = await api.put("/api/users/update", payload, {
    headers: { Authorization: `Bearer ${token}` },
  });

  return data;
}

// ===============================
// PASSWORD MANAGEMENT
// ===============================

export async function resetPasswordApi(email, newPassword, confirmPassword) {
  const { data } = await api.post("/api/users/reset-password", {
    Email: email,
    NewPassword: newPassword,
    ConfirmPassword: confirmPassword,
  });
  return data;
}

// ===============================
// SUBSCRIPTIONS
// ===============================

export async function subscribeApi({ intersection, metric }) {
  const { data } = await api.post("/api/users/subscriptions/subscribe", {
    Intersection: intersection,
    Metric: metric,
  });
  return data;
}

// ===============================
// HEALTH CHECKS
// ===============================

export async function checkHealth() {
  return api.get("/user-service/health");
}

export async function checkReady() {
  return api.get("/user-service/ready");
}
