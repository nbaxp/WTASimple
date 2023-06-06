import html from "html";
import { reactive, watch } from "vue";
import Icon from "../components/icon/index.js";

export default {
  components: { Icon },
  template: html`<el-sub-menu :index="modelValue.path" v-if="modelValue.children">
      <el-icon><icon v-if="modelValue.meta.icon" :name="modelValue.meta.icon" /></el-icon>
      <template #title>
        <span>{{modelValue.meta.title}}</span>
      </template>
      <menu-item v-for="item in modelValue.children" v-model="item" />
    </el-sub-menu>
    <el-menu-item :index="modelValue.path" v-else>
      <el-icon><icon v-if="modelValue.meta.icon" :name="modelValue.meta.icon" /></el-icon>
      <template #title>
        <span>{{modelValue.meta.title}}</span>
      </template>
    </el-menu-item>`,
  props: {
    modelValue: {
      typeof: Object,
    },
  },
  setup(props, context) {
    const model = reactive(props.modelValue);
    watch(
      model,
      (value) => {
        context.emit("update:modelValue", value);
      },
      { deep: true }
    );
    return {
      model,
    };
  },
};
