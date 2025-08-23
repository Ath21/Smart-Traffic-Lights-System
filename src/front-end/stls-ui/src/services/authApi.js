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
