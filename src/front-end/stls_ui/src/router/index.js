import { createRouter, createWebHistory } from "vue-router";
import Dashboard from "../pages/Dashboard.vue";
import Map from "../pages/Map.vue";
import LogsPage from "../pages/LogsPage.vue";

const routes = [
  { path: "/", name: "Dashboard", component: Dashboard },
  { path: "/map", name: "Map", component: Map },
  { path: "/logs", name: "Logs", component: LogsPage },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
