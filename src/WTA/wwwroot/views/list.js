import AppList from "../components/list/index.js";
import { ref } from "vue";
import router from "../router/index.js";

export default {
  components: { AppList },
  template: `<app-list :initApi="api" />`,
  setup() {
    const url =
      router.currentRoute.value.fullPath +
      "/" +
      router.currentRoute.value.meta.buttons.find((o) => o.path === "index").path;
    const api = ref(url);
    return {
      api,
    };
  },
};
