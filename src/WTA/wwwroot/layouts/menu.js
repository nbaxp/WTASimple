import html from "html";
import Icon from "../components/icon/index.js";
import { useAppStore } from "../store/index.js";
import MenuItem from "./menu-item.js";
import router from "../router/index.js";

export default {
  components: { Icon, MenuItem },
  template: html`<el-menu
    :collapse="appStore.isMenuCollapse"
    :collapse-transition="false"
    :default-active="$route.fullPath"
  >
    <menu-item v-for="item in menus" v-model="item" />
  </el-menu>`,
  setup() {
    const appStore = useAppStore();
    const menus = router.getRoutes().find((o) => o.path === "/").children;
    return {
      appStore,
      menus,
    };
  },
};
