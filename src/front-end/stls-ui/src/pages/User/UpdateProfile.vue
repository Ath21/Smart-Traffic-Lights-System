<template>
  <div class="update-profile-page">
    <div class="update-card">
      <h2>Update Profile</h2>

      <form class="form" @submit.prevent="submit">
        <!-- Email -->
        <label>Email</label>
        <input v-model="form.email" type="email" required class="input" />

        <!-- Username -->
        <label>Username</label>
        <input v-model="form.username" type="text" required class="input" />

        <!-- Password -->
        <label>New Password</label>
        <input
          v-model="form.password"
          type="password"
          placeholder="Leave blank to keep old password"
          class="input"
        />

        <!-- Confirm Password -->
        <label>Confirm New Password</label>
        <input
          v-model="form.confirmPassword"
          type="password"
          placeholder="Re-enter new password"
          class="input"
        />

        <!-- Error Message -->
        <p v-if="error" class="error-msg">❌ {{ error }}</p>
        <p v-if="success" class="success-msg">✅ Profile updated successfully!</p>

        <button class="btn" type="submit">Save Changes</button>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { useAuth } from "../../stores/userStore";
import { updateProfileApi } from "../../services/userApi";
import "../../assets/update-profile.css";

const auth = useAuth();

const form = ref({
  email: auth.user?.email || "",
  username: auth.user?.username || "",
  password: "",
  confirmPassword: "",
});

const error = ref("");
const success = ref(false);

async function submit() {
  error.value = "";
  success.value = false;

  if (form.value.password && form.value.password !== form.value.confirmPassword) {
    error.value = "Passwords do not match!";
    return;
  }

  try {
    await updateProfileApi(auth.token, form.value);

    // Update Pinia store state
    auth.user = {
      ...auth.user,
      email: form.value.email,
      username: form.value.username,
    };

    success.value = true;
    form.value.password = "";
    form.value.confirmPassword = "";
  } catch (err) {
    console.error("[UpdateProfile] error:", err);
    error.value = err.response?.data?.message || "❌ Failed to update profile.";
  }
}
</script>
