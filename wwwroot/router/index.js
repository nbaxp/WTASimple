import { createRouter, createWebHashHistory } from 'vue-router';
import layout from '../layouts/index.js';
import home from '../views/home.js';
import login from '../views/login.js';
import { useAppStore } from '../store/index.js';

import NProgress from '../lib/nprogress/nprogress.vite-esm.js';

NProgress.configure({ showSpinner: false });

const routes = [
    {
        path: '/',
        redirect: '/home',
        component: layout,
        meta: {
            title: 'home'
        },
        children: [
            {
                path: 'home',
                component: home,
            }
        ],
    },
    {
        path: '/login',
        component: login,
    },
];

const router = createRouter({
    history: createWebHashHistory(),
    routes,
});

router.beforeEach(async (to, from, next) => {
    NProgress.start();
    try {
        const appStore = useAppStore();
        if (to.path !== '/login' && !localStorage.getItem('access_token')) {
            next({ path: '/login', query: { redirect: to.fullPath } });
        } else {
            next();
        }
    } catch (error) {
        NProgress.done();
    }
});

router.afterEach((to, from) => {
    NProgress.done();
});

export default router;
