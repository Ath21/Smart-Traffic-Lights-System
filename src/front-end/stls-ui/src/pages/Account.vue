<template>
  <div class="account-page p-6 max-w-lg mx-auto">
    <h2 class="text-2xl font-bold mb-4">My Account</h2>

    <div v-if="loading" class="text-gray-500">Loading...</div>
    <div v-else-if="error" class="text-red-600">Error: {{ error }}</div>

    <div v-else class="space-y-4 bg-white p-4 rounded shadow">
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
import { useAuth } from '../stores/auth'

const auth = useAuth()

const user = ref(null)
const loading = ref(true)
const error = ref(null)

async function fetchUser() {
  try {
    const res = await auth.apiFetch('http://localhost:5055/api/users/me')
    if (!res.ok) throw new Error(`HTTP error ${res.status}`)
    const data = await res.json()

    // normalize PascalCase to camelCase
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
