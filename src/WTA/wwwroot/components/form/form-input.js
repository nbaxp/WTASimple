import html from "html";
import { ref, reactive, watch } from "vue";
import { dayjs } from "element-plus";
import { post } from "../../request/index.js";

export default {
  template: html`
    <template v-if="getDisabled()">
      <template v-if="model[prop]!==null">
        <el-switch disabled v-model="model[prop]" type="checked" v-if="schema.type==='boolean'" />
        <template v-else-if="schema.format==='datetime'">{{dayjs(model[prop]).format('YYYY-MM-DD HH:mm:ss')}}</template>
        <template v-else-if="schema.format==='date'">{{dayjs(model[prop]).format('YYYY-MM-DD')}}</template>
        <template v-else>{{model[prop]}}</template>
      </template>
    </template>
    <template v-else>
      <template v-if="getInput(schema)==='select'">
        <el-select
          v-model="model[prop]"
          :placeholder="$t('select')"
          :multiple="!!schema.multiple"
          clearable
          style="width:100%"
          :title="JSON.stringify(options)"
        >
          <el-option v-for="item in options" :key="item.id" :label="item.name" :value="item.id" />
        </el-select>
      </template>
      <el-input
        :disabled="getDisabled()"
        :placeholder="schema.title"
        v-model="model[prop]"
        type="number"
        v-else-if="schema.type==='number'"
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
    const getInput = (schema) => {
      return schema.input ?? schema.type;
    };
    /*end*/
    const options = ref([]);
    if (props.schema.options) {
      options.value = props.schema.options;
    } else if (props.schema.url) {
      try {
        const url = `${props.schema.url}/index`;
        const result = await post(url, { queryAll: true });
        options.value = result.data?.items;
      } catch (error) {
        console.log(error);
      }
    }
    return {
      model,
      getDisabled,
      getInput,
      dayjs,
      options,
    };
  },
};
