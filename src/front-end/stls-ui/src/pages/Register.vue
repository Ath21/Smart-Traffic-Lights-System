<script setup>
import { ref } from 'vue'
import { registerApi } from '../services/fakeAuthApi'
import { useAuth } from '../stores/auth'
import { useRouter } from 'vue-router'


const email = ref('newuser@example.com')
const password = ref('password')
const auth = useAuth()
const router = useRouter()


async function submit() {
const { token } = await registerApi({ email: email.value, password: password.value })
auth.login(token)
router.push('/app')
}
</script>


<template>
<div class="min-h-screen flex items-center justify-center p-4 bg-gray-50">
<form @submit.prevent="submit" class="w-full max-w-sm bg-white p-6 rounded-2xl shadow">
<h1 class="text-xl font-semibold mb-4">Register</h1>
<label class="block text-sm">Email</label>
<input v-model="email" type="email" class="w-full border rounded p-2 mb-3" />
<label class="block text-sm">Password</label>
<input v-model="password" type="password" class="w-full border rounded p-2 mb-4" />
<button class="w-full py-2 rounded bg-gray-800 text-white">Create account</button>
<RouterLink class="block text-center text-sm text-blue-600 mt-3" to="/login">Already have an account?</RouterLink>
</form>
</div>
</template>