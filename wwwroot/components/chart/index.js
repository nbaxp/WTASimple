import { nextTick, ref } from 'vue';
import VCharts from 'vue-echarts';

const template = `<VCharts
  v-if="renderChart"
  :option="options"
  :autoresize="autoresize"
  :style="{ width, height }"
/>`;

export default {
  template,
  components: { VCharts },
  props: {
    options: {
      default: {},
    },
    autoresize: {
      default: true,
    },
    width: {
      default: '100%',
    },
    height: {
      default: '100%',
    },
  },
  setup() {
    const renderChart = ref(false);
    nextTick(() => {
      renderChart.value = true;
    });
    return {
      renderChart,
    };
  },
};
