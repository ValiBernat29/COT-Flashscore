import { createApp } from "vue";
import { createPinia } from "pinia";
import "./assets/main.css";
import App from "./App.vue";
import router from "./router";

const app = createApp(App);

// Pinia is registered before the router so stores are available to any future route guards
app.use(createPinia());
app.use(router);

app.mount("#app");
