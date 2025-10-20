// src/router/index.js
import { createRouter, createWebHistory } from 'vue-router'
import { useAuth } from '../stores/userStore'

// === Pages ===
const MapView = () => import('../components/MapView.vue')
const Login = () => import('../pages/User/Login.vue')
const Register = () => import('../pages/User/Register.vue')
const ResetPassword = () => import('../pages/User/ResetPassword.vue')
const Profile = () => import('../pages/User/Profile.vue')
const UpdateProfile = () => import('../pages/User/UpdateProfile.vue')
const Notifications = () => import('../pages/User/Notifications.vue')

// === Routes ===
const routes = [
  // Public Home (Map)
  { path: '/', name: 'home', component: MapView, meta: { public: true } },

  // Auth routes
  { path: '/login', name: 'login', component: Login, meta: { public: true } },
  { path: '/register', name: 'register', component: Register, meta: { public: true } },
  { path: '/reset-password', name: 'reset-password', component: ResetPassword, meta: { public: true } },

  // Authenticated User routes
  { path: '/stls', name: 'stls', component: MapView, meta: { role: 'user' } },
  { path: '/stls/profile', name: 'profile', component: Profile, meta: { role: 'user' } },
  { path: '/stls/update', name: 'update', component: UpdateProfile, meta: { role: 'user' } },
  { path: '/stls/notifications', name: 'notifications', component: Notifications, meta: { role: 'user' } },

  // Fallback
  { path: '/:pathMatch(.*)*', redirect: '/' }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// === Navigation Guard ===
router.beforeEach(async (to) => {
  const auth = useAuth()

  // Initialize once
  if (!auth.isInitialized) {
    await auth.bootstrap()
    auth.isInitialized = true
  }

  const isAuthenticated = !!auth.token
  const role = auth.user?.role?.toLowerCase() || 'guest'

  // Public route logic
  if (to.meta?.public) {
    // Avoid redirect loop
    if (isAuthenticated && ['home', 'login', 'register', 'reset-password'].includes(to.name)) {
      return { name: 'stls' }
    }
    return true
  }

  // Protected route
  if (!isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  // Everything else allowed
  return true
})

export default router
