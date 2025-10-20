<template>
  <header class="topbar">
    <!-- === Left side: PADA + UNIWA STLS + STLS emblem === -->
    <div class="left">
      <img
        src="/PADA.png"
        alt="PADA"
        class="pada-logo"
      />
      <h1 class="title">
        UNIWA <span class="highlight">STLS</span>
      </h1>
      <img
        src="/STLS-Logo-TopBar.png"
        alt="UNIWA STLS"
        class="stls-logo"
      />
    </div>

    <!-- === Right side: Buttons / Role-based === -->
    <div class="right-nav">
      <!-- Unauthenticated users -->
      <template v-if="!auth.isAuthenticated">
        <!-- On auth pages show Home button -->
        <template v-if="['/login', '/register', '/reset-password'].includes(router.currentRoute.value.path)">
          <RouterLink to="/" class="home-btn">Home</RouterLink>
        </template>

        <!-- On other public pages show Login / Register -->
        <template v-else>
          <RouterLink to="/login" class="home-btn">Login</RouterLink>
          <RouterLink to="/register" class="home-btn register-btn">Register</RouterLink>
        </template>
      </template>

      <!-- Authenticated users -->
      <template v-else>
        <span class="user-info">👋 {{ auth.user?.username || auth.user?.email }}</span>

        <RouterLink
          v-if="auth.user?.role === 'User'"
          to="/stls/profile"
          class="home-btn"
        >
          Profile
        </RouterLink>

        <RouterLink
          v-if="auth.user?.role === 'User'"
          to="/stls/notifications"
          class="home-btn"
        >
          🔔
        </RouterLink>

        <RouterLink
          v-if="auth.user?.role === 'TrafficOperator'"
          to="/stls/operator"
          class="home-btn operator-btn"
        >
          Operator
        </RouterLink>

        <RouterLink
          v-if="auth.user?.role === 'Admin'"
          to="/stls/admin"
          class="home-btn admin-btn"
        >
          Admin
        </RouterLink>

        <button @click="logout" class="home-btn logout-btn">Logout</button>
      </template>
    </div>
  </header>
</template>

<script setup>
import "../assets/topbar.css";
import { useAuth } from "../stores/userStore";
import { useRouter } from "vue-router";

const auth = useAuth();
const router = useRouter();

function logout() {
  auth.logout();
  router.push("/");
}
</script>
