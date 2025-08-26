<template>
  <div class="flex items-center gap-3 relative">
    <!-- Alert Me -->
<!-- Alert Me -->
<button class="alert-btn" @click="$emit('alert')">
  Alert Me
</button>


    <!-- Dropdown -->
    <div class="relative" ref="menuRef">
      <button class="user-btn" @click="toggleMenu">
        <svg xmlns="http://www.w3.org/2000/svg"
             class="h-6 w-6 text-gray-200"
             fill="currentColor"
             viewBox="0 0 20 20">
          <path d="M10 10a4 4 0 100-8 4 4 0 000 8zm-7 8a7 7 0 1114 0H3z" />
        </svg>
        <NotificationBadge :count="notificationCount" />
      </button>

      <div v-if="showMenu" class="dropdown">
        <div class="dropdown-header">{{ username }}</div>
        <RouterLink :to="homePath" class="dropdown-item">Home</RouterLink>
        <RouterLink to="/profile" class="dropdown-item">Profile</RouterLink>
        <RouterLink to="/notifications" class="dropdown-item">
          Notification Status
          <NotificationBadge :count="notificationCount" inline />
        </RouterLink>
        <RouterLink to="/update" class="dropdown-item">Update Profile</RouterLink>
        <button @click="$emit('logout')" class="dropdown-item logout">
          Logout
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { RouterLink } from 'vue-router'
import NotificationBadge from './NotificationBadge.vue'

import '../assets/user-menu.css'

defineProps({
  username: { type: String, required: true },
  homePath: { type: String, required: true },
  notificationCount: { type: Number, default: 0 }
})

const emit = defineEmits(['logout', 'alert'])

const showMenu = ref(false)
const menuRef = ref(null)

function toggleMenu() {
  showMenu.value = !showMenu.value
}
function handleClickOutside(e) {
  if (menuRef.value && !menuRef.value.contains(e.target)) {
    showMenu.value = false
  }
}
onMounted(() => document.addEventListener('click', handleClickOutside))
onBeforeUnmount(() => document.removeEventListener('click', handleClickOutside))
</script>
