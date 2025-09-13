// router/index.js
import { createRouter, createWebHistory } from 'vue-router'
import { useAuth } from '../stores/users'

// Layouts
const ViewerLayout = () => import('../layouts/ViewerLayout.vue')
const UserLayout = () => import('../layouts/UserLayout.vue')
const OperatorLayout = () => import('../layouts/OperatorLayout.vue')
const AdminLayout = () => import('../layouts/AdminLayout.vue')

// Auth pages
const Login = () => import('../pages/Login.vue')
const Register = () => import('../pages/Register.vue')
const ResetPassword = () => import('../pages/ResetPassword.vue')

// User pages
const Profile = () => import('../pages/Profile.vue')
const UpdateProfile = () => import('../pages/UpdateProfile.vue')
const Notifications = () => import('../pages/Notification.vue')

// Operator pages
const OperatorDetections = () => import('../pages/Operator/Detections.vue')
const OperatorSensors = () => import('../pages/Operator/Sensors.vue')

// Admin pages
const AdminLogs = () => import('../pages/Admin/Logs.vue')
const AdminReports = () => import('../pages/Admin/Reports.vue')
const AdminConfig = () => import('../pages/Admin/Config.vue')

// Viewer pages
const ViewerCongestion = () => import('../pages/Viewer/Congestion.vue')

const ROLE_ORDER = ['viewer', 'user', 'operator', 'admin']
function canAccess(requiredRole, userRole) {
  return ROLE_ORDER.indexOf(userRole) >= ROLE_ORDER.indexOf(requiredRole)
}

const routes = [
  { path: '/login', name: 'login', component: Login, meta: { public: true } },
  { path: '/register', name: 'register', component: Register, meta: { public: true } },
  { path: '/reset-password', name: 'reset-password', component: ResetPassword, meta: { public: true } },

  // Viewer
  { path: '/', name: 'viewer', component: ViewerLayout, meta: { public: true }, children: [
    { path: '', name: 'viewer-congestion', component: ViewerCongestion }
  ]},

  // User
  { path: '/app', name: 'user', component: UserLayout, meta: { role: 'user' }, children: [
    { path: 'profile', name: 'profile', component: Profile },
    { path: 'update', name: 'update', component: UpdateProfile },
    { path: 'notifications', name: 'notifications', component: Notifications }
  ]},

  // Operator
  { path: '/operator', name: 'operator', component: OperatorLayout, meta: { role: 'operator' }, children: [
    { path: 'detections', name: 'detections', component: OperatorDetections },
    { path: 'sensors', name: 'sensors', component: OperatorSensors }
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

  const required = to.meta?.role || 'viewer'
  const role = auth.user?.role?.toLowerCase() || 'viewer'

  if (!canAccess(required, role)) {
    return auth.isAuthenticated
      ? { name: 'viewer' }
      : { name: 'login', query: { redirect: to.fullPath } }
  }

  return true
})

export default router
