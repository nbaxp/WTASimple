import html from "html";
import LayoutHeader from "./header.js";
import LayoutMenu from "./menu.js";
import LayoutTabs from "./tabs.js";
import LayoutFooter from "./footer.js";
import Icon from "../components/icon/index.js";
import { useAppStore } from "../store/index.js";
import { computed } from "vue";

export default {
  components: { Icon, LayoutHeader, LayoutMenu, LayoutTabs, LayoutFooter },
  template: html`<el-container>
    <el-header><layout-header /></el-header>
    <el-container>
      <el-aside width="auto">
        <el-scrollbar><layout-menu /></el-scrollbar>
      </el-aside>
      <el-container class="backtop">
        <el-scrollbar>
          <layout-tabs />
          <el-main>
            <router-view v-if="!isRefreshing" v-slot="{ Component, route }">
              <component
                :is="Component"
                v-if="!appStore.isUseTabsRouter || !route.meta?.cached"
                :key="$route.fullPath"
              />
              <keep-alive>
                <component
                  :is="Component"
                  v-if="appStore.isUseTabsRouter && route.meta?.cached"
                  :key="route.fullPath"
                />
              </keep-alive>
            </router-view>
          </el-main>
          <el-footer>
            <layout-footer />
          </el-footer>
          <el-backtop target=".backtop > .el-scrollbar > .el-scrollbar__wrap" />
        </el-scrollbar>
      </el-container>
    </el-container>
  </el-container>`,
  setup() {
    const appStore = useAppStore();
    const isRefreshing = computed(() => appStore.isRefreshing);
    const path = computed(() => useRoute().matched[0].path);
    const items = computed(() => useRoute().matched[0].children);
    return {
      appStore,
      isRefreshing,
      path,
      items,
    };
  },
};
