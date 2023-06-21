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
      if (props.name.indexOf("ep-") !== 0) {
        try {
          const response = await fetch(`./assets/icons/${props.name}.svg`);
          if (response.ok && response.status === 200) {
            svg.value = await response.text();
          }
        } catch (error) {
          console.error(error);
        }
        if (!svg.value) {
          const response = await fetch("./assets/icons/file.svg");
          svg.value = await response.text();
        }
      }
    });
    return {
      svg,
    };
  },
};
