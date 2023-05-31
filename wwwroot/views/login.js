import html, { schemaToModel } from 'html';
import { reactive } from 'vue';
import AppForm from '../components/form/index.js'
import {post} from '../request/index.js';
import router from '../router/index.js';

export default {
  components: { AppForm },
  template: html`<el-container>
    <el-main style="display:flex;align-items: center;justify-content: center;">
      <el-card class="box-card" style="width:400px;">
        <app-form v-model="model" label-width="80px" @submit="submit">登录</app-form>
      </el-card>
    </el-main>
</el-container>`,
  setup() {
    const schema = {
      title: '登录',
      type: 'object',
      properties: {
        UserName: {
          title: '用户名'
        },
        Password: {
          title: '密码'
        },
        RememberMe: {
          title: '记住我',
          type: 'boolean'
        }
      }
    };
    const model = reactive({
      schema,
      data: schemaToModel(schema),
      action:'/token/create'
    });
    const submit =async () => {
      const result = await post(model.action,model.data);
      localStorage.setItem('access_token',result.access_token);
      localStorage.setItem('refresh_token',result.refresh_token);
      const userInfo = await post('/')
      var redirect =router.currentRoute.value.query?.redirect??'/'
      router.push(redirect);
    };
    return {
      model,
      submit
    }
  }
}
