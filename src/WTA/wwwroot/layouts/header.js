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
        <el-space>
          <el-switch inline-prompt v-model="isDark" active-icon="ep-moon" inactive-icon="ep-sunny" />
          <el-dropdown class="cursor-pointer" v-if="appStore.token">
            <span class="el-dropdown-link flex">
              <el-icon class="el-icon--left" :size="18">
                <img v-if="appStore.user.avatar" />
                <ep-user v-else />
              </el-icon>
              {{ appStore.user.name }}
              <el-icon class="el-icon--right">
                <ep-arrow-down />
              </el-icon>
            </span>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item>
                  <router-link to="/account">
                    <el-icon> <ep-user /> </el-icon>{{appStore.localization.resources.userCenter}}
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item divided @click="confirmLogout">
                  <el-icon> <ep-switch-button /> </el-icon>{{appStore.localization.resources.logout}}
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
          <el-link type="info" v-else>
            <router-link to="/register"> {{appStore.localization.resources.register}}</router-link>
          </el-link>
        </el-space>
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
