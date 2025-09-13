// router/index.js
import { createRouter, createWebHistory } from 'vue-router'
import { useAuth } from '../stores/users'

// Layouts
const GuestLayout = () => import('../layouts/GuestLayout.vue')
const UserLayout = () => import('../layouts/UserLayout.vue')
const TrafficOperatorLayout = () => import('../layouts/TrafficOperatorLayout.vue')
const AdminLayout = () => import('../layouts/AdminLayout.vue')

// Auth pages
const Login = () => import('../pages/Guest/Login.vue')
const Register = () => import('../pages/Guest/Register.vue')
const ResetPassword = () => import('../pages/Guest/ResetPassword.vue')

// User pages
const Profile = () => import('../pages/User/Profile.vue')
const UpdateProfile = () => import('../pages/User/UpdateProfile.vue')
const Notifications = () => import('../pages/User/Notification.vue')

// Traffic Operator pages
const TrafficOperatorDetections = () => import('../pages/TrafficOperator/Detections.vue')
const TrafficOperatorSensors = () => import('../pages/TrafficOperator/Sensors.vue')

// Admin pages
const AdminLogs = () => import('../pages/Admin/Logs.vue')
const AdminReports = () => import('../pages/Admin/Reports.vue')
const AdminConfig = () => import('../pages/Admin/Config.vue')

// Guest pages
const GuestCongestion = () => import('../pages/Guest/Congestion.vue')

const ROLE_ORDER = ['guest', 'user', 'traffic_operator', 'admin']
function canAccess(requiredRole, userRole) {
  return ROLE_ORDER.indexOf(userRole) >= ROLE_ORDER.indexOf(requiredRole)
}

const routes = [
  { path: '/login', name: 'login', component: Login, meta: { public: true } },
  { path: '/register', name: 'register', component: Register, meta: { public: true } },
  { path: '/reset-password', name: 'reset-password', component: ResetPassword, meta: { public: true } },

  // Guest
  { path: '/', name: 'guest', component: GuestLayout, meta: { public: true }, children: [
    { path: '', name: 'guest-congestion', component: GuestCongestion }
  ]},

  // User
  { path: '/stls', name: 'user', component: UserLayout, meta: { role: 'user' }, children: [
    { path: 'profile', name: 'profile', component: Profile },
    { path: 'update', name: 'update', component: UpdateProfile },
    { path: 'notifications', name: 'notifications', component: Notifications }
  ]},

  // Traffic Operator
  { path: '/operator', name: 'operator', component: TrafficOperatorLayout, meta: { role: 'traffic_operator' }, children: [
    { path: 'detections', name: 'detections', component: TrafficOperatorDetections },
    { path: 'sensors', name: 'sensors', component: TrafficOperatorSensors }
  ]},

  // Admin
  { path: '/admin', name: 'admin', component: AdminLayout, meta: { role: 'admin' }, children: [
    { path: 'logs', name: 'logs', component: AdminLogs },
    { path: 'reports', name: 'reports', component: AdminReports },
    { path: 'config', name: 'config', component: AdminConfig }
  ]}
]

const router = createRouter({ history: createWebHistory(), routes })

router.beforeEach(async (to) => {
  const auth = useAuth()
  if (!auth.user && !auth.token) {
    await auth.bootstrap()
  }

  if (to.meta?.public) return true

  const required = to.meta?.role || 'guest'
  const role = auth.user?.role?.toLowerCase() || 'guest'

  if (!canAccess(required, role)) {
    return auth.isAuthenticated
      ? { name: 'guest' }
      : { name: 'login', query: { redirect: to.fullPath } }
  }

  return true
})

export default router
