<template>
  <div class="users-page">
    <h2 class="page-title">User Management</h2>

    <div v-if="loading" class="loading">Loading users...</div>

    <table v-else-if="users.length" class="users-table">
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
        <tr v-for="u in users" :key="u.id">
          <td>{{ u.id }}</td>
          <td>{{ u.username }}</td>
          <td>{{ u.email }}</td>
          <td>{{ u.role }}</td>
          <td>{{ u.status }}</td>
          <td>
            <button
              class="btn-delete"
              @click="deleteUser(u.id)"
              :disabled="deletingIds.includes(u.id)"
            >
              {{ deletingIds.includes(u.id) ? "Deleting..." : "Delete" }}
            </button>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-else class="no-users">No users found.</div>
    <div v-if="error" class="error">{{ error }}</div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import { useUserStore } from "../../stores/userStore";
import "../../assets/users.css";

const userStore = useUserStore();

// Reactive state from store
const users = ref([]);
const loading = computed(() => userStore.loading);
const error = computed(() => userStore.error);
const deletingIds = ref([]);

// ===============================
// Fetch users
// ===============================
async function fetchUsers() {
  try {
    const data = await userStore.apiFetch("/api/users/all");
    if (!data.ok) throw new Error(`Failed to fetch users: ${data.status}`);
    users.value = await data.json();
  } catch (err) {
    console.error(err);
    userStore.error = err.message;
  }
}

// ===============================
// Delete user
// ===============================
async function deleteUser(userId) {
  if (!confirm("Are you sure you want to delete this user?")) return;

  deletingIds.value.push(userId);
  try {
    const res = await userStore.apiFetch(`/api/users/${userId}`, { method: "DELETE" });
    if (!res.ok) {
      const body = await res.json().catch(() => ({}));
      throw new Error(body.error || `Failed to delete user: ${res.status}`);
    }
    // Remove locally
    users.value = users.value.filter(u => u.id !== userId);
  } catch (err) {
    console.error(err);
    userStore.error = err.message;
  } finally {
    deletingIds.value = deletingIds.value.filter(id => id !== userId);
  }
}

// ===============================
// Lifecycle
// ===============================
onMounted(() => {
  fetchUsers();
});
</script>
