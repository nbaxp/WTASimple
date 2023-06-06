import html from "html";
import Icon from "../components/icon/index.js";
import {useAppStore} from "../store/index.js";

export default {
  components: { Icon },
  template: html`<el-menu :collapse="appStore.isMenuCollapse" :collapse-transition="false">
    <el-sub-menu index="1">
      <template #title>
        <el-icon><ep-location /></el-icon>
        <span>Navigator One</span>
      </template>
      <el-menu-item-group>
        <template #title><span>Group One</span></template>
        <el-menu-item index="1-1">item one</el-menu-item>
        <el-menu-item index="1-2">item two</el-menu-item>
      </el-menu-item-group>
      <el-menu-item-group title="Group Two">
        <el-menu-item index="1-3">item three</el-menu-item>
      </el-menu-item-group>
      <el-sub-menu index="1-4">
        <template #title><span>item four</span></template>
        <el-menu-item index="1-4-1">item one</el-menu-item>
      </el-sub-menu>
    </el-sub-menu>
    <el-menu-item index="2">
      <el-icon><ep-icon-menu /></el-icon>
      <template #title>Navigator Two</template>
    </el-menu-item>
    <el-menu-item index="3" disabled>
      <el-icon><ep-document /></el-icon>
      <template #title>Navigator Three</template>
    </el-menu-item>
    <el-menu-item index="4">
      <el-icon><ep-setting /></el-icon>
      <template #title>Navigator Four</template>
    </el-menu-item>
  </el-menu>`,
  setup() {
    const appStore = useAppStore();
    return {
      appStore,
    };
  },
};
