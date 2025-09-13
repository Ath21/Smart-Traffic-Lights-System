import { defineStore } from 'pinia'
import { ref } from 'vue'
import { controlLight, updateLight } from '../services/trafficApi'

export const useTraffic = defineStore('traffic', () => {
  const lastAction = ref(null)

  async function sendControl(token, intersectionId, lightId, newState) {
    lastAction.value = await controlLight(token, intersectionId, lightId, newState)
  }

  async function sendUpdate(token, intersectionId, lightId, state) {
    lastAction.value = await updateLight(token, intersectionId, lightId, state)
  }

  return { lastAction, sendControl, sendUpdate }
})
