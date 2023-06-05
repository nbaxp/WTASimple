import html from "html";
import { useAppStore } from "../store/index.js";

export default {
  template: html`<router-link to="/" class="logo">
    <div class="flex">
      <img src="/assets/logo.svg" />
      <h1 v-if="!appStore.isMenuCollapse">{{ appStore.localization.resources.application }}</h1>
    </div>
  </router-link>`,
  setup() {
    const appStore = useAppStore();
    return {
      appStore,
    };
  },
};
