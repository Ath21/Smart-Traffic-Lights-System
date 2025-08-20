<script setup>
import { ref } from 'vue'
import { loginApi } from '../services/fakeAuthApi'
import { useAuth } from '../stores/auth'
import { useRoute, useRouter } from 'vue-router'


const email = ref('user@example.com')
const password = ref('password')
const loading = ref(false)
const auth = useAuth()
const route = useRoute()
const router = useRouter()


async function submit() {
loading.value = true
const { token } = await loginApi({ email: email.value, password: password.value })
auth.login(token)
const payload = JSON.parse(atob(token))
// redirect by role or to ?redirect=
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
<form @submit.prevent="submit" class="w-full max-w-sm bg-white p-6 rounded-2xl shadow">
<h1 class="text-xl font-semibold mb-4">Login</h1>
<label class="block text-sm">Email</label>
<input v-model="email" type="email" class="w-full border rounded p-2 mb-3" />
<label class="block text-sm">Password</label>
<input v-model="password" type="password" class="w-full border rounded p-2 mb-4" />
<button :disabled="loading" class="w-full py-2 rounded bg-blue-600 text-white">{{ loading ? '...' : 'Login' }}</button>
<RouterLink class="block text-center text-sm text-blue-600 mt-3" to="/register">Create an account</RouterLink>
</form>
</div>
</template>