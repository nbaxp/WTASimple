import html from "html";
import { ElConfigProvider } from "element-plus";
import zh from "./lib/element-plus/locale/zh-cn.min.mjs";
import en from "./lib/element-plus/locale/en.min.mjs";
import { Suspense, reactive } from "vue";

export default {
  components: { ElConfigProvider, Suspense },
  template: html`<suspense>
    <el-config-provider :locale="localeMap.get($i18n.locale)">
      <router-view></router-view>
    </el-config-provider>
    <template #fallback> Loading... </template>
  </suspense>`,
  setup() {
    const localeMap = reactive(
      new Map([
        ["zh", zh],
        ["en", en],
      ])
    );
    return {
      localeMap,
    };
  },
};
