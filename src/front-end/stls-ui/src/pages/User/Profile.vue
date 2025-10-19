<template>
  <div class="account-page">
    <h2>My Profile</h2>

    <div v-if="loading" class="loading">Loading...</div>
    <div v-else-if="error" class="error">Error: {{ error }}</div>

    <div v-else class="profile-card">
      <p><strong>Username:</strong> {{ user.username }}</p>
      <p><strong>Email:</strong> {{ user.email }}</p>
      <p><strong>Role:</strong> {{ user.role }}</p>
      <p><strong>Status:</strong> {{ user.status }}</p>
      <p><strong>Created at:</strong> {{ formatDate(user.createdAt) }}</p>
      <p><strong>Updated at:</strong> {{ formatDate(user.updatedAt) }}</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuth } from '../../stores/users'
import '../../assets/profile.css'   // ✅ make sure this import is here

const auth = useAuth()
const user = ref(null)
const loading = ref(true)
const error = ref(null)

async function fetchUser() {
  try {
    const data = await getProfileApi()
    user.value = {
      userId: data.UserId,
      username: data.Username,
      email: data.Email,
      role: data.Role,
      status: data.Status,
      createdAt: data.CreatedAt,
      updatedAt: data.UpdatedAt
    }
  } catch (err) {
    error.value = err.message
  } finally {
    loading.value = false
  }
}

function formatDate(dateStr) {
  if (!dateStr) return "—"
  return new Date(dateStr).toLocaleString()
}

onMounted(fetchUser)
</script>
