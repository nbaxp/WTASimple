import html, { getProp } from "html";
import AppForm from "../form/index.js";
import { get, post } from "../../request/index.js";
import { ref, reactive } from "vue";
import { useRoute } from "vue-router";

export default {
  components: { AppForm },
  template: html`
    <el-row>
      <el-col>
        <app-form>
          <template v-for="item in $route.meta.buttons">
            <el-button v-if="item.meta.isMultiple">{{item.meta.title}}</el-button>
          </template>
          <slot name="topButtons"></slot>
        </app-form>
      </el-col>
    </el-row>
    <el-row>
      <el-col>
        <el-scrollbar>
          <el-table :ref="tableRef" row-key="id" table-layout="fixed" border fit :data="data.items">
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
              <template v-else-if="item.navigation">
                <el-table-column :prop="key" :label="item.title">
                  <template #default="scope">{{getProp(scope.row,item.navigation)}}</template>
                </el-table-column>
              </template>
              <template v-else>
                <el-table-column :prop="key" :label="item.title" show-overflow-tooltip v-if="!item.hidden">
                  <template #default="scope">
                    <el-switch disabled v-model="scope.row[key]" type="checked" v-if="item.type==='boolean'" />
                    <el-date-picker disabled v-model="scope.row[key]" type="date" v-else-if="item.format==='date'" />
                    <el-date-picker
                      disabled
                      v-model="scope.row[key]"
                      type="datetime"
                      v-else-if="item.format==='datetime'"
                    />
                    <template v-else>{{ scope.row[key] }}</template>
                  </template>
                </el-table-column>
              </template>
            </template>
            <el-table-column :label="$t('operations')" fixed="right">
              <template #default="scope">
                <template v-for="item in $route.meta.buttons">
                  <el-button v-if="!item.meta.isMultiple">{{item.meta.title}}</el-button>
                </template>
                <slot name="rowButtons"></slot>
              </template>
            </el-table-column>
          </el-table>
        </el-scrollbar>
      </el-col>
    </el-row>
    <el-row>
      <el-col>
        <el-pagination
          v-if="data.pageSize<data.totalCount"
          v-model:currentPage="data.pageIndex"
          v-model:page-size="data.pageSize"
          class="justify-end"
          :background="true"
          layout="total, sizes, prev, pager, next, jumper"
          :total="data.total"
          @size-change="onPageSizeChange"
          @current-change="onPageIndexChange"
        />
      </el-col>
    </el-row>
  `,
  props: ["modelValue", "api"],
  async setup() {
    const tableRef = ref(null);
    const route = useRoute();
    const url = `${route.meta.path}/index`.substring(1);
    const schema = (await get(url)).data;
    const tableSchema = schema.properties.items;
    const data = reactive((await post(url)).data);
    return {
      route,
      schema,
      tableRef,
      tableSchema,
      data,
      getProp,
    };
  },
};
