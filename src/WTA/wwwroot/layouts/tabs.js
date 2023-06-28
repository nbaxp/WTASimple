import html from "html";
import { ref, nextTick } from "vue";
import { useRoute, onBeforeRouteUpdate, useRouter } from "vue-router";
import Icon from "../components/icon/index.js";
import { useAppStore } from "../store/index.js";
import MenuItem from "./menu-item.js";

export default {
  components: { Icon, MenuItem },
  template: html`<el-tabs
    v-model="model"
    type="border-card"
    class="router-tab"
    @tab-remove="remove"
    @tab-click="onClick"
  >
    <template v-for="(item, index) in appStore.routes" :key="item.fullPath">
      <el-tab-pane v-model="item.fullPath" :name="item.fullPath" :closable="appStore.routes.length > 1">
        <template #label>
          <el-dropdown
            :ref="(el) => setRef(index, el)"
            class="h-full"
            trigger="contextmenu"
            @visible-change="showContextMenu(index, $event)"
          >
            <span class="inline-flex items-center">
              <el-icon><icon v-if="item.meta.icon" :name="item.meta.icon" /></el-icon>
              {{ item.meta?.title ?? item.fullPath }}
            </span>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item @click="refresh(index)"><i-ep-refresh />刷新</el-dropdown-item>
                <el-dropdown-item :disabled="index === 0" @click="removeLeft(index)">
                  <i-ep-back />关闭左侧
                </el-dropdown-item>
                <el-dropdown-item :disabled="index === appStore.routes.length - 1" @click="removeRight(index)">
                  <i-ep-right />关闭右侧
                </el-dropdown-item>
                <el-dropdown-item
                  :disabled="index === 0 && index === appStore.routes.length - 1"
                  @click="removeOthers(index)"
                >
                  <i-ep-switch />关闭其他
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </template>
      </el-tab-pane>
    </template>
  </el-tabs>`,
  styles: html`
    <style>
      .router-tab {
        box-sizing: border-box;
        height: 40px !important;
        background-color: var(--el-fill-color-blank);
        border-width: 0 !important;
      }

      .router-tab .el-tabs__item {
        padding: 13px !important;
        border-bottom-width: 0;
      }

      .router-tab .el-tabs__content {
        display: none;
      }
    </style>
  `,
  setup() {
    const appStore = useAppStore();
    const itemRefs = ref([]);
    const currentRoute = useRoute();
    const router = useRouter();
    const model = ref(currentRoute.fullPath);

    onBeforeRouteUpdate((to) => {
      model.value = to.fullPath;
    });

    const setRef = (index, el) => {
      if (el) {
        itemRefs.value[index] = el;
      } else {
        itemRefs.value.splice(index, 1);
      }
    };
    const showContextMenu = (index, show) => {
      if (show) {
        itemRefs.value.forEach((item, i) => {
          if (i !== index) {
            item?.handleClose();
          }
        });
      }
    };

    const refresh = (index) => {
      const currentIndex = appStore.routes.findIndex((o) => o.fullPath === currentRoute.fullPath);
      const route = appStore.routes[index];
      if (index !== currentIndex) {
        router.push({ path: route.fullPath });
      }
      appStore.isRefreshing = true;
      nextTick(() => {
        appStore.isRefreshing = false;
      });
    };

    const remove = (name) => {
      if (appStore.routes.length > 1) {
        const index = appStore.routes.findIndex((o) => o.fullPath === name);
        const currentIndex = appStore.routes.findIndex((o) => o.fullPath === currentRoute.fullPath);
        appStore.routes.splice(index, 1);
        if (index === currentIndex) {
          if (appStore.routes[index]) {
            router.push(appStore.routes[index]);
          } else {
            router.push(appStore.routes[index - 1]);
          }
        }
      }
    };

    const removeLeft = (index) => {
      const currentIndex = appStore.routes.findIndex((o) => o.fullPath === currentRoute.fullPath);
      const route = appStore.routes[index];
      appStore.routes.splice(0, index);
      if (currentIndex < index) {
        router.push(route);
      }
    };

    const removeRight = (index) => {
      const currentIndex = appStore.routes.findIndex((o) => o.fullPath === currentRoute.fullPath);
      appStore.routes.splice(index + 1, appStore.routes.length - index);
      if (currentIndex > index) {
        router.push(appStore.routes[index]);
      }
    };

    const removeOthers = (index) => {
      removeRight(index);
      removeLeft(index);
      if (appStore.routes[0].fullPath !== currentRoute.fullPath) {
        router.push(appStore.routes[0]);
      }
    };

    const onClick = (context) => {
      if (!context.active) {
        router.push(context.props.name);
      }
    };
    return {
      model,
      appStore,
      itemRefs,
      onBeforeRouteUpdate,
      setRef,
      showContextMenu,
      refresh,
      remove,
      removeLeft,
      removeRight,
      removeOthers,
      onClick,
    };
  },
};
