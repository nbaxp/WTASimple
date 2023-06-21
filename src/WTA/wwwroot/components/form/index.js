import html from "html";
import { ref, reactive, watch } from "vue";
import AppFormItem from "./form-item.js";

export default {
  components: { AppFormItem },
  name: "AppForm",
  template: html`<el-form ref="formRef" :model="model" label-width="auto">
    <template v-for="(value, prop) in schema.properties">
      <app-form-item
        :parentSchema="schema"
        :schema="value"
        v-model="model"
        :prop="prop"
        :mode="mode"
        :errors="errors"
      />
    </template>
    <slot name="items"></slot>
    <el-form-item v-if="!hideButton">
      <template #label></template>
      <el-button type="primary" @click="submit" :disabled="loading"><slot>$t('confirm')</slot></el-button>
    </el-form-item>
  </el-form>`,
  props: ["modelValue", "schema", "action", "hideButton", "isQueryForm", "mode"],
  emits: ["update:modelValue", "submit"],
  setup(props, context) {
    // init
    const model = reactive(props.modelValue);
    watch(model, (value) => {
      context.emit("update:modelValue", value);
    });
    // ref
    const formRef = ref(null);
    const loading = ref(false);
    //
    const errors = ref({});
    // reset
    const reset = () => {
      formRef.value.resetFields();
    };
    // validate
    const validate = async () => {
      return formRef.value.validate();
    };
    // submit
    const submit = async () => {
      try {
        //const valid = await validate();
        //if (valid) {
        loading.value = true;
        context.emit("submit", (serverErrors) => {
          if (serverErrors) {
            errors.value = serverErrors;
          }
        });
        //}
      } catch (error) {
        console.error(error);
      } finally {
        loading.value = false;
      }
    };
    // expose
    context.expose({ validate, reset });
    return {
      model,
      formRef,
      loading,
      errors,
      reset,
      submit,
    };
  },
};
