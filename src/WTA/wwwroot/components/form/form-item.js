import html from "html";
import { ref, reactive, watch } from "vue";
import AppFormInput from "./form-input.js";

export default {
  components: { AppFormInput },
  template: html`
    <template v-if="showItem()">
      <template v-if="schema.type==='object'"></template>
      <template v-else-if="schema.type==='array'"> {{schema}} </template>
      <template v-else>
        <el-form-item
          :label="schema.title"
          :prop="getProp(prop)"
          :rules="getRules(parentSchema,schema,model)"
          :error="mode==='query'?null:getError(prop)"
        >
          <app-form-input :schema="schema" :prop="prop" v-model="model" :isReadOnly="mode==='details'" />
        </el-form-item>
      </template>
    </template>
  `,
  props: ["modelValue", "mode", "parentSchema", "schema", "prop", "errors"],
  emit: ["update:modelValue"],
  async setup(props, context) {
    const model = reactive(props.modelValue);
    watch(model, (value) => {
      context.emit("update:modelValue", value);
    });
    /*start*/
    const showItem = () => {
      if (props.schema.hidden) {
        return false;
      }
      if (props.schema.readOnly && (props.mode === "query" || props.mode === "create" || props.mode === "update")) {
        return false;
      }
      return true;
    };
    //
    const getProp = (prop) => {
      return prop;
    };
    //
    const getError = (prop) => {
      return props.errors[prop];
    };
    //
    const getRules = (parentSchema, property, data) => {
      if (props.mode === "query" || props.mode === "details" || !property.rules) {
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
    /*end*/
    return {
      model,
      showItem,
      getProp,
      getError,
      getRules,
    };
  },
};
