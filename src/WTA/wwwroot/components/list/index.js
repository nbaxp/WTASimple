import html from "html";
import AppForm from "../form/index.js";
import { get, post } from "../../request/index.js";
import { useRoute } from "vue-router";

export default {
  components: { AppForm },
  template: html`<el-table
    :ref="tableRef"
    row-key="id"
    table-layout="auto"
    border
    highlight-current-row
    :data="data.items"
  >
    <el-table-column type="selection" align="center" fixed="left" width="60" />
    <el-table-column :label="$t('rowIndex')" type="index" align="center" fixed="left" width="60">
      <template #default="scope"> {{ (data.pageIndex - 1) * data.pageSize + scope.$index + 1 }} </template>
    </el-table-column>
    <template v-for="(item,key) in tableSchema.items.properties">
      <template v-if="key==='properties'">
        <el-table-column :label="subKey" v-for="(subItem,subKey) in properties">
          <template #default="scope">{{ scope.row[key][subKey] }} </template>
        </el-table-column>
      </template>
      <template v-else>
        <el-table-column :prop="key" :label="item.title" v-if="!item.isDisableDisplay">
          <template #default="scope">
            <el-switch disabled v-model="scope.row[key]" type="checked" v-if="item.type==='boolean'" />
            <template v-else>{{ scope.row[key] }}</template>
          </template>
        </el-table-column>
      </template>
    </template>
  </el-table>`,
  props: ["modelValue", "api"],
  async setup(props) {
    const route = useRoute();
    const url = `${route.meta.path}/index`.substring(1);
    const schema = (await get(url)).data;
    const tableSchema = schema.properties.items;
    const data = (await post(url)).data;
    return {
      route,
      schema,
      tableSchema,
      data,
    };
  },
};
