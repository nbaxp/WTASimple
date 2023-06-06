import { onMounted, ref } from "vue";

const template = `<component v-if="name.indexOf('ep-')===0" :is="name" /> <v-else g v-html="svg" /> `;

export default {
  props: {
    name: {
      default: "file",
    },
  },
  template,
  setup(props) {
    const svg = ref("");
    onMounted(async () => {
      try {
        const response = await fetch(`./assets/icons/${props.name}.svg`);
        svg.value = await response.text();
      } catch (error) {
        svg.value = error;
      }
    });
    return {
      svg,
    };
  },
};
