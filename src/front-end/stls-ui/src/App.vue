<template>
  <!-- Splash Screen -->
  <div
    v-if="auth && auth.loading"
    class="fixed inset-0 flex flex-col items-center justify-center bg-[#002b5c] text-white z-50"
  >
    <img src="/STLS-Logo.png" alt="STLS Logo" class="w-32 h-32 mb-4 animate-pulse" />
    <h2 class="text-xl font-semibold tracking-wide">Loading UNIWA STLS...</h2>
  </div>

  <!-- App Layout -->
  <div v-else class="flex flex-col min-h-screen bg-white">
    <!-- Fixed top bar -->
    <TopBar v-if="showTopbar" class="sticky top-0 z-50" />

    <!-- Page content below -->
    <main class="flex-1 overflow-hidden">
      <RouterView />
    </main>
  </div>
</template>

<script setup>
import { computed } from "vue";
import { useRoute } from "vue-router";
import { useAuth } from "./stores/userStore";
import TopBar from "./components/TopBar.vue";

const auth = useAuth();
const route = useRoute();

const topbarPages = [
  "/",
  "/stls",
  "/login",
  "/register",
  "/reset-password",
  "/stls/profile",
  "/stls/update",
  "/stls/notifications"
];
const showTopbar = computed(() => topbarPages.includes(route.path));
</script>

<style scoped>
@keyframes pulse {
  0%, 100% { opacity: 1; transform: scale(1); }
  50% { opacity: 0.7; transform: scale(1.1); }
}
</style>
