// src/services/authApi.js
import axios from 'axios'

const api = axios.create({
  baseURL: 'http://localhost:5055/api/users',
})

// Register a new user
export async function registerApi({ email, username, password }) {
  const response = await api.post('/register', { email, username, password })
  return response.data
}

// Login user
export async function loginApi({ email, password }) {
  const response = await api.post('/login', { email, password })
  return {
    token: response.data.Token,        // full JWT from server
    expiresAt: response.data.ExpiresAt // expiry timestamp
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
export async function resetPasswordApi(email, newPassword) {
  const response = await api.post(
    '/reset-password',
    {
      UsernameOrEmail: email,
      NewPassword: newPassword,
    }
  )
  return response.data
}

// 📝 Update profile API
export async function updateProfileApi(token, { email, username, password, status, role }) {
  const payload = {
    Username: username,
    Email: email,
    Password: password || '',
    Status: status || 'active',
    Role: role || 'user'
  }

  const response = await api.put(
    '/update',
    payload,
    { headers: { Authorization: `Bearer ${token}` } }
  )

  return response.data
}


// 🚨 Send Notification
export async function sendNotificationApi(token, { message, type }) {
  const response = await api.post(
    '/send-notification',
    {
      Message: message,
      Type: type,
    },
    {
      headers: { Authorization: `Bearer ${token}` },
    }
  )
  return response.data
}