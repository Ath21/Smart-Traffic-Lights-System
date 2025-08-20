<script setup>
import { ref } from 'vue'
import { loginApi } from '../services/fakeAuthApi'
import { useAuth } from '../stores/auth'
import { useRoute, useRouter } from 'vue-router'

const email = ref('')
const password = ref('')
const loading = ref(false)
const auth = useAuth()
const route = useRoute()
const router = useRouter()

async function submit() {
  loading.value = true
  const { token } = await loginApi({ email: email.value, password: password.value })
  auth.login(token)
  const payload = JSON.parse(atob(token))
  const redirect = route.query.redirect
  if (redirect) router.push(String(redirect))
  else if (payload.role === 'admin') router.push('/admin')
  else if (payload.role === 'operator') router.push('/operator')
  else if (payload.role === 'user') router.push('/app')
  else router.push('/')
  loading.value = false
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center p-4 bg-gray-50">
    <form @submit.prevent="submit" class="w-full max-w-sm bg-white p-6 rounded-2xl shadow space-y-4">
      <h1 class="text-2xl font-bold mb-2">Login</h1>
      <div>
        <label class="block text-sm font-medium mb-1">Email</label>
        <input v-model="email" type="email" class="w-full border rounded-lg p-2 focus:ring-2 focus:ring-blue-500 focus:outline-none" />
      </div>
      <div>
        <label class="block text-sm font-medium mb-1">Password</label>
        <input v-model="password" type="password" class="w-full border rounded-lg p-2 focus:ring-2 focus:ring-blue-500 focus:outline-none" />
      </div>
      <button :disabled="loading" class="w-full py-2 rounded-lg bg-blue-600 text-white font-medium hover:bg-blue-700 transition">{{ loading ? '...' : 'Login' }}</button>
      <RouterLink class="block text-center text-sm text-blue-600 hover:underline" to="/register">Create an account</RouterLink>
    </form>
  </div>
</template>