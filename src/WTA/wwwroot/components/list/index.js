import html, { getProp } from "html";
import AppForm from "../form/index.js";
import { get, post } from "../../request/index.js";
import { ref, reactive } from "vue";
import { useRoute } from "vue-router";
import { useI18n } from "vue-i18n";
import SvgIcon from "../../components/icon/index.js";
import { schemaToModel } from "../../utils/index.js";
import qs from "../../lib/qs/shim.js";
import VueOfficeExcel from "@vue-office/excel";

export default {
  components: { AppForm, SvgIcon, VueOfficeExcel },
  template: html`
    <el-row>
      <el-col>
        <app-form
          inline="inline"
          label-position="left"
          :schema="queryFromSchema"
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
          <el-button
            :class="item.meta.htmlClass??'el-button--primary'"
            v-if="item.meta.isTop"
            @click="click(item,selectedRows)"
          >
            <el-icon v-if="item.meta.icon"><svg-icon :name="item.meta.icon" /></el-icon>
            <span>{{item.meta.title}}</span>
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
            v-if="data.items"
          >
            <el-table-column fixed="left" type="selection" />
            <el-table-column type="index" :label="$t('rowIndex')">
              <template #default="scope"> {{ (data.pageIndex - 1) * data.pageSize + scope.$index + 1 }} </template>
            </el-table-column>
            <template v-for="(item,key) in tableSchema.items.properties">
              <template v-if="key==='properties'">
                <el-table-column :label="subKey" v-for="(subItem,subKey) in item.properties">
                  <template #default="scope">{{ scope.row[key][subKey] }} </template>
                </el-table-column>
              </template>
              <template v-else-if="item.navigation">
                <el-table-column :prop="key" :label="item.title">
                  <template #default="scope">{{getProp(scope.row,item.navigation)}}</template>
                </el-table-column>
              </template>
              <template v-else>
                <template v-if="!item.hidden&&item.type!=='array'">
                  <el-table-column :prop="key" :label="item.title" show-overflow-tooltip>
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
            </template>
            <el-table-column fixed="right" :label="$t('operations')">
              <template #default="scope">
                <div class="flex">
                  <template v-for="item in $route.meta.buttons">
                    <el-button
                      :class="item.meta.htmlClass??'el-button--primary'"
                      v-if="!item.meta.isTop"
                      @click="click(item,[scope.row])"
                    >
                      <el-icon v-if="item.meta.icon"><svg-icon :name="item.meta.icon" /></el-icon>
                      <span>{{item.meta.title}}</span>
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
          v-if="data.items&&data.pageSize<data.totalCount"
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
    <el-dialog v-model="dialogVisible" align-center destroy-on-close>
      <template #header> <span class="el-dialog__title"> {{editFormTitle}} </span> </template>
      <el-row>
        <el-col style="height:calc(90vh - 180px );">
          <el-scrollbar>
            <app-form inline="inline" label-position="left" :hideButton="true" v-if="editFormSchema.properties" />
          </el-scrollbar>
        </el-col>
      </el-row>
      <template #footer>
        <span class="dialog-footer">
          <el-button type="primary" @click="dialogVisible = false"> {{$t('confirm')}} </el-button>
        </span>
      </template>
    </el-dialog>
  `,
  props: ["modelValue"],
  emits: ["command"],
  async setup(props, context) {
    const tableRef = ref(null);
    const selectedRows = ref([]);
    const dialogVisible = ref(false);
    const route = useRoute();
    const { t } = useI18n();
    const url = `${route.meta.path}/index`.substring(1);
    const vm = (await get(url)).data;
    const schema = vm.schema;
    const data = reactive(vm.model ?? schemaToModel(schema));
    const queryFromSchema = schema.properties.query;
    const tableSchema = schema.properties.items;
    const editFormTitle = ref("");
    const editFormSchema = reactive({});
    const editFormModel = reactive({});
    const exportModel = reactive({
      includeAll: false,
      includeDeleted: false,
    });
    const handleSelectionChange = (rows) => (selectedRows.value = rows);
    const load = async (url) => {
      const postData = JSON.parse(JSON.stringify(data));
      delete postData["Id"];
      delete postData["items"];
      Object.assign(data, (await post(url, postData)).data);
    };
    const click = async (item, rows) => {
      console.log(item, rows);
      context.emit("command", item, rows);
      if (item.path === "index") {
        await load(url);
      } else if (item.path === "details") {
        const url = `${route.meta.path}/${item.path}`.substring(1);
        const detailsUrl = `${url}?${qs.stringify(rows[0].id)}`;
        editFormTitle.value = `${t("details")}${schema.title}`;
        dialogVisible.value = true;
      } else if (item.path === "create") {
        const url = `${route.meta.path}/${item.path}`.substring(1);
        const vm = await get(url);
        editFormTitle.value = `${t("create")}${schema.title}`;
        dialogVisible.value = true;
      } else if (item.path === "update") {
        const url = `${route.meta.path}/${item.path}`.substring(1);
        const vm = await get(url, { id: rows[0].id });
        editFormTitle.value = `${t("update")}${schema.title}`;
        dialogVisible.value = true;
      } else if (item.path === "delete") {
        if (!rows.length) {
          return;
        }
        const url = `${route.meta.path}/${item.path}`.substring(1);
        await post(
          url,
          rows.map((o) => o.id)
        );
        await load(url);
      } else if (item.path === "export") {
        const url = `${route.meta.path}/${item.path}`.substring(1);
        const exportUrl = `${url}?${qs.stringify(exportModel)}`;
        await load(exportUrl);
      } else if (item.path === "import") {
        const url = `${route.meta.path}/${item.path}`.substring(1);
        const exportUrl = `${url}?${qs.stringify(exportModel)}`;
        editFormTitle.value = `${t("import")}${schema.title}`;
        dialogVisible.value = true;
      }
    };
    await load(url);
    return {
      route,
      tableRef,
      dialogVisible,
      selectedRows,
      schema,
      queryFromSchema,
      tableSchema,
      data,
      getProp,
      editFormTitle,
      editFormSchema,
      editFormModel,
      exportModel,
      onPageSizeChange() {},
      onPageIndexChange() {},
      handleSelectionChange,
      load,
      click,
    };
  },
};
