import { defineStore } from 'pinia'
import { tokenService, parseToken } from '../services/tokenService'


export const useAuth = defineStore('auth', {
state: () => ({ token: tokenService.get(), user: null }),
getters: {
role: (s) => s.user?.role ?? 'viewer',
isAuthenticated: (s) => !!s.token,
},
actions: {
bootstrap() {
const token = tokenService.get()
if (token) {
this.token = token
this.user = parseToken(token)
}
},
login(token) {
this.token = token
tokenService.set(token)
this.user = parseToken(token)
},
logout() {
this.token = null
this.user = null
tokenService.clear()
}
}
})