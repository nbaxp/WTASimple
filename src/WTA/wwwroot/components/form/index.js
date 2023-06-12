import html from "html";
import { ref, reactive, watch } from "vue";

export default {
  name: "AppForm",
  template: html`<el-form ref="formRef" :model="model" label-width="auto">
    <template v-for="(item,key) in schema.properties">
      <template v-if="!item.hidden&&item.type!=='array'">
        <el-form-item
          :label="item.title"
          :prop="getProp(key)"
          :rules="disableValid?[]:getRules(schema,item,model)"
          :error="disableValid?null:getError(key)"
        >
          <el-input :placeholder="item.title" v-model="model[key]" type="number" v-if="item.type==='number'" />
          <el-input-number
            :placeholder="item.title"
            v-model="model[key]"
            :precision="0"
            v-else-if="item.type==='integer'"
          />
          <template v-else-if="item.type==='boolean'">
            <el-select v-model="model[key]" :placeholder="item.title" v-if="item.nullable">
              <el-option key="select" :value="null" :label="$t('select')" />
              <el-option key="true" :value="true" :label="$t('true')" />
              <el-option key="false" :value="false" :label="$t('false')" />
            </el-select>
            <el-switch v-model="model[key]" type="checked" v-else />
          </template>
          <template v-else>
            <el-input
              :placeholder="item.title"
              v-model="model[key]"
              type="password"
              show-password
              v-if="item.format==='password'"
            />
            <el-input :placeholder="item.title" v-model="model[key]" type="text" v-else />
          </template>
        </el-form-item>
      </template>
    </template>
    <el-form-item v-if="!hideButton">
      <template #label></template>
      <el-button type="primary" @click="submit" :disabled="loading"><slot>$t('confirm')</slot></el-button>
    </el-form-item>
  </el-form>`,
  props: ["modelValue", "schema", "action", "hideButton", "disableValid"],
  emits: ["update:modelValue", "submit"],
  setup(props, context) {
    // init
    const model = reactive(props.modelValue);
    watch(
      model,
      (value) => {
        context.emit("update:modelValue", value);
      },
      { deep: true }
    );
    const errors = reactive({});
    // ref
    const formRef = ref(null);
    const loading = ref(false);
    //
    const getProp = (key) => {
      return key;
    };
    //
    const getError = (key) => {
      return errors[key];
    };
    //
    const getRules = (parentSchema, property, data) => {
      if (!property.rules) {
        return null;
      }
      const rules = [...(Array.isArray(property.rules) ? property.rules : [property.rules])].map((o) =>
        JSON.parse(JSON.stringify(o))
      );
      Object.values(rules).forEach((rule) => {
        rule.data = data;
        rule.schema = parentSchema;
        rule.title = rule.title ?? property.title;
        rule.type = property.type;
        if (rule.validator) {
          rule.validator = validators[rule.validator];
        }
        if (!rule.message) {
          if (rule.required) {
            rule.message = format(schema.messages.required, property.title);
          } else if (rule.pattern) {
            rule.message = format(schema.messages.pattern, property.title);
          } else if (property.type === "string" || property.type === "number" || property.type === "array") {
            if (rule.len) {
              rule.message = format(schema.messages[property.type].len, property.title, rule.len);
            } else if (rule.min) {
              rule.message = format(schema.messages[property.type].min, property.title, rule.min);
            } else if (rule.max) {
              rule.message = format(schema.messages[property.type].max, property.title, rule.max);
            } else if (rule.range) {
              rule.message = format(schema.messages[property.type].range, property.title, rule.range);
            }
          }
        }
      });
      return rules;
    };
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
        context.emit("submit");
        //}
      } catch (error) {
        console.error(error);
      } finally {
        loading.value = false;
      }
    };
    return {
      model,
      formRef,
      loading,
      errors,
      getProp,
      getError,
      getRules,
      reset,
      submit,
    };
  },
};
