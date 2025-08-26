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

// Pages where Topbar should be visible
const topbarPages = [
  '/',                // viewer home
  '/app',             // user home
  '/login',
  '/register',
  '/reset-password',
  '/profile',
  '/update'
]

const showTopbar = computed(() => topbarPages.includes(route.path))
</script>
