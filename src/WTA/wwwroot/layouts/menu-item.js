import html from "html";
import { reactive, watch } from "vue";
import Icon from "../components/icon/index.js";
import { useRouter } from "vue-router";

export default {
  name: "menuItem",
  components: { Icon },
  template: html`<el-sub-menu :index="modelValue.meta.path" v-if="modelValue.children">
      <template #title>
        <el-icon><icon :name="modelValue.meta.icon??'folder'" /></el-icon>
        <span>{{modelValue.meta.title}}</span>
      </template>
      <menu-item v-for="item in modelValue.children" v-model="item" />
    </el-sub-menu>
    <el-menu-item v-else :index="modelValue.meta.path">
      <el-icon><icon :name="modelValue.meta.icon??file" /></el-icon>
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
    const router = useRouter();
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
