<template>
  <!-- Case: Login/Register pages → Home -->
  <RouterLink
    v-if="isLoginOrRegister"
    :to="homePath"
    class="home-btn"
  >
    Home
  </RouterLink>

  <!-- Case: Not logged in (but not login/register) → Login + Register -->
  <nav v-else-if="!isAuthenticated" class="flex gap-2">
    <RouterLink class="login" to="/login">Login</RouterLink>
    <RouterLink class="register" to="/register">Register</RouterLink>
  </nav>
</template>

<script setup>
import { RouterLink } from 'vue-router'
import '../assets/auth-links.css'

defineProps({
  isLoginOrRegister: { type: Boolean, required: true },
  isAuthenticated: { type: Boolean, required: true },
  homePath: { type: String, required: true }
})
</script>
