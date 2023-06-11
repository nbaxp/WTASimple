import AppList from "../components/list/index.js";
import html from "html";
import router from "../router/index.js";

export default {
  components: { AppList },
  template: html`<el-scrollbar>
    <app-list @command="onCommand">
      <template v-slot:topButtons><el-button>{{$t('test')}}</el-button></template>
      <template v-slot:rowButtons><el-button>{{$t('test')}}</el-button></template>
    </app-list>
  </el-scrollbar>`,
  setup() {
    console.log(router.currentRoute.value.fullPath);
    const onCommand = (item, rows) => {
      alert(item.path);
    };
    return { onCommand };
  },
};
