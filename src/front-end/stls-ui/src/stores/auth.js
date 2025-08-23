// src/stores/auth.js
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { getProfile } from '../services/authApi'

function parseJwt(token) {
  try {
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    return JSON.parse(atob(base64))
  } catch (e) {
    console.error("❌ Failed to decode JWT", e)
    return null
  }
}

export const useAuth = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || null)
  const expiresAt = ref(localStorage.getItem('expiresAt') || null)
  const user = ref(null)
  let logoutTimer = null

  const isAuthenticated = computed(() => !!token.value)

  async function login(newToken, newExpiresAt) {
    token.value = newToken
    expiresAt.value = newExpiresAt

    localStorage.setItem('token', newToken)
    localStorage.setItem('expiresAt', newExpiresAt)

    const decoded = parseJwt(newToken)
    if (decoded) {
      console.log("✅ Decoded JWT payload:", decoded)
      console.log("   userId:", decoded.sub)
      console.log("   email:", decoded.email)
      console.log("   username:", decoded.username || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"])
      console.log("   role:", decoded.role || decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"])
      console.log("   status:", decoded.status)
      console.log("   exp:", decoded.exp ? new Date(decoded.exp * 1000).toISOString() : "(no exp claim)")

      user.value = {
        userId: decoded.sub || null,
        email: decoded.email || null,
        username: decoded.username || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || null,
        role: decoded.role || decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || null,
        status: decoded.status || null
      }
    }

    startLogoutTimer()
    await fetchProfile()
    return user.value?.role?.toLowerCase() || 'viewer'
  }

  function logout() {
    token.value = null
    expiresAt.value = null
    user.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('expiresAt')
    if (logoutTimer) {
      clearTimeout(logoutTimer)
      logoutTimer = null
    }
    console.log("👋 Logged out")
  }

  async function fetchProfile() {
    if (!token.value) return
    try {
      const profile = await getProfile(token.value)
      if (profile) {
        console.log("🌐 Profile from server:", profile)
        user.value = profile
      }
    } catch (err) {
      console.error('❌ Failed to fetch profile', err)
      logout()
    }
  }

  function startLogoutTimer() {
    if (!expiresAt.value) return
    if (logoutTimer) clearTimeout(logoutTimer)

    const expiryTime = new Date(expiresAt.value).getTime()
    const now = Date.now()
    const timeout = expiryTime - now

    if (timeout > 0) {
      console.log(`⏳ Auto-logout scheduled in ${Math.round(timeout / 1000)}s`)
      logoutTimer = setTimeout(() => {
        console.warn('⚠️ Token expired — auto-logout triggered')
        logout()
      }, timeout)
    } else {
      console.warn("⚠️ Token already expired")
      logout()
    }
  }

  async function bootstrap() {
    if (token.value) {
      const decoded = parseJwt(token.value)
      if (decoded) {
        console.log("🔄 Bootstrap decode:", decoded)
        console.log("   userId:", decoded.sub)
        console.log("   email:", decoded.email)
        console.log("   username:", decoded.username || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"])
        console.log("   role:", decoded.role || decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"])
        console.log("   status:", decoded.status)
        console.log("   exp:", decoded.exp ? new Date(decoded.exp * 1000).toISOString() : "(no exp claim)")

        user.value = {
          userId: decoded.sub || null,
          email: decoded.email || null,
          username: decoded.username || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || null,
          role: decoded.role || decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || null,
          status: decoded.status || null
        }
      }
      startLogoutTimer()
      await fetchProfile()
    }
  }

  return { token, expiresAt, user, login, logout, fetchProfile, bootstrap, isAuthenticated }
})
