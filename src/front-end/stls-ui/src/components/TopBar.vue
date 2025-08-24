<template>
  <header class="topbar">
    <!-- Brand block: logos + title -->
    <div class="brand-block">
      <img src="/PADA.png" alt="PADA Logo" class="logo" />
      <h1>UNIWA <span>STLS</span></h1>
      <img src="/SmartCityLog.png" alt="Smart City Logo" class="logo" />
    </div>

    <!-- Right side -->
    <!-- Case 1: Auth/account-related pages -->
    <div v-if="isAuthPage" class="right-nav">
      <RouterLink class="home-btn" :to="homePath">Home</RouterLink>
    </div>

    <!-- Case 2: Normal topbar -->
    <template v-else>
      <nav v-if="!auth.isAuthenticated" class="flex gap-2">
        <RouterLink class="login" to="/login">Login</RouterLink>
        <RouterLink class="register" to="/register">Register</RouterLink>
      </nav>

      <div v-else class="flex items-center gap-3 relative">
        <!-- Alert Me button -->
        <button class="login" @click="alertMe">
          Alert Me
        </button>

        <!-- User menu toggle -->
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
            <RouterLink to="/settings" class="dropdown-item">Settings</RouterLink>
            <button @click="logout" class="dropdown-item logout">Logout</button>
          </div>
        </div>
      </div>
    </template>
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

// --- Detect if current page is auth/account related ---
const isAuthPage = computed(() =>
  ['/login', '/register', '/reset-password', '/account'].includes(route.path)
)

// --- Home path depends on user role ---
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

function alertMe() {
  alert('Alerts enabled for incidents (demo).')
}

function logout() {
  auth.logout()
  router.push('/login')
}
</script>
