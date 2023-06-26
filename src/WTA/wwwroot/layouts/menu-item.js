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
    <el-menu-item
      v-else-if="modelValue.meta.type==='Resource'"
      :index="modelValue.meta.isExternal?null:modelValue.meta.path"
      @click.native="click(modelValue)"
    >
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
    //
    const click = (route) => {
      if (!route.meta.isExternal) {
        router.push(route.meta.path);
      } else {
        window.open(route.path);
      }
    };
    //
    return {
      model,
      click,
    };
  },
};
