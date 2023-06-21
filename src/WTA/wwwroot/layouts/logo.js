import html from "html";
import { useAppStore } from "../store/index.js";

export default {
  template: html`<router-link to="/" class="logo">
    <div class="flex h-full items-center">
      <img src="/assets/logo.svg" />
      <h1 v-if="!appStore.isMenuCollapse">{{$t('application')}}</h1>
    </div>
  </router-link>`,
  setup() {
    const appStore = useAppStore();
    return {
      appStore,
    };
  },
};
