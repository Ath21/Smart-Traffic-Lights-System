// src/services/notificationApi.js
import axios from 'axios'

const notificationApi = axios.create({
  baseURL: 'http://localhost:5087/api/notifications',
})

// 🚨 Send User Notification
export async function sendNotificationApi(token, { userId, recipientEmail, message, type }) {
  const response = await notificationApi.post(
    'send',
    { UserId: userId, RecipientEmail: recipientEmail, Message: message, Type: type },
    { headers: { Authorization: `Bearer ${token}` } }
  )
  return response.data
}

// 📜 Get user notification history
export async function getUserNotificationsApi(token, email) {
  const response = await notificationApi.get(`recipient/${email}`, {
    headers: { Authorization: `Bearer ${token}` },
  })
  return response.data
}

// ✅ Mark one as read
export async function markAsReadApi(token, notificationId) {
  const response = await notificationApi.patch(
    `${notificationId}/read`, // ✅ no leading slash
    {},
    { headers: { Authorization: `Bearer ${token}` } }
  )
  return response.data
}

// ✅ Mark all as read
export async function markAllAsReadApi(token) {
  const response = await notificationApi.patch(
    'read-all', // ✅ no leading slash
    {},
    { headers: { Authorization: `Bearer ${token}` } }
  )
  return response.data
}

// 📢 Send public notice
export async function sendPublicNoticeApi(token, { title, message, targetAudience }) {
  const response = await notificationApi.post(
    'public-notice',
    { Title: title, Message: message, TargetAudience: targetAudience },
    { headers: { Authorization: `Bearer ${token}` } }
  )
  return response.data
}
