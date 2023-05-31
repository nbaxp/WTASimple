import { defineStore } from 'pinia';
import settings from '../config/settings.js';

const useAppStore = defineStore('app', {
  state: () => {
    const state = {
      settings: { ...settings },
    };
    var localSettings = JSON.parse(localStorage.getItem('settings') ?? '{}');
    Object.assign(state.settings, localSettings);
    return state;
  },
  actions: {
    resetSettings() {
      Object.assign(this.settings, settings);
    },
  },
});

export default useAppStore;
