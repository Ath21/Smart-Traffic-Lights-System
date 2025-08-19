import { createRouter, createWebHistory } from "vue-router";
import Home from "../pages/Home.vue";
import Map from "../pages/Map.vue";

const routes = [
  { path: "/", name: "Home", component: Home },
  { path: "/map", name: "Map", component: Map },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
