import { defineStore } from "pinia";
import { ref, computed } from "vue";

// ===============================
// JWT Decoder (handles malformed/padded tokens)
// ===============================
function decodeJWT(token) {
  try {
    const base64 = token.split(".")[1];
    if (!base64) return null;

    // Fix base64 padding
    const padded = base64.replace(/-/g, "+").replace(/_/g, "/");
    const payload = JSON.parse(
      decodeURIComponent(
        atob(padded)
          .split("")
          .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
          .join("")
      )
    );
    return payload;
  } catch (err) {
    console.warn("[Auth] Failed to decode JWT:", err);
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
  const ready = ref(false);

  // ===============================
  // Derived
  // ===============================
  const isAuthenticated = computed(() => !!token.value && !!user.value);

  // ===============================
  // Bootstrap from localStorage
  // ===============================
  function bootstrap() {
    const saved = localStorage.getItem("jwt");
    if (!saved) {
      ready.value = true;
      return;
    }

    const decoded = decodeJWT(saved);
    if (!decoded) {
      localStorage.removeItem("jwt");
      ready.value = true;
      return;
    }

    // Normalize common JWT claim names
    user.value = {
      id: decoded.sub,
      email:
        decoded.email ||
        decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] ||
        null,
      username:
        decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ||
        decoded.username ||
        decoded.email,
      role:
        decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
        decoded.role ||
        "User",
      status: decoded.status || "active",
      exp: decoded.exp,
    };

    token.value = saved;
    ready.value = true;
  }

  // ===============================
  // Auth Actions
  // ===============================
  function login(newToken) {
    token.value = newToken;
    localStorage.setItem("jwt", newToken);

    const decoded = decodeJWT(newToken);
    if (decoded) {
      user.value = {
        id: decoded.sub,
        email:
          decoded.email ||
          decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] ||
          null,
        username:
          decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ||
          decoded.username ||
          decoded.email,
        role:
          decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
          decoded.role ||
          "User",
        status: decoded.status || "active",
        exp: decoded.exp,
      };
    }
  }

  function logout() {
    token.value = null;
    user.value = null;
    localStorage.removeItem("jwt");
  }

  // ===============================
  // Helper for authorized fetch
  // ===============================
  async function apiFetch(url, options = {}) {
    const headers = options.headers || {};
    if (token.value) headers.Authorization = `Bearer ${token.value}`;
    return fetch(url, { ...options, headers });
  }

  // ===============================
  // Init
  // ===============================
  bootstrap();

  return {
    token,
    user,
    loading,
    ready,
    isAuthenticated,
    bootstrap,
    login,
    logout,
    apiFetch,
  };
});
