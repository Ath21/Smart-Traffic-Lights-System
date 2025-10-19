<template>
  <!-- Global Topbar -->
  <Topbar v-if="showTopbar" />

  <!-- Page content -->
  <RouterView />
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import Topbar from './components/TopBar.vue'

const route = useRoute()

// Show Topbar on all main user-facing pages
const topbarPages = [
  '/',               // redirect to profile
  '/login',
  '/register',
  '/reset-password',
  '/profile',
  '/update',
  '/notifications'
]

// Determine if current route should show Topbar
const showTopbar = computed(() => topbarPages.includes(route.path))
</script>
