import { createApp } from "vue";
import store, { useAppStore } from "./store/index.js";
import router from "./router/index.js";
import ElementPlus from "element-plus";
import * as ElementPlusIconsVue from "@element-plus/icons-vue";
import App from "/app.js";
import useLocale from "./locale/index.js";

const app = createApp(App);
app.use(store);
await useAppStore().init();
app.use(useLocale());
app.use(router);
app.use(ElementPlus);
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(`Ep${key}`, component);
}
app.mount("#app");
