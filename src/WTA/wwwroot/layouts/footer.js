import html from "html";
import { useAppStore } from "../store/index.js";

export default {
  template: html`<div class="footer flex items-center justify-center">{{$t('copyright')}}</div>`,
  setup() {
    const appStore = useAppStore();
    return {
      appStore,
    };
  },
};
