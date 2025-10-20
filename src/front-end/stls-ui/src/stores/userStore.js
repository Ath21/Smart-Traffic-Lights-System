import { defineStore } from "pinia";
import { ref, computed } from "vue";

// Lightweight decoder (no external lib)
function decodeJWT(token) {
  try {
    const payload = token.split(".")[1];
    const decoded = JSON.parse(atob(payload));
    return decoded;
  } catch {
    return null;
  }
}

export const useAuth = defineStore("auth", () => {
  // ===============================
  // State
  // ===============================
  const token = ref(localStorage.getItem("jwt") || null);
  const user = ref(null);
  const loading = ref(false);

  // ===============================
  // Derived values
  // ===============================
  const isAuthenticated = computed(() => !!token.value);

  // ===============================
  // Bootstrap from localStorage
  // ===============================
  function bootstrap() {
    const saved = localStorage.getItem("jwt");
    if (saved) {
      const decoded = decodeJWT(saved);
      if (decoded) {
        user.value = {
          id: decoded.sub,
          email: decoded.email,
          username:
            decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ||
            decoded.email,
          role:
            decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
            "User",
          status: decoded.status || "active",
          exp: decoded.exp,
        };
        token.value = saved;
      }
    }
  }

  // ===============================
  // Login / Register / Logout
  // ===============================
  function login(newToken) {
    token.value = newToken;
    localStorage.setItem("jwt", newToken);

    const decoded = decodeJWT(newToken);
    if (decoded) {
      user.value = {
        id: decoded.sub,
        email: decoded.email,
        username:
          decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ||
          decoded.email,
        role:
          decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
          "User",
        status: decoded.status || "active",
      };
    }
  }

  function logout() {
    token.value = null;
    user.value = null;
    localStorage.removeItem("jwt");
  }

  // ===============================
  // API Fetch Helper
  // ===============================
  async function apiFetch(url, options = {}) {
    const headers = options.headers || {};
    if (token.value) {
      headers.Authorization = `Bearer ${token.value}`;
    }
    return fetch(url, { ...options, headers });
  }

  // Initialize on store load
  bootstrap();

  return {
    token,
    user,
    loading,
    isAuthenticated,
    bootstrap,
    login,
    logout,
    apiFetch,
  };
});
