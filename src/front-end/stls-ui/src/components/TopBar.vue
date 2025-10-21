<template>
  <header class="topbar">
    <!-- === Left side: PADA + UNIWA STLS + STLS emblem === -->
    <div class="left">
      <img src="/PADA.png" alt="PADA" class="pada-logo" />
      <h1 class="title">
        UNIWA <span class="highlight">STLS</span>
      </h1>
      <img src="/STLS-Logo-TopBar.png" alt="UNIWA STLS" class="stls-logo" />
    </div>

    <!-- === Right side: User Menu / Auth Buttons === -->
    <div class="right-nav">
      <!-- Show User Menu if logged in -->
      <UserMenu
        v-if="auth.isAuthenticated"
        :username="auth.user?.username || auth.user?.email"
        :isAuthenticated="auth.isAuthenticated"
        :notification-count="notificationCount"
        home-path="/stls"
        @logout="handleLogout"
        @alert="handleAlert"
      />

      <!-- Show Login / Register / Home when not authenticated -->
      <template v-else>
        <RouterLink
          v-if="['/login', '/register', '/reset-password'].includes(router.currentRoute.value.path)"
          to="/"
          class="home-btn"
        >
          Home
        </RouterLink>

        <template v-else>
          <RouterLink to="/login" class="home-btn">Login</RouterLink>
          <RouterLink to="/register" class="home-btn register-btn">Register</RouterLink>
        </template>
      </template>
    </div>
  </header>
</template>

<script setup>
import "../assets/topbar.css";
import UserMenu from "./UserMenu.vue";
import { useAuth } from "../stores/userStore";
import { useRouter } from "vue-router";
import { ref } from "vue";

const auth = useAuth();
const router = useRouter();

// === Notification Badge Count ===
// (You can later fetch this from Notification API)
const notificationCount = ref(0);

// === Methods ===
function handleLogout() {
  auth.logout();
  router.push("/");
}

function handleAlert() {
  // Later connect to NotificationService or local modal
  alert("🚦 Public Alert: Notification triggered!");
}
</script>
