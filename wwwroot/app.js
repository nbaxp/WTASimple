import html from 'html';
import { ElConfigProvider } from 'element-plus';
import zhCn from './lib/element-plus/locale/zh-cn.min.mjs'

export default {
  components: { ElConfigProvider },
  template: html`<el-config-provider :locale="locale">
    <router-view></router-view>
  </el-config-provider>`,
  setup() {
    return {
      locale: zhCn
    }
  }
}
