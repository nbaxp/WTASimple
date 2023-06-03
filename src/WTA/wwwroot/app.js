import html from 'html';
import { ElConfigProvider } from 'element-plus';
import zhCn from './lib/element-plus/locale/zh-cn.min.mjs'
import { Suspense } from 'vue';

export default {
  components: { ElConfigProvider, Suspense },
  template: html`<suspense><el-config-provider :locale="locale">
    <router-view></router-view>
  </el-config-provider></suspense>`,
  setup() {
    return {
      locale: zhCn
    }
  }
}
