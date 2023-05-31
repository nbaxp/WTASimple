import { createPinia } from 'pinia';
import useAppStore from './app.js';

const store = createPinia();

export { useAppStore };
export default store;
