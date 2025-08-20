<template>
  <div class="login-page">
    <div class="overlay"></div>

    <div class="login-card">
      <div class="card-content">
        <!-- Title -->
        <h1 class="title">
          UNIWA STLS
        </h1>

        <!-- Form -->
        <form class="form" @submit.prevent="submit">
          <input v-model="username" type="text" required placeholder="Username" class="input" />
          <input v-model="password" type="password" required placeholder="Password" class="input" />

          <button :disabled="loading" class="submit-btn">
            {{ loading ? 'Logging in…' : 'LOGIN' }}
          </button>
        </form>

        <!-- Footer link -->
        <p class="footer-text">
          Don’t have an account?
          <RouterLink to="/register" class="register-link">Register</RouterLink>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { loginApi } from '../services/fakeAuthApi'
import { useAuth } from '../stores/auth'
import { useRouter } from 'vue-router'
import '../assets/login.css'

const username = ref('')
const password = ref('')
const loading = ref(false)
const auth = useAuth()
const router = useRouter()

async function submit() {
  if (loading.value) return
  loading.value = true
  try {
    const { token } = await loginApi({
      username: username.value,
      password: password.value,
    })
    auth.login(token)
    router.push('/app')
  } finally {
    loading.value = false
  }
}
</script>
