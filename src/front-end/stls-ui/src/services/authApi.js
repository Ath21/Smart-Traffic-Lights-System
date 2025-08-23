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
// Login user
export async function loginApi({ email, password }) {
  const response = await api.post('/login', { email, password })

  // backend sends { Token, ExpiresAt }
  return {
    token: response.data.Token,        // the real JWT
    expiresAt: response.data.ExpiresAt // optional expiry
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

// Get profile
export async function getProfile(token) {
  const response = await api.get('/profile', {
    headers: { Authorization: `Bearer ${token}` },
  })
  return response.data
}
