import { createApp } from "vue";
import store, { useAppStore } from "./store/index.js";
import router from "./router/index.js";
import ElementPlus from "element-plus";
import * as ElementPlusIconsVue from "@element-plus/icons-vue";
import App from "/app.js";
import useLocale from "./locale/index.js";
import PubSub from "pubsub-js";
import * as signalR from "@microsoft/signalr";

//
let connectionId = null;
const connection = new signalR.HubConnectionBuilder().withUrl("./api/hub").build();
const connect = () => {
  if (connection.state === signalR.HubConnectionState.Disconnected) {
    connection
      .start()
      .then(function () {
        console.log("signalr connected");
      })
      .catch(function (error) {
        console.error(error);
        setTimeout(connect, 5000);
      });
  }
};
connection.onclose(function () {
  connect();
});
connection.on("connected", function (id) {
  connectionId = id;
});
connection.on("ServerToClient", function (method, data) {
  PubSub.publish(method, data);
});
connect();
//
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
