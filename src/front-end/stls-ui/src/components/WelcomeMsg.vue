<template>
  <div class="welcome-msg">
    Welcome, {{ username }}
  </div>
</template>

<script setup>
import '../assets/welcome-msg.css'

defineProps({
  username: { type: String, required: true }
})
</script>
