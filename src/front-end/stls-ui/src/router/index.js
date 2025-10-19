import { createRouter, createWebHistory } from 'vue-router'
import { useAuth } from '../stores/userStore'

// === Pages ===

// Auth
const Login = () => import('../pages/User/Login.vue')
const Register = () => import('../pages/User/Register.vue')
const ResetPassword = () => import('../pages/User/ResetPassword.vue')

// User area
const Profile = () => import('../pages/User/Profile.vue')
const UpdateProfile = () => import('../pages/User/UpdateProfile.vue')
const Notifications = () => import('../pages/User/Notifications.vue')

// Role hierarchy (guest < user < traffic_operator < admin)
const ROLE_ORDER = ['guest', 'user', 'traffic_operator', 'admin']
function canAccess(requiredRole, userRole) {
  return ROLE_ORDER.indexOf(userRole) >= ROLE_ORDER.indexOf(requiredRole)
}

const routes = [
  // Public auth routes
  { path: '/login', name: 'login', component: Login, meta: { public: true } },
  { path: '/register', name: 'register', component: Register, meta: { public: true } },
  { path: '/reset-password', name: 'reset-password', component: ResetPassword, meta: { public: true } },

  // Core user routes
  { path: '/', redirect: '/profile' },
  { path: '/profile', name: 'profile', component: Profile, meta: { role: 'user' } },
  { path: '/update', name: 'update', component: UpdateProfile, meta: { role: 'user' } },
  { path: '/notifications', name: 'notifications', component: Notifications, meta: { role: 'user' } },

  // Catch-all fallback
  { path: '/:pathMatch(.*)*', redirect: '/login' }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// === Global auth guard ===
router.beforeEach(async (to) => {
  const auth = useAuth()

  // Initialize store if needed
  if (!auth.user && !auth.token && auth.bootstrap) {
    await auth.bootstrap()
  }

  // Public route? allow
  if (to.meta?.public) return true

  // Determine access
  const required = to.meta?.role || 'guest'
  const role = auth.user?.role?.toLowerCase() || 'guest'

  if (!canAccess(required, role)) {
    return auth.token
      ? { name: 'profile' } // logged in but not enough rights
      : { name: 'login', query: { redirect: to.fullPath } }
  }

  return true
})

export default router
