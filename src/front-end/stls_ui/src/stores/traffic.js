import { defineStore } from "pinia";

export const useTrafficStore = defineStore("traffic", {
  state: () => ({
    lights: [
      { id: 1, location: "Main Street", status: "green", timer: 30 },
      { id: 2, location: "2nd Avenue", status: "red", timer: 45 },
    ],
  }),

  actions: {
    toggleLight(id) {
      const light = this.lights.find(l => l.id === id);
      if (light) {
        light.status = light.status === "green" ? "red" : "green";
      }
    },

    countdown() {
      this.lights.forEach(light => {
        if (light.timer > 0) {
          light.timer--;
        } else {
          // auto-switch when timer hits 0
          light.status = light.status === "green" ? "red" : "green";
          light.timer = 30; // reset timer
        }
      });
    },
  },
});
