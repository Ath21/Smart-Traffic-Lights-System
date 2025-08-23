import { createRouter, createWebHistory } from 'vue-router'
import { useAuth } from '../stores/auth'

const ViewerLayout = () => import('../layouts/ViewerLayout.vue')
const UserLayout = () => import('../layouts/UserLayout.vue')
const OperatorLayout = () => import('../layouts/OperatorLayout.vue')
const AdminLayout = () => import('../layouts/AdminLayout.vue')

const Login = () => import('../pages/Login.vue')
const Register = () => import('../pages/Register.vue')

const ROLE_ORDER = ['viewer', 'user', 'operator', 'admin']
function canAccess(requiredRole, userRole) {
  return ROLE_ORDER.indexOf(userRole) >= ROLE_ORDER.indexOf(requiredRole)
}

const routes = [
  { path: '/login', name: 'login', component: Login, meta: { public: true } },
  { path: '/register', name: 'register', component: Register, meta: { public: true } },

  { path: '/', name: 'viewer', component: ViewerLayout, meta: { role: 'viewer' } },
  { path: '/app', name: 'user', component: UserLayout, meta: { role: 'user' } },
  { path: '/operator', name: 'operator', component: OperatorLayout, meta: { role: 'operator' } },
  { path: '/admin', name: 'admin', component: AdminLayout, meta: { role: 'admin' } },
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
