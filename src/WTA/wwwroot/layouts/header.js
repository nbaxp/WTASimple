import html from "html";
import { useAppStore } from "../store/index.js";
import Icon from "../components/icon/index.js";
import LayoutLogo from "./logo.js";
import { useDark, useToggle } from "@vueuse/core";

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
      <div class="flex">
        <el-switch inline-prompt v-model="isDark" active-icon="ep-moon" inactive-icon="ep-sunny" />
      </div>
    </div>
  `,
  setup() {
    const appStore = useAppStore();
    const isDark = useDark();
    const toggleDark = useToggle(isDark);
    return {
      appStore,
      isDark,
      toggleDark,
      toggle() {
        appStore.isMenuCollapse = !appStore.isMenuCollapse;
      },
    };
  },
};
