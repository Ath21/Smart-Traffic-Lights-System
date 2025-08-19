import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import { createPinia } from "pinia"; // ✅ import pinia
import "./style.css";

const app = createApp(App);
const pinia = createPinia(); // ✅ create pinia instance

app.use(router);
app.use(pinia); // ✅ register pinia
app.mount("#app");
