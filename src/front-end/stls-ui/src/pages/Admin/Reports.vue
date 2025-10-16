<script setup>
import { onMounted, ref } from 'vue'
import { useAuth } from '../../stores/users'
import { getDailyReports } from '../../services/analyticsApi'

const auth = useAuth()
const reports = ref([])

onMounted(async () => {
  reports.value = await getDailyReports(auth.token)
})
</script>

<template>
  <div class="p-4">
    <h2 class="font-bold mb-2">Daily Traffic Reports</h2>
    <ul>
      <li v-for="r in reports" :key="r.date">
        <b>{{ r.date }}</b> — {{ r.summary }}
      </li>
    </ul>
  </div>
</template>
