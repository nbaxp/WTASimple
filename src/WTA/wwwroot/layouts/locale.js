import html from "html";
import { useAppStore } from "../store/index.js";
import { useI18n } from "vue-i18n";
import Icon from "../components/icon/index.js";
export default {
  components: { Icon },
  template: html`<el-dropdown class="cursor-pointer" v-if="appStore.settings.enableLocale">
    <span class="el-dropdown-link flex">
      <el-icon :size="18">
        <icon name="lang" />
      </el-icon>
    </span>
    <template #dropdown>
      <el-dropdown-menu>
        <el-dropdown-item v-for="locale in $i18n.availableLocales" @click="changeLocale(locale)">
          {{appStore.localization.options.find(o=>o.value===locale).label}}
          <el-icon class="el-icon--right" v-if="locale===$i18n.locale">
            <ep-select />
          </el-icon>
        </el-dropdown-item>
      </el-dropdown-menu>
    </template>
  </el-dropdown>`,
  setup() {
    const appStore = useAppStore();
    const i18n = useI18n();
    const changeLocale = (locale) => {
      appStore.localization.locale = locale;
      i18n.locale.value = locale;
    };
    return {
      appStore,
      changeLocale,
    };
  },
};
