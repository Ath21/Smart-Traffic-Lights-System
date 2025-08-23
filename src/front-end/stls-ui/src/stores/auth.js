import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

// helper to decode JWT payload
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
      user.value = {
        userId: decoded.sub,
        email: decoded.email,
        username: decoded.username || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
        role: decoded.role || decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"],
        status: decoded.status
      }
    }

    startLogoutTimer()
    return user.value?.role?.toLowerCase() || 'viewer'
  }

  function logout() {
    token.value = null
    expiresAt.value = null
    user.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('expiresAt')
    if (logoutTimer) clearTimeout(logoutTimer)
    console.log("👋 Logged out")
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
      logout()
    }
  }

  async function bootstrap() {
    if (token.value && !user.value) {
      const decoded = parseJwt(token.value)
      if (decoded) {
        console.log("🔄 Bootstrap decode:", decoded)
        user.value = {
          userId: decoded.sub,
          email: decoded.email,
          username: decoded.username || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
          role: decoded.role || decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"],
          status: decoded.status
        }
      }
      startLogoutTimer()
    }
  }

  /**
   * Centralized API fetch with Authorization header
   */
  async function apiFetch(url, options = {}) {
    if (!token.value) {
      throw new Error("No auth token available")
    }

    const res = await fetch(url, {
      ...options,
      headers: {
        ...(options.headers || {}),
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token.value}`
      }
    })

    if (res.status === 401) {
      console.warn("⚠️ Unauthorized, logging out...")
      logout()
    }

    return res
  }

  return { token, expiresAt, user, login, logout, bootstrap, isAuthenticated, apiFetch }
})
