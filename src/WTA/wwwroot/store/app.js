import { defineStore } from 'pinia';
import settings from '../config/settings.js';
import { getUser } from '../api/user.js';
import { get } from '../request/index.js';

const useAppStore = defineStore('app', {
  state: () => {
    const state = {
      settings: { ...settings },
      isMenuCollapse: false,
    };
    var localSettings = JSON.parse(localStorage.getItem('settings') ?? '{}');
    Object.assign(state.settings, localSettings);
    return state;
  },
  actions: {
    async init() {
      const result = await get('localization');
      this.localization = result.data;
      //const user = await get('user/info');
    },
    resetSettings() {
      Object.assign(this.settings, settings);
    },
    async getUser() {
      this.user = await getUser();
    },
  },
});

export default useAppStore;
