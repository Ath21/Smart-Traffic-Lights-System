import { createRouter, createWebHistory } from 'vue-router'
import { useAuth } from '../stores/auth'



// Lazy imports
const ViewerLayout = () => import('../layouts/ViewerLayout.vue')
const UserLayout = () => import('../layouts/UserLayout.vue')
const OperatorLayout = () => import('../layouts/OperatorLayout.vue')
const AdminLayout = () => import('../layouts/AdminLayout.vue')


const Login = () => import('../pages/Login.vue')
const Register = () => import('../pages/Register.vue')


// Role helper
const ROLE_ORDER = ['viewer','user','operator','admin']
function canAccess(requiredRole, userRole) {
return ROLE_ORDER.indexOf(userRole) >= ROLE_ORDER.indexOf(requiredRole)
}


const routes = [
// Public auth pages
{ path: '/login', name: 'login', component: Login, meta: { public: true } },
{ path: '/register', name: 'register', component: Register, meta: { public: true } },


// Viewer (public map + login/register buttons)
{ path: '/', name: 'viewer', component: ViewerLayout, meta: { role: 'viewer' } },


// User
{ path: '/app', name: 'user', component: UserLayout, meta: { role: 'user' } },


// Traffic Operator
{ path: '/operator', name: 'operator', component: OperatorLayout, meta: { role: 'operator' } },


// Admin
{ path: '/admin', name: 'admin', component: AdminLayout, meta: { role: 'admin' } },
]


const router = createRouter({ history: createWebHistory(), routes })


router.beforeEach((to) => {
const auth = useAuth()
if (!auth.user && auth.token == null) auth.bootstrap()


// Allow public routes
if (to.meta?.public) return true


// Default required role is 'viewer'
const required = to.meta?.role || 'viewer'
const role = auth.role


if (!canAccess(required, role)) {
// If not logged in, go to login; if logged in but insufficient role, send to home
return auth.isAuthenticated ? { name: 'viewer' } : { name: 'login', query: { redirect: to.fullPath } }
}


return true
})


export default router