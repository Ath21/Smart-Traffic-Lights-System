<template>
  <div class="users-page">
    <h2 class="page-title">User Management</h2>

    <div v-if="loading" class="loading">Loading users...</div>

    <table v-else class="users-table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Username</th>
          <th>Email</th>
          <th>Role</th>
          <th>Status</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="u in users" :key="u.UserId">
        <td>{{ u.UserId }}</td>
        <td>{{ u.Username }}</td>
        <td>{{ u.Email }}</td>
        <td>{{ u.Role }}</td>
        <td>{{ u.Status }}</td>
        <td>
            <button
            class="btn-delete"
            @click="deleteUser(u.UserId)"
            :disabled="deletingIds.includes(u.UserId)"
            >
            {{ deletingIds.includes(u.UserId) ? "Deleting..." : "Delete" }}
            </button>
        </td>
        </tr>

      </tbody>
    </table>

    <div v-if="error" class="error">{{ error }}</div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuth } from '../../stores/userStore'
import '../../assets/users.css'

const auth = useAuth()
const users = ref([])
const loading = ref(false)
const error = ref(null)
const deletingIds = ref([])

async function fetchUsers() {
  loading.value = true
  error.value = null
  try {
    const res = await auth.apiFetch('http://localhost:5055/api/users/all')
    if (!res.ok) throw new Error(`Failed to fetch users: ${res.status}`)
    users.value = await res.json()
  } catch (err) {
    console.error(err)
    error.value = err.message
  } finally {
    loading.value = false
  }
}

async function deleteUser(userId) {
  if (!confirm('Are you sure you want to delete this user?')) return

  deletingIds.value.push(userId)
  error.value = null
  try {
    const res = await auth.apiFetch(`http://localhost:5055/api/users/${userId}`, {
      method: 'DELETE',
    })
    if (!res.ok) {
      const body = await res.json().catch(() => ({}))
      throw new Error(body.error || `Failed to delete user: ${res.status}`)
    }
    // Remove from local list
    users.value = users.value.filter(u => u.userId !== userId)
  } catch (err) {
    console.error(err)
    error.value = err.message
  } finally {
    deletingIds.value = deletingIds.value.filter(id => id !== userId)
  }
}

onMounted(fetchUsers)
</script>


