import { createRouter, createWebHashHistory } from "vue-router";
import Layout from "../layouts/index.js";
import Home from "../views/home.js";
import Login from "../views/login.js";
import Forbidden from "../views/403.js";
import { useTitle } from "@vueuse/core";
import NProgress from "../lib/nprogress/nprogress.vite-esm.js";
import { isLogin, hasPermission } from "../api/user.js";
import { useAppStore } from "../store/index.js";

NProgress.configure({ showSpinner: false });

const routes = [
  {
    path: "/",
    redirect: "/home",
    component: Layout,
    children: [
      {
        path: "home",
        component: Home,
        meta: {
          title: "首页",
        },
      },
    ],
  },
  {
    path: "/login",
    component: Login,
    meta: {
      title: "登录",
    },
  },
  {
    path: "/403",
    component: Forbidden,
    meta: {
      title: "权限不足",
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
    if (to.path !== "/login") {
      if (!(await isLogin())) {
        next({ path: "/login", query: { redirect: to.fullPath } });
      } else {
        if (!hasPermission(to)) {
          next({ path: "/403", query: { redirect: to.fullPath } });
        } else {
          next();
        }
      }
    } else {
      next();
    }
  } catch (error) {
    NProgress.done();
  }
});

router.afterEach((to, from) => {
  try {
    if (to.meta.title) {
      useTitle().value = `${to.meta.title}`;
    }
  } finally {
    NProgress.done();
  }
});

const refreshRouter = () => {
  const appStore = useAppStore();
  console.log(appStore.user);
};
export default router;
export { refreshRouter };
