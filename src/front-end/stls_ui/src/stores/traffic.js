import { defineStore } from "pinia";

export const useTrafficStore = defineStore("traffic", {
  state: () => ({
    lights: [
      // Left Intersection Edessis
      { id: 1, location: "Edessis Light 1", coords: [23.67372816002177, 38.00152733622807], status: "green" },
      { id: 2, location: "Edessis Light 2", coords: [23.673722795604068, 38.00165837717111], status: "red" },
      { id: 3, location: "Edessis Light 3", coords: [23.673620871667698, 38.00162456017597], status: "yellow" },

      // Mid Intersection Dimitsanas Gate
      { id: 4, location: "Dimitsanas Gate 1", coords: [23.674363239694117, 38.002491943770984], status: "green" },
      { id: 5, location: "Dimitsanas Gate 2", coords: [23.674508078972114, 38.002686388980045], status: "red" },
      { id: 6, location: "Dimitsanas Gate 3", coords: [23.674544473009455, 38.002610019425525], status: "yellow" },

      // Up Left Corner Dimitsanas Intersection
      { id: 7, location: "Dimitsanas Intersection 1", coords: [23.67606783486132, 38.00464905032311], status: "red" },
      { id: 8, location: "Dimitsanas Intersection 2", coords: [23.67616266526471, 38.004681928278714], status: "green" },

      // Agiou Spyridonos Gate
      { id: 9, location: "Agiou Spyridonos 1", coords: [23.67646612255553, 38.00445178227973], status: "yellow" },
      { id: 10, location: "Agiou Spyridonos 2", coords: [23.67657233260732, 38.0044607490205], status: "green" },

      // Milou Gate
      { id: 11, location: "Milou Gate", coords: [23.677717883925634, 38.00365373795255], status: "red" },
    ],
  }),

  actions: {
    toggleLight(id) {
      const light = this.lights.find((l) => l.id === id);
      if (light) {
        const cycle = ["green", "yellow", "red"];
        light.status = cycle[(cycle.indexOf(light.status) + 1) % cycle.length];
      }
    },
  },
});
