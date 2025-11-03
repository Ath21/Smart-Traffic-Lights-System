import { defineStore } from 'pinia';
import { ref } from 'vue';
import { trafficCoordinatorService } from '../services/coordinatorApi';

export const useTrafficCoordinatorStore = defineStore('trafficCoordinator', () => {
  // State
  const intersections = ref([]);
  const trafficLights = ref([]);
  const configurations = ref([]);
  const loading = ref(false);
  const error = ref(null);

  // Actions
  const fetchIntersections = async () => {
    loading.value = true;
    error.value = null;
    try {
      const { data } = await trafficCoordinatorService.getAllIntersections();
      intersections.value = data;
    } catch (err) {
      error.value = err;
    } finally {
      loading.value = false;
    }
  };

  const fetchTrafficLights = async () => {
    loading.value = true;
    error.value = null;
    try {
      const { data } = await trafficCoordinatorService.getAllTrafficLights();
      trafficLights.value = data;
    } catch (err) {
      error.value = err;
    } finally {
      loading.value = false;
    }
  };

  const fetchConfigurations = async () => {
    loading.value = true;
    error.value = null;
    try {
      const { data } = await trafficCoordinatorService.getAllConfigurations();
      configurations.value = data;
    } catch (err) {
      error.value = err;
    } finally {
      loading.value = false;
    }
  };

  const applyMode = async (payload) => {
    try {
      await trafficCoordinatorService.applyMode(payload);
      await fetchConfigurations(); // refresh configs
    } catch (err) {
      error.value = err;
    }
  };

  const overrideLight = async (payload) => {
    try {
      await trafficCoordinatorService.overrideLight(payload);
      await fetchTrafficLights(); // refresh lights
    } catch (err) {
      error.value = err;
    }
  };

  return {
    intersections,
    trafficLights,
    configurations,
    loading,
    error,
    fetchIntersections,
    fetchTrafficLights,
    fetchConfigurations,
    applyMode,
    overrideLight,
  };
});
