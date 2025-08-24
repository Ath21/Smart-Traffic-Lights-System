<template>
  <header class="topbar">
    <!-- Brand block: logos + title -->
    <div class="brand-block">
      <img src="/PADA.png" alt="PADA Logo" class="logo" />
      <h1>UNIWA <span>STLS</span></h1>
      <img src="/SmartCityLog.png" alt="Smart City Logo" class="logo" />
    </div>

    <!-- Right side -->
    <div class="flex items-center gap-3 relative">
      <!-- Case 1: Login/Register pages → show Home -->
      <RouterLink
        v-if="isLoginOrRegister"
        :to="homePath"
        class="home-btn"
      >
        Home
      </RouterLink>

      <!-- Case 2: Not logged in (but not login/register) → show Login/Register -->
      <nav v-else-if="!auth.isAuthenticated" class="flex gap-2">
        <RouterLink class="login" to="/login">Login</RouterLink>
        <RouterLink class="register" to="/register">Register</RouterLink>
      </nav>

      <!-- Case 3: Logged in → Alert Me + dropdown -->
      <div v-else class="flex items-center gap-3 relative">
        <button class="login" @click="alertMe">
          Alert Me
        </button>

        <div class="relative" ref="menuRef">
          <button class="user-btn" @click="toggleMenu">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              class="h-6 w-6 text-gray-200"
              fill="currentColor"
              viewBox="0 0 20 20"
            >
              <path d="M10 10a4 4 0 100-8 4 4 0 000 8zm-7 8a7 7 0 1114 0H3z" />
            </svg>
          </button>

          <!-- Dropdown -->
          <div v-if="showMenu" class="dropdown">
            <div class="dropdown-header">
              {{ auth.user?.email }}
            </div>
            <RouterLink to="/account" class="dropdown-item">Account</RouterLink>
            <RouterLink to="/notifications" class="dropdown-item">Notification Status</RouterLink>
            <RouterLink to="/update" class="dropdown-item">Update Profile</RouterLink>
            <button @click="logout" class="dropdown-item logout">Logout</button>
          </div>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { useAuth } from '../stores/auth'
import { useRouter, useRoute } from 'vue-router'
import '../assets/topbar.css'

const auth = useAuth()
const router = useRouter()
const route = useRoute()

const showMenu = ref(false)
const menuRef = ref(null)

// Show Home button only on login/register pages
const isLoginOrRegister = computed(() =>
  ['/login', '/register'].includes(route.path)
)

// Home path depends on user role
const homePath = computed(() =>
  auth.user?.role?.toLowerCase() === 'user' ? '/app' : '/'
)

function toggleMenu() {
  showMenu.value = !showMenu.value
}

function handleClickOutside(e) {
  if (menuRef.value && !menuRef.value.contains(e.target)) {
    showMenu.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})
onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside)
})

async function alertMe() {
  try {
    const res = await auth.apiFetch("http://localhost:5055/api/users/send-notification", {
      method: "POST",
      body: JSON.stringify({
        Message: "User subscribed to traffic alerts",
        Type: "Traffic"
      })
    })
    if (!res.ok) throw new Error(`HTTP ${res.status}`)
    await res.json()
    alert("✅ Notification request sent!")
  } catch (err) {
    console.error("❌ Failed to send alert:", err)
    alert("❌ Failed to send notification")
  }
}

function logout() {
  auth.logout()
  router.push('/login')
}
</script>
