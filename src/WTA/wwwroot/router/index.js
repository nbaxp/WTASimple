import { createRouter, createWebHashHistory } from 'vue-router';
import layout from '../layouts/index.js';
import home from '../views/home.js';
import login from '../views/login.js';

import NProgress from '../lib/nprogress/nprogress.vite-esm.js';
import { isLogin } from '../api/user.js';
import useAppStore from '../store/app.js';

NProgress.configure({ showSpinner: false });

const routes = [
  {
    path: '/',
    redirect: '/home',
    component: layout,
    children: [
      {
        path: 'home',
        component: home,
        meta: {
          title: '首页',
        },
      },
    ],
  },
  {
    path: '/login',
    component: login,
    meta: {
      title: '登录',
    },
  },
];

const router = createRouter({
  history: createWebHashHistory(),
  routes,
});

router.beforeEach(async (to, from, next) => {
  NProgress.start();
  const appStore = useAppStore();
  try {
    if (to.path !== '/login' && !isLogin()) {
      next({ path: '/login', query: { redirect: to.fullPath } });
    } else {
      if (to.path !== '/login' && !appStore.user) {
        await appStore.getUser();
        if (!appStore.user) {
          next({ path: '/login', query: { redirect: to.fullPath } });
        }
      }
      next();
    }
  } catch (error) {
    NProgress.done();
  }
});

router.afterEach((to, from) => {
  try {
    if (to.meta.title) {
      document.title = `${to.meta.title}`;
    }
  } finally {
    NProgress.done();
  }
});

export default router;