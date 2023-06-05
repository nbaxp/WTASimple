import html, { schemaToModel } from "html";
import { reactive } from "vue";
import AppForm from "../components/form/index.js";
import { login } from "../api/user.js";
import { get } from "../request/index.js";
import LayoutLogo from "../layouts/logo.js";
import LayoutFooter from "../layouts/footer.js";

export default {
  components: { AppForm, LayoutLogo, LayoutFooter },
  template: html`<el-container>
    <el-main style="display:flex;align-items: center;justify-content: center;">
      <div>
        <layout-logo />
        <el-card class="box-card" style="width:400px;">
          <app-form v-model="model" label-width="80px" @submit="submit">登录</app-form>
        </el-card>
        <layout-footer />
      </div>
    </el-main>
  </el-container>`,
  async setup() {
    const schema = (await get("token/create")).data;
    // const schema = {
    //   title: '登录',
    //   type: 'object',
    //   properties: {
    //     UserName: {
    //       title: '用户名'
    //     },
    //     Password: {
    //       title: '密码'
    //     },
    //     RememberMe: {
    //       title: '记住我',
    //       type: 'boolean'
    //     }
    //   }
    // };
    const model = reactive({
      schema,
      data: schemaToModel(schema),
      errors: {},
      action: "token/create",
    });
    const submit = async () => {
      const result = await login(model.action, model.data);
      if (result.errors) {
        //Object.assign(model.errors, result.errors);
        model.errors = result.errors;
      }
    };
    return {
      model,
      submit,
    };
  },
};
