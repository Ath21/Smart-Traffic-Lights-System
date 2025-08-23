// src/services/authApi.js
import axios from 'axios'

const api = axios.create({
  baseURL: 'http://localhost:5055/api/users', // backend base URL
})

// Register a new user
export async function registerApi({ email, username, password }) {
  const response = await api.post('/register', { email, username, password })
  return response.data
}

// Login user
export async function loginApi({ username, password }) {
  const response = await api.post('/login', { username, password })
  return response.data
}

// Logout user
export async function logoutApi(token) {
  await api.post(
    '/logout',
    {},
    { headers: { Authorization: `Bearer ${token}` } }
  )
}

// Get profile
export async function getProfile(token) {
  const response = await api.get('/profile', {
    headers: { Authorization: `Bearer ${token}` },
  })
  return response.data
}
