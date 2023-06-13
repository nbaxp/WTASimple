import html from "html";
import { ref, reactive, watch } from "vue";

export default {
  template: html`
    <template v-if="getDisabled()">
      <el-switch disabled v-model="model[prop]" type="checked" v-if="schema.type==='boolean'" />
      <template v-else>{{model[prop]}}</template>
    </template>
    <template v-else>
      <el-input
        :disabled="getDisabled()"
        :placeholder="schema.title"
        v-model="model[prop]"
        type="number"
        v-if="schema.type==='number'"
      />
      <el-input-number
        :disabled="getDisabled()"
        :placeholder="schema.title"
        v-model="model[prop]"
        :precision="0"
        v-else-if="schema.type==='integer'"
      />
      <template v-else-if="schema.type==='boolean'">
        <el-select :disabled="getDisabled()" v-model="model[prop]" :placeholder="schema.title" v-if="schema.nullable">
          <el-option prop="select" :value="null" :label="$t('select')" />
          <el-option prop="true" :value="true" :label="$t('true')" />
          <el-option prop="false" :value="false" :label="$t('false')" />
        </el-select>
        <el-switch v-model="model[prop]" type="checked" v-else />
      </template>
      <template v-else>
        <el-input
          :disabled="getDisabled()"
          :placeholder="schema.title"
          v-model="model[prop]"
          type="password"
          show-password
          v-if="schema.format==='password'"
        />
        <el-input :disabled="getDisabled()" :placeholder="schema.title" v-model="model[prop]" type="text" v-else />
      </template>
    </template>
  `,
  props: ["modelValue", "schema", "prop", "isReadOnly"],
  emit: ["update:modelValue"],
  async setup(props, context) {
    const model = reactive(props.modelValue);
    watch(model, (value) => {
      context.emit("update:modelValue", value);
    });
    /*start*/
    const getDisabled = () => {
      if (props.isReadOnly && props.isReadOnly === true) {
        return true;
      }
      if (props.schema.displayOnly) {
        return true;
      }
      if (props.mode === "update" && props.schema.addOnly) {
        return true;
      }
      return false;
    };
    /*end*/
    return {
      model,
      getDisabled,
    };
  },
};
