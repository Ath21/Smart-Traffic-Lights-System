// src/services/userApi.js
import axios from 'axios'

// === USERS API ===
const api = axios.create({
  baseURL: 'http://localhost:5055/api/users',
})

// ==================== USERS ====================

// Register a new user
export async function registerApi({ email, username, password, confirmPassword }) {
  const response = await api.post('/register', {
    email,
    username,
    password,
    confirmPassword,
  })
  return response.data
}

// Login user
export async function loginApi({ email, password }) {
  const response = await api.post('/login', { email, password })
  return {
    token: response.data.token,
    expiresAt: response.data.expiresAt,
  }
}

// Logout user
export async function logoutApi(token) {
  await api.post(
    '/logout',
    {},
    { headers: { Authorization: `Bearer ${token}` } }
  )
}

// 🔑 Reset password API
export async function resetPasswordApi(email, newPassword, confirmPassword) {
  const response = await api.post('/reset-password', {
    UsernameOrEmail: email,
    NewPassword: newPassword,
    ConfirmPassword: confirmPassword,
  })
  return response.data
}

// 📝 Update profile API
export async function updateProfileApi(token, { email, username, password, confirmPassword, status, role }) {
  const payload = {
    Username: username,
    Email: email,
    Password: password || '',
    ConfirmPassword: confirmPassword || '',
    Status: status || 'active',
    Role: role || 'user',
  }

  const response = await api.put('/update', payload, {
    headers: { Authorization: `Bearer ${token}` },
  })

  return response.data
}
