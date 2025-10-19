import axios from "axios";

const USER_API = import.meta.env.VITE_USER_API || "http://localhost:5055";

const api = axios.create({
  baseURL: USER_API,
  headers: { "Content-Type": "application/json" }
});

api.interceptors.request.use(config => {
  const token = localStorage.getItem("stls_token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// LOGIN
export async function loginApi({ email, password }) {
  const { data } = await api.post("/api/users/login", {
    Email: email,
    Password: password
  });
  localStorage.setItem("stls_token", data.Token);
  localStorage.setItem("stls_token_exp", data.ExpiresAt);
  return { token: data.Token, expiresAt: data.ExpiresAt };
}

// REGISTER
export async function registerApi({ email, username, password, confirmPassword }) {
  const { data } = await api.post("/api/users/register", {
    Email: email,
    Username: username,
    Password: password,
    ConfirmPassword: confirmPassword
  });
  return data;
}

// PROFILE
export async function getProfileApi() {
  const { data } = await api.get("/api/users/profile");
  return data;
}

// UPDATE PROFILE
export async function updateProfileApi(token, form) {
  const { data } = await api.put("/api/users/update", {
    Email: form.email,
    Username: form.username,
    Password: form.password || null,
    ConfirmPassword: form.confirmPassword || null
  }, {
    headers: { Authorization: `Bearer ${token}` }
  });
  return data;
}

// RESET PASSWORD
export async function resetPasswordApi(email, newPassword, confirmPassword) {
  const { data } = await api.post("/api/users/reset-password", {
    Email: email,
    NewPassword: newPassword,
    ConfirmPassword: confirmPassword
  });
  return data;
}

// SUBSCRIBE
export async function subscribeApi({ intersection, metric }) {
  const { data } = await api.post("/api/users/subscriptions/subscribe", {
    Intersection: intersection,
    Metric: metric
  });
  return data;
}

// LOGOUT
export async function logoutApi() {
  await api.post("/api/users/logout");
  localStorage.removeItem("stls_token");
  localStorage.removeItem("stls_token_exp");
}
