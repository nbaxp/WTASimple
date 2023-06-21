import html from "html";
import { ref, onMounted, onUnmounted } from "vue";
import { useAppStore } from "../store/index.js";
import SvgIcon from "../components/icon/index.js";
import LayoutLogo from "./logo.js";
import { useDark, useFullscreen, useToggle } from "@vueuse/core";
import { ElMessage, ElMessageBox } from "element-plus";
import { useI18n } from "vue-i18n";
import { logout } from "../api/user.js";
import LayoutLocale from "./locale.js";
import screenfull from "screenfull";

export default {
  components: { SvgIcon, LayoutLogo, LayoutLocale, ElMessage, ElMessageBox },
  template: html`
    <div class="flex items-center justify-between">
      <div class="flex items-center justify-center">
        <layout-logo />
        <el-icon @click="toggleMenuCollapse" class="cursor-pointer">
          <svg-icon name="unfold" v-if="appStore.isMenuCollapse" />
          <svg-icon name="fold" v-else />
        </el-icon>
      </div>
      <div class="flex">
        <el-space>
          <el-icon v-model="isDark" @click="toggleDark()" :size="18" class="cursor-pointer">
            <ep-sunny v-if="isDark" />
            <ep-moon v-else />
          </el-icon>
          <el-icon @click="toggleFullscreen" :size="18" class="cursor-pointer">
            <svg-icon name="fullscreen-exit" v-if="isFullscreen" />
            <svg-icon name="fullscreen" v-else />
          </el-icon>
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
                    <el-icon> <ep-user /> </el-icon>{{$t('userCenter')}}
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item divided @click="confirmLogout">
                  <el-icon> <ep-switch-button /> </el-icon>{{$t('logout')}}
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
          <el-link type="info" v-else>
            <router-link to="/register"> {{$t('register')}}</router-link>
          </el-link>
          <layout-locale />
        </el-space>
      </div>
    </div>
  `,
  setup() {
    const i18n = useI18n();
    const appStore = useAppStore();
    //
    const isDark = useDark();
    const toggleDark = useToggle(isDark);
    const toggleMenuCollapse = () => (appStore.isMenuCollapse = !appStore.isMenuCollapse);
    //
    const { isFullscreen, toggle: toggleFullscreen } = useFullscreen(document.documentElement);
    const confirmLogout = async () => {
      try {
        await ElMessageBox.confirm(i18n.t("confirmLogout"), i18n.t("tip"), { type: "warning" });
        logout();
      } catch (error) {
        if (error === "cancel") {
          ElMessage({
            type: "info",
            message: i18n.t("cancel"),
          });
        }
      }
    };
    return {
      appStore,
      isDark,
      toggleDark,
      toggleMenuCollapse,
      isFullscreen,
      toggleFullscreen,
      confirmLogout,
    };
  },
};
