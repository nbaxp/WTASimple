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
        <app-form
          inline="inline"
          label-position="left"
          :schema="formSchema"
          v-model="data.query"
          @submit="load"
          :hideButton="true"
          :disableValid="true"
        />
      </el-col>
    </el-row>
    <el-row style="padding-bottom:20px;">
      <el-col>
        <template v-for="item in $route.meta.buttons">
          <el-button type="primary" v-if="item.meta.isTop" @click="click(item,selectedRows)">
            {{item.meta.title}}
          </el-button>
        </template>
      </el-col>
    </el-row>
    <el-row>
      <el-col>
        <el-scrollbar>
          <el-table
            ref="tableRef"
            row-key="id"
            table-layout="auto"
            border
            fit
            :data="data.items"
            @selection-change="handleSelectionChange"
          >
            <el-table-column fixed="left" type="selection" />
            <el-table-column type="index" :label="$t('rowIndex')">
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
            <el-table-column fixed="right" :label="$t('operations')">
              <template #default="scope">
                <div class="flex">
                  <template v-for="item in $route.meta.buttons">
                    <el-button type="primary" v-if="!item.meta.isTop" @click="click(item,[scope.row])">
                      {{item.meta.title}}
                    </el-button>
                  </template>
                  <slot name="rowButtons"></slot>
                </div>
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
          :total="data.totalCount"
          :page-sizes="[20, 50, 100]"
          class="justify-end"
          :background="true"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="onPageSizeChange"
          @current-change="onPageIndexChange"
          style="margin-top:20px"
        />
      </el-col>
    </el-row>
  `,
  props: ["modelValue"],
  emits: ["command"],
  async setup(props, context) {
    const tableRef = ref(null);
    const selectedRows = ref([]);
    const route = useRoute();
    const url = `${route.meta.path}/index`.substring(1);
    const schema = (await get(url)).data;
    const formSchema = schema.properties.query;
    const tableSchema = schema.properties.items;
    const data = reactive((await post(url)).data);
    const handleSelectionChange = (rows) => (selectedRows.value = rows);
    const load = async () => {};
    const click = (item, data) => {
      console.log(item, data);
      context.emit("command", item, data);
    };
    return {
      route,
      tableRef,
      selectedRows,
      formSchema,
      tableSchema,
      data,
      getProp,
      onPageSizeChange() {},
      onPageIndexChange() {},
      handleSelectionChange,
      load,
      click,
    };
  },
};
