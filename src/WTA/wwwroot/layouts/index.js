import html from "html";
import LayoutHeader from "./header.js";
import LayoutMenu from "./menu.js";
import LayoutFooter from "./footer.js";
import Icon from "../components/icon/index.js";

export default {
  components: { Icon, LayoutHeader, LayoutMenu, LayoutFooter },
  template: html`<el-container>
    <el-header><layout-header /></el-header>
    <el-container>
      <el-aside width="auto">
        <el-scrollbar><layout-menu /></el-scrollbar>
      </el-aside>
      <el-container class="backtop">
        <el-scrollbar>
          <el-main>
            <router-view></router-view>
          </el-main>
          <el-footer>
            <layout-footer />
          </el-footer>
          <el-backtop target=".backtop > .el-scrollbar > .el-scrollbar__wrap" />
        </el-scrollbar>
      </el-container>
    </el-container>
  </el-container>`,
};
