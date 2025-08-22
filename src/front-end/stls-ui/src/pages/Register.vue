<template>
  <div class="register-page">
    <div class="overlay"></div>

    <div class="register-card">
      <div class="card-content">
        <!-- Title -->
        <h1 class="title">
          UNIWA STLS
        </h1>

        <!-- Form -->
        <form class="form" @submit.prevent="submit">
          <input v-model="email" type="email" required placeholder="Email" class="input" />
          <input v-model="username" type="text" required placeholder="Username" class="input" />
          <input v-model="password" type="password" required placeholder="Password" class="input" />

          <button :disabled="loading" class="submit-btn">
            {{ loading ? 'Registering…' : 'REGISTER' }}
          </button>
        </form>

        <!-- Footer link -->
        <p class="footer-text">
          Have an account?
          <RouterLink to="/login" class="login-link">Log in</RouterLink>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { registerApi } from '../services/fakeAuthApi'
import { useAuth } from '../stores/auth'
import { useRouter } from 'vue-router'
import '../assets/register.css'

const email = ref('')
const username = ref('')
const password = ref('')
const loading = ref(false)
const auth = useAuth()
const router = useRouter()

async function submit() {
  if (loading.value) return
  loading.value = true
  try {
    const { token } = await registerApi({
      email: email.value,
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
