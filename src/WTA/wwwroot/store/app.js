import { defineStore } from "pinia";
import settings from "../config/settings.js";
import { getUser, isLogin } from "../api/user.js";
import { get, post } from "../request/index.js";
import { refreshRouter } from "../router/index.js";

const useAppStore = defineStore("app", {
  state: () => {
    const state = {
      settings: { ...settings },
      isMenuCollapse: false,
    };
    var localSettings = JSON.parse(localStorage.getItem("settings") ?? "{}");
    Object.assign(state.settings, localSettings);
    return state;
  },
  actions: {
    async init() {
      // 获取站点信息
      const result = await get("localization", null, null, true);
      this.localization = result.data;
      // 获取用户信息
      if (await isLogin()) {
        this.user = await getUser();
        refreshRouter();
      }
    },
  },
});

export default useAppStore;
