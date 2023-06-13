import html from "html";
import { template } from "lodash";
import { ref, reactive, watch } from "vue";

const template = html` <template></template> `;

export default {
  template,
  props: ["modelValue"],
  emit: ["update:modelValue", "key", "schema"],
  async setup(props, context) {
    const model = reactive(props.modelValue);
    watch(model, (value) => {
      context.emit("update:modelValue", value);
    });
    /*start*/

    /*end*/
    return {
      model,
    };
  },
};
