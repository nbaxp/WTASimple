import { createApp } from 'vue';
import store, { useAppStore } from './store/index.js';
import router from './router/index.js';
import ElementPlus from 'element-plus'

import App from '/app.js';

const app = createApp(App);

app.use(store);
app.use(router);
app.use(ElementPlus);

app.mount('#app');
