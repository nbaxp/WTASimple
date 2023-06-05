import html from "html";
import { useAppStore } from "../store/index.js";
import Icon from "../components/icon/index.js";
import LayoutLogo from "./logo.js";

export default {
  components: { Icon, LayoutLogo },
  template: html`
    <div class="flex-justify">
      <div class="flex">
        <layout-logo />
        <el-icon @click="toggle" class="cursor-pointer">
          <icon name="unfold" v-if="isCollapse" />
          <icon name="fold" v-else />
        </el-icon>
      </div>
      <div class="flex">theme</div>
    </div>
  `,
  setup() {
    const appStore = useAppStore();
    return {
      appStore,
      toggle() {
        appStore.isMenuCollapse = !appStore.isMenuCollapse;
      },
    };
  },
};
