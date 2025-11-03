import { defineStore } from 'pinia';
import { ref } from 'vue';
import { trafficControllerService } from '../services/lightControllerApi';

export const useTrafficControllerStore = defineStore('trafficController', () => {
  const controllers = ref([]); // array of {state, cycle, failover}
  const loading = ref(false);
  const error = ref(null);

  const fetchControllerData = async (index) => {
    loading.value = true;
    error.value = null;
    try {
      const [stateRes, cycleRes, failoverRes] = await Promise.all([
        trafficControllerService.getState(index),
        trafficControllerService.getCycle(index),
        trafficControllerService.getFailover(index),
      ]);
      controllers.value[index] = {
        state: stateRes.data,
        cycle: cycleRes.data,
        failover: failoverRes.data,
      };
    } catch (err) {
      error.value = err;
    } finally {
      loading.value = false;
    }
  };

  const fetchAllControllers = async (count = 12) => {
    // fetch all controllers 5261–5272 by default
    const promises = Array.from({ length: count }).map((_, i) => fetchControllerData(i));
    await Promise.all(promises);
  };

  return {
    controllers,
    loading,
    error,
    fetchControllerData,
    fetchAllControllers,
  };
});
