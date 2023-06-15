import html, { getProp } from "html";
import AppForm from "../form/index.js";
import { get, post } from "../../request/index.js";
import { ref, reactive } from "vue";
import { useRoute } from "vue-router";
import { useI18n } from "vue-i18n";
import SvgIcon from "../../components/icon/index.js";
import { schemaToModel } from "../../utils/index.js";
import qs from "../../lib/qs/shim.js";
import AppFormInput from "../form/form-input.js";
import VueOfficeExcel from "@vue-office/excel";
import { camelCase, capitalize } from "lodash";

export default {
  components: { AppForm, SvgIcon, AppFormInput, VueOfficeExcel },
  template: html`
    <el-row>
      <el-col>
        <app-form
          inline
          mode="query"
          label-position="left"
          :schema="queryFromSchema"
          v-model="data.query"
          @submit="load"
          :hideButton="true"
          :isQueryForm="true"
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
        <slot name="tableButtons" :rows="selectedRows"></slot>
      </el-col>
    </el-row>
    <el-row>
      <el-col>
        <el-scrollbar>
          <el-table
            ref="tableRef"
            v-loading="tableLoading"
            row-key="id"
            table-layout="auto"
            border
            fit
            :data="data.items"
            @selection-change="handleSelectionChange"
            @sort-change="sortChange"
            :header-cell-class-name="getClass"
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
                <template v-if="showColumn(item,key)">
                  <el-table-column :prop="key" sortable="custom" :sort-orders="['descending', 'ascending', null]">
                    <template #header="scope">{{item.title}}</template>
                    <template #default="scope">
                      <app-form-input :isReadOnly="true" :schema="item" :prop="key" v-model="scope.row" />
                    </template>
                  </el-table-column>
                </template>
              </template>
            </template>
            <slot name="columns"></slot>
            <el-table-column fixed="right">
              <template #header>
                <el-button @click="filterDrawer = true">
                  {{$t('operations')}}
                  <el-icon class="el-icon--right"><ep-filter /></el-icon>
                </el-button>
              </template>
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
                  <slot name="rowButtons" :rows="[scope.row]"></slot>
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
    <el-drawer v-model="filterDrawer" destroy-on-close @close="tableRef.doLayout()">
      <template #header> <span class="el-dialog__title"> {{$t('filter')}} </span> </template>
      <el-scrollbar>
        <el-row>
          <el-col style="max-height:calc(100vh - 180px);">
            <el-form inline>
              <el-form-item>
                <el-button type="primary" @click="columns.forEach(o=>o.checked=true)"> {{$t('selectAll')}} </el-button>
              </el-form-item>
              <el-form-item>
                <el-button type="primary" @click="columns.forEach(o=>o.checked=!o.checked)">
                  {{$t('selectInverse')}}
                </el-button>
              </el-form-item>
              <el-form-item v-for="item in columns">
                <el-checkbox v-model="item.checked" :label="item.title" size="large" />
              </el-form-item>
            </el-form>
          </el-col>
        </el-row>
      </el-scrollbar>

      <template #footer>
        <span class="dialog-footer">
          <el-button type="primary" @click="filterDrawer=false"> {{$t('confirm')}} </el-button>
        </span>
      </template>
    </el-drawer>
    <el-dialog v-model="dialogVisible" align-center destroy-on-close width="700">
      <template #header> <span class="el-dialog__title"> {{editFormTitle}} </span> </template>
      <el-row>
        <el-col style="max-height:calc(100vh - 180px );">
          <el-scrollbar>
            <app-form
              v-loading="editFormloading"
              :disabled="editFormMode==='details'"
              :mode="editFormMode"
              ref="editFormRef"
              inline
              label-position="left"
              :hideButton="true"
              :schema="editFormSchema"
              v-model="editFormModel"
              v-if="editFormMode!=='import'"
            />
          </el-scrollbar>
        </el-col>
      </el-row>
      <template #footer>
        <span class="dialog-footer">
          <el-button type="primary" @click="submit"> {{$t('confirm')}} </el-button>
        </span>
      </template>
    </el-dialog>
  `,
  props: ["modelValue"],
  emits: ["command"],
  async setup(props, context) {
    const tableRef = ref(null);
    const columns = ref([]);
    const filterDrawer = ref(false);
    const tableLoading = ref(false);
    const selectedRows = ref([]);
    const dialogVisible = ref(false);
    const route = useRoute();
    const { t } = useI18n();
    const baseUrl = `${route.meta.path}`.substring(1);
    const indexUrl = `${baseUrl}/index`;
    const vm = (await get(indexUrl)).data;
    const schema = vm.schema;
    const data = reactive(vm.model ?? schemaToModel(schema));
    const sortColumns = ref(new Map());
    const getSortModel = (model) => {
      const orderBy = model.orderBy
        .split(",")
        .map((o) => o.trim())
        .filter((o) => o)
        .map((o) => ({
          prop: camelCase(o.split(" ")[0]),
          order: (o.split(" ").filter((o) => o)[1] ?? "asc") + "ending",
        }))
        .forEach((o) => sortColumns.value.set(o.prop, o.order));
      return orderBy;
    };
    const sortModel = reactive(getSortModel(data));
    const getClass = ({ row, column }) => {
      if (column.property) {
        column.order = sortColumns.value.get(column.property);
      }
    };
    const sortChange = ({ column, prop, order }) => {
      if (order === null) {
        sortColumns.value.delete(prop);
      } else {
        sortColumns.value.set(prop, order);
      }
      data.orderBy = Array.from(sortColumns.value)
        .map((o) => capitalize(o[0]) + (o[1] === "ascending" ? "" : ` DESC`))
        .join(",");
      load(indexUrl);
    };
    const getColumns = (schema) => {
      Object.keys(schema.properties).forEach((propertyName) => {
        const property = schema.properties[propertyName];
        if (property.type !== "object" && property.type !== "array" && !property.hidden) {
          columns.value.push({ name: propertyName, title: property.title, checked: true });
        }
      });
    };
    const showColumn = (item, prop) => {
      return (
        item.type !== "object" &&
        item.type !== "array" &&
        !item.hidden &&
        columns.value.findIndex((o) => o.name === prop && o.checked) >= 0
      );
    };
    getColumns(schema.properties.query);
    const queryFromSchema = schema.properties.query;
    const tableSchema = schema.properties.items;
    const editFormRef = ref(null);
    const editFormloading = ref(false);
    const editFormMode = ref(null);
    const editFormTitle = ref("");
    const editFormSchema = reactive({});
    const editFormModel = reactive({});
    const exportModel = reactive({
      includeAll: false,
      includeDeleted: false,
    });
    const handleSelectionChange = (rows) => (selectedRows.value = rows);
    const load = async (url) => {
      tableLoading.value = true;
      try {
        const postData = JSON.parse(JSON.stringify(data));
        delete postData["Id"];
        delete postData["items"];
        Object.assign(data, (await post(url, postData)).data);
      } catch (error) {
        console.log(error);
      } finally {
        tableLoading.value = false;
      }
    };
    const onPageIndexChange = () => load(indexUrl);
    const onPageSizeChange = () => load(indexUrl);
    const click = async (item, rows) => {
      editFormMode.value = item.path;
      context.emit("command", item, rows);
      if (item.path === "index") {
        //list
        await load(indexUrl);
      } else if (item.path === "details") {
        //details
        const detailsUrl = `${baseUrl}/${item.path}?${qs.stringify({ id: rows[0].id })}`;
        Object.assign(editFormSchema, schema.properties.items.items);
        Object.assign(editFormModel, (await post(detailsUrl)).data);
        editFormTitle.value = `${t("details")}${schema.title}`;
        dialogVisible.value = true;
      } else if (item.path === "create") {
        //create
        const url = `${baseUrl}/${item.path}`;
        const vm = (await get(url)).data;
        Object.assign(editFormSchema, vm.schema);
        Object.assign(editFormModel, vm.model);
        editFormTitle.value = `${t("update")}${schema.title}`;
        dialogVisible.value = true;
      } else if (item.path === "update") {
        //update
        const url = `${baseUrl}/${item.path}`;
        const vm = (await get(url, { id: rows[0].id })).data;
        Object.assign(editFormSchema, vm.schema);
        Object.assign(editFormModel, vm.model);
        editFormTitle.value = `${t("update")}${schema.title}`;
        dialogVisible.value = true;
      } else if (item.path === "delete") {
        //delete
        if (!rows.length) {
          return;
        }
        const url = `${baseUrl}/${item.path}`;
        await post(
          url,
          rows.map((o) => o.id)
        );
        await load(indexUrl);
      } else if (item.path === "export") {
        //export
        const url = `${baseUrl}/${item.path}`;
        const exportUrl = `${url}?${qs.stringify(exportModel)}`;
        await load(exportUrl);
      } else if (item.path === "import") {
        //import
        const url = `${baseUrl}/${item.path}`;
        editFormTitle.value = `${t("import")}${schema.title}`;
        dialogVisible.value = true;
      }
    };
    const submit = async () => {
      if (editFormMode.value === "create" || editFormMode.value === "update") {
        try {
          const valid = await editFormRef.value.validate();
          if (valid) {
            editFormloading.value = true;
            const url = `${baseUrl}/${editFormMode.value}`;
            const result = await post(url, editFormModel);
            if (result.errors) {
              model.errors = result.errors; //??
            } else {
              await load(indexUrl);
              editFormMode.value = null;
              dialogVisible.value = false;
            }
          }
        } catch (error) {
          console.error(error);
        } finally {
          editFormloading.value = false;
        }
      } else if (editFormMode.value === "details") {
        load(indexUrl);
        editFormMode.value = null;
        dialogVisible.value = false;
      }
    };
    await load(indexUrl);
    return {
      route,
      tableRef,
      tableLoading,
      columns,
      showColumn,
      filterDrawer,
      dialogVisible,
      selectedRows,
      schema,
      queryFromSchema,
      tableSchema,
      data,
      sortModel,
      getClass,
      sortChange,
      getProp,
      editFormRef,
      editFormMode,
      editFormTitle,
      editFormSchema,
      editFormModel,
      exportModel,
      onPageSizeChange,
      onPageIndexChange,
      handleSelectionChange,
      load,
      click,
      submit,
    };
  },
};
