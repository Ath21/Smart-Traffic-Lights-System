<template>
  <div class="update-profile-page">
    <div class="update-card">
      <h1 class="title">Update Profile</h1>

      <form class="form" @submit.prevent="submit">
        <!-- Email -->
        <label>Email</label>
        <input
          v-model="form.email"
          type="email"
          required
          class="input"
        />

        <!-- Username -->
        <label>Username</label>
        <input
          v-model="form.username"
          type="text"
          required
          class="input"
        />

        <!-- Password -->
        <label>New Password</label>
        <input
          v-model="form.password"
          type="password"
          placeholder="Leave blank to keep old password"
          class="input"
        />

        <button class="btn" type="submit">Save Changes</button>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useAuth } from '../stores/auth'
import { updateProfileApi } from "../services/authApi";

const auth = useAuth()

// pre-fill with current user data
const form = ref({
  email: auth.user?.email || '',
  username: auth.user?.username || '',
  password: '',
  status: auth.user?.status || 'active',
  role: auth.user?.role || 'user'
})

async function submit() {
  try {
    const res = await updateProfileApi(auth.token, form.value)

    // server might not return user object, so update manually
    auth.user = {
      ...auth.user,
      email: form.value.email,
      username: form.value.username,
      role: form.value.role,
      status: form.value.status
    }

    alert('Profile updated successfully ✅')
  } catch (err) {
    console.error(err)
    alert('Failed to update profile ❌')
  }
}
</script>

<style scoped>
.update-profile-page {
  display: flex;
  justify-content: center;
  padding: 40px;
}

.update-card {
  background: white;
  padding: 24px;
  border-radius: 8px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
  width: 400px;
}

.title {
  font-size: 1.5rem;
  font-weight: bold;
  margin-bottom: 16px;
}

.form {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.input {
  border: 1px solid #ccc;
  border-radius: 6px;
  padding: 8px;
}

.btn {
  background: #06b6d4;
  color: white;
  font-weight: bold;
  padding: 10px;
  border-radius: 6px;
  cursor: pointer;
}

.btn:hover {
  background: #0891b2;
}
</style>
