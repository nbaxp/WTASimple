import { createI18n } from "vue-i18n";
import { useAppStore } from "../store/index.js";

function useLocale() {
  const appStore = useAppStore();
  const i18n = createI18n({
    legacy: false,
    ...appStore.localization,
  });
  return i18n;
}

export default useLocale;
