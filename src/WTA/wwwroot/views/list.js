import AppList from "../components/list/index.js";
import html from "html";
import router from "../router/index.js";

export default {
  components: { AppList },
  template: html`<el-scrollbar>
    <app-list @command="onCommand">
      <template #tableButtons="{rows}"><el-button>{{$t('test')}}</el-button></template>
      <template #rowButtons="{rows}"><el-button>{{$t('test')}}</el-button></template>
    </app-list>
  </el-scrollbar>`,
  setup() {
    console.log(router.currentRoute.value.fullPath);
    const onCommand = (item, rows) => {
      console.log(item.path, item, rows);
    };
    return { onCommand };
  },
};
