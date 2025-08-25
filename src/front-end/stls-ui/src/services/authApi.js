// src/services/authApi.js
import axios from 'axios'

// === USERS API ===
const api = axios.create({
  baseURL: 'http://localhost:5055/api/users',
})

// === NOTIFICATIONS API ===
const notificationApi = axios.create({
  baseURL: 'http://localhost:5055/api/notifications',
})

// ==================== USERS ====================

// Register a new user
export async function registerApi({ email, username, password, confirmPassword }) {
  const response = await api.post('/register', { 
    email, 
    username, 
    password, 
    confirmPassword    // ✅ add if backend requires it
  })
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
export async function resetPasswordApi(email, newPassword, confirmPassword) {
  const response = await api.post(
    '/reset-password',
    {
      UsernameOrEmail: email,
      NewPassword: newPassword,
      ConfirmPassword: confirmPassword,   // ✅ added
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

// ==================== NOTIFICATIONS ====================

// 🚨 Send User Notification
export async function sendNotificationApi(token, { userId, recipientEmail, message, type }) {
  const response = await notificationApi.post(
    '/send',
    {
      UserId: userId,
      RecipientEmail: recipientEmail,
      Message: message,
      Type: type,
    },
    {
      headers: { Authorization: `Bearer ${token}` },
    }
  )
  return response.data
}

// 📜 Get user notification history
export async function getUserNotificationsApi(token, userId) {
  const response = await notificationApi.get(`/history/${userId}`, {
    headers: { Authorization: `Bearer ${token}` }
  })
  return response.data
}

// 📢 Send a public notice (admin use)
export async function sendPublicNoticeApi(token, { title, message, targetAudience }) {
  const response = await notificationApi.post(
    '/public-notice',
    {
      Title: title,
      Message: message,
      TargetAudience: targetAudience,
    },
    {
      headers: { Authorization: `Bearer ${token}` },
    }
  )
  return response.data
}
