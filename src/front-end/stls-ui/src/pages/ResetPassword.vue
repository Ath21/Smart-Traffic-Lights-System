<template>
  <div class="login-page">
    <div class="overlay"></div>

    <div class="login-card">
      <div class="card-content">
        <h1 class="title">Reset Password</h1>

        <form class="form" @submit.prevent="resetPassword">
          <!-- Email -->
          <input
            v-model="email"
            type="email"
            required
            placeholder="Email"
            class="input"
          />

          <!-- New Password -->
          <input
            v-model="newPassword"
            type="password"
            required
            placeholder="New Password"
            class="input"
          />

          <button type="submit" class="btn">Reset Password</button>
        </form>

        <p v-if="message" class="message">{{ message }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { resetPasswordApi } from "../services/authApi";

const email = ref("");
const newPassword = ref("");
const message = ref("");

async function resetPassword() {
  try {
    await resetPasswordApi(email.value, newPassword.value);
    message.value = "✅ Password reset successful!";
  } catch (err) {
    message.value =
      "❌ Failed to reset password: " +
      (err.response?.data?.message || err.message);
  }
}
</script>

<style scoped>
.input {
  display: block;
  width: 100%;
  margin: 10px 0;
  padding: 10px;
  border-radius: 8px;
  border: 1px solid #ccc;
}

.btn {
  width: 100%;
  padding: 12px;
  margin-top: 10px;
  background: teal;
  color: white;
  font-weight: bold;
  border: none;
  border-radius: 8px;
  cursor: pointer;
}

.message {
  margin-top: 15px;
  font-size: 14px;
}
</style>
