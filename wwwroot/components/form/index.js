import html from 'html';
import { reactive } from 'vue';

export default {
  name:'AppForm',
  template: html`<el-form :model=model>
      <template v-for="(item,key) in schema.properties">
        <el-form-item :label="item.title+'：'">
          <el-input :placeholder="item.title" v-model="model[key]" type="number" v-if="item.type==='number'" />
          <el-input-number :placeholder="item.title" v-model="model[key]" :precision="0" v-else-if="item.type==='integer'" />
          <el-switch v-model="model[key]" type="checked" v-else-if="item.type==='boolean'"/>
          <el-input :placeholder="item.title" v-model="model[key]" type="text" v-else />
        </el-form-item>
      </template>
      <el-form-item>
        <el-button type="primary" @click="onSubmit"><slot>确定</slot></el-button>
      </el-form-item>
    </el-form>`,
  props: {
    modelValue: { type: Object },
  },
  emits: ['submit'],
  setup(props, context) {
    const model = reactive(props.modelValue.data);
    const schema = reactive(props.modelValue.schema);
    const onSubmit = () => {
      context.emit('submit');
    };
    return {
      schema,
      model,
      onSubmit
    }
  }
}
