import { createRouter, createWebHashHistory } from "vue-router";
import { useTitle } from "@vueuse/core";
import NProgress from "../lib/nprogress/nprogress.vite-esm.js";
import { isLogin, hasPermission } from "../api/user.js";
import { useAppStore } from "../store/index.js";
import { listToTree } from "../utils/index.js";

NProgress.configure({ showSpinner: false });

const routes = [
  {
    name: "layout",
    path: "/",
    redirect: "/home",
    component: () => import("../layouts/index.js"),
    children: [
      {
        path: "home",
        component: () => import("../views/home.js"),
        meta: {
          title: "首页",
          icon: "home",
        },
      },
    ],
  },
  {
    path: "/login",
    component: () => import("../views/login.js"),
    meta: {
      title: "登录",
    },
  },
  {
    path: "/403",
    component: () => import("../views/403.js"),
    meta: {
      title: "权限不足",
    },
  },
  {
    path: "/:pathMatch(.*)*",
    component: () => import("../views/404.js"),
    meta: {
      title: "无法找到",
    },
  },
];

const router = createRouter({
  history: createWebHashHistory(),
  routes,
});

router.beforeEach(async (to, from, next) => {
  NProgress.start();
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

router.afterEach((to) => {
  try {
    if (to.meta.title) {
      useTitle().value = `${to.meta.title}`;
    }
  } finally {
    NProgress.done();
  }
});

const reset = (list, parent = null) => {
  return list.map((o) => {
    const item = {
      path: o.path,
      meta: o.meta,
    };
    if (o.type === "Resource") {
      item.component = import(`../views/${o.component ? o.component : "list"}.js`);
    }
    item.meta.path = `${parent === null ? "/" : parent.meta.path + "/"}${item.path}`;
    item.meta.fullName = `${parent === null ? "" : parent.meta.title + " > "}${item.meta.title}`;
    if (o.type === "Resource") {
      if (o.children.length) {
        item.meta.buttons = o.children.map((b) => {
          return {
            path: b.path,
            meta: b.meta,
          };
        });
      }
    } else if (o.type !== "Operation" && o.children.length) {
      item.children = reset(o.children, item);
    }
    return item;
  });
};

const refreshRouter = () => {
  const appStore = useAppStore();
  const permissions = appStore.user.permissions.filter((o) => !o.isHidden);
  const tree = reset(
    listToTree(permissions, (o) => {
      o.meta = {
        title: o.name,
        icon: o.icon,
        order: o.order,
        permission: o.number,
        isExternal: o.isExternal,
        isTop: o.isTop,
        htmlClass: o.htmlClass,
      };
    })
  );
  const route = {
    name: "layout",
    path: "/",
    redirect: "/home",
    component: () => import("../layouts/index.js"),
    children: tree,
  };
  router.removeRoute("layout");
  router.addRoute("/", route);
};
export default router;
export { refreshRouter };
