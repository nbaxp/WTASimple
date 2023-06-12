import html, { schemaToModel } from "html";
import { ref, reactive } from "vue";
import AppForm from "../components/form/index.js";
import { login } from "../api/user.js";
import { get } from "../request/index.js";
import LayoutLogo from "../layouts/logo.js";
import LayoutLocale from "../layouts/locale.js";
import LayoutFooter from "../layouts/footer.js";

export default {
  components: { AppForm, LayoutLogo, LayoutLocale, LayoutFooter },
  template: html`<el-container>
    <el-main style="display:flex;align-items: center;justify-content: center;">
      <div>
        <div class="flex">
          <layout-logo />
          <layout-locale />
        </div>
        <el-card class="box-card" style="width:400px;">
          <app-form :schema="schema" v-model="model" :action="action" @submit="submit">{{$t('login')}}</app-form>
        </el-card>
        <layout-footer />
      </div>
    </el-main>
  </el-container>`,
  async setup() {
    const vm = (await get("token/create")).data;
    const schema = reactive(vm.schema);
    const model = reactive(vm.model ?? schemaToModel(schema));
    const action = ref("token/create");
    const submit = async () => {
      const result = await login(action.value, model);
      if (result.errors) {
        model.errors = result.errors;
      }
    };
    return {
      schema,
      model,
      action,
      submit,
    };
  },
};
