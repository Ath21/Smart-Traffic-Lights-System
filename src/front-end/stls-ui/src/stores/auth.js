// src/stores/auth.js
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { getProfile } from '../services/authApi'

export const useAuth = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || null)
  const user = ref(null) // { userId, username, email, role, status }

  const isAuthenticated = computed(() => !!token.value)

  async function login(newToken) {
    token.value = newToken
    localStorage.setItem('token', newToken)
    await fetchProfile()
    return user.value?.role?.toLowerCase() || 'viewer'
  }

  function logout() {
    token.value = null
    user.value = null
    localStorage.removeItem('token')
  }

  async function fetchProfile() {
    if (!token.value) return
    try {
      const profile = await getProfile(token.value)
      user.value = profile
    } catch (err) {
      console.error('❌ Failed to fetch profile', err)
      logout()
    }
  }

  async function bootstrap() {
    if (token.value && !user.value) {
      await fetchProfile()
    }
  }

  return { token, user, login, logout, fetchProfile, bootstrap, isAuthenticated }
})
