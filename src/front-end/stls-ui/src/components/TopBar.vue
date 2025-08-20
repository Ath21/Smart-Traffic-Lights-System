<template>
  <header class="topbar">
    <h1>UNIWA <span>STLS</span></h1>
    <nav>
      <RouterLink class="login" to="/login">Login</RouterLink>
      <RouterLink class="register" to="/register">Register</RouterLink>
    </nav>
  </header>
</template>

<script setup>
import '../assets/topbar.css'
</script>
