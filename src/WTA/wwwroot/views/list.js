import AppList from "../components/list/index.js";
import html from "html";
import router from "../router/index.js";

export default {
  components: { AppList },
  template: html`<el-scrollbar>
    <app-list @command="onCommand">
      <template #columns="scope">
        <el-table-column label="自定义列测试1" prop="id">
          <template #default="scope"> {{scope.row.id}} </template>
        </el-table-column>
        <el-table-column label="自定义列测试2">
          <template #default="scope"> {{scope.row.parent?.id}} </template>
        </el-table-column>
      </template>
      <template #tableButtons="{rows}">
        <el-button class="el-button--primary" @click="()=>(console.log(rows))">{{$t('test')}}</el-button>
      </template>
      <template #rowButtons="{rows}">
        <el-button class="el-button--primary" @click="()=>(console.log(rows))">{{$t('test')}}</el-button>
      </template>
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
