<template>
  <div class="subscribe-page">
    <div class="subscribe-card">
      <form class="form" @submit.prevent="onSubmit">
        <!-- Intersection -->
        <label for="intersection">Intersection</label>
        <select v-model="intersection" id="intersection" required>
          <option disabled value="">Select Intersection</option>
          <option v-for="i in intersections" :key="i.value" :value="i.value">
            {{ i.label }}
          </option>
        </select>

        <!-- Metric -->
        <label for="metric">Metric</label>
        <select v-model="metric" id="metric" required>
          <option disabled value="">Select Metric</option>
          <option v-for="m in metrics" :key="m" :value="m">{{ m }}</option>
        </select>

        <!-- Submit -->
        <button type="submit" class="btn" :disabled="loading">
          {{ loading ? 'Subscribing...' : 'Alert Me' }}
        </button>

        <!-- Feedback -->
        <p v-if="success" class="success-msg">{{ success }}</p>
        <p v-if="error" class="error-msg">{{ error }}</p>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { subscribeApi } from "../../services/userApi";

const intersection = ref("");
const metric = ref("");
const loading = ref(false);
const error = ref("");
const success = ref("");

const intersections = [
  { value: "Agiou Spyridonos", label: "Agiou Spyridonos" },
  { value: "Anatoliki Pyli", label: "Anatoliki Pyli" },
  { value: "Dytiki Pyli", label: "Dytiki Pyli" },
  { value: "Ekklisia", label: "Ekklisia" },
  { value: "Kentriki Pyli", label: "Kentriki Pyli" },
];

const metrics = ["Congestion", "Incidents", "Summary"];

async function onSubmit() {
  if (!intersection.value || !metric.value) return;

  loading.value = true;
  error.value = "";
  success.value = "";

  try {
    await subscribeApi({
      intersection: intersection.value,
      metric: metric.value
    });
    success.value = `Subscribed to ${metric.value} alerts at ${intersection.value}!`;
    intersection.value = "";
    metric.value = "";
  } catch (err) {
    console.error(err);
    error.value = err.response?.data?.message || err.message || "Subscription failed.";
  } finally {
    loading.value = false;
  }
}
</script>

<style src="../../assets/subscribe.css"></style>
