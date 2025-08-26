// src/services/notificationApi.js
import axios from 'axios'

const notificationApi = axios.create({
  baseURL: 'http://localhost:5055/api/notifications',
})

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
    headers: { Authorization: `Bearer ${token}` },
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
