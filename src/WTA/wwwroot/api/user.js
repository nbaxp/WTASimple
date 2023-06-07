import router from "../router/index.js";
import { post } from "../request/index.js";
import jwt_decode from "../lib/jwt-decode/jwt-decode.esm.js";
import qs from "../lib/qs/shim.js";
import { useAppStore } from "../store/index.js";
import { refreshRouter } from "../router/index.js";
import Enumerable from "linq";

const isLogin = async () => {
  const appStore = useAppStore();
  // 有 token，判断是否过期，失败设置 token 为 null
  if (appStore.token) {
    var exp = new Date(jwt_decode(appStore.token).exp * 1000);
    if (exp > new Date()) {
      return true;
    } else {
      appStore.token = null;
    }
  }
  // 有 refresh token，获取 token，失败删除 refresh token
  const refreshToken = getRefreshToken();
  if (refreshToken) {
    const refreshRespoonse = await fetch("/api/token/refresh", {
      method: "POST",
      body: qs.stringify({ refreshToken }),
      headers: {
        "Content-Type": "application/x-www-form-urlencoded",
      },
    });
    if (refreshRespoonse.ok) {
      const refreshResult = await refreshRespoonse.json();
      // 设置 token 并同时更新 refresh token
      appStore.token = refreshResult.access_token;
      setRefreshToken(refreshResult.refresh_token);
      return true;
    } else {
      removeRefreshToken();
    }
  }
  return false;
};

const login = async (action, data) => {
  const appStore = useAppStore();
  const result = await post(action, data, null, true);
  if (!result.errors) {
    appStore.token = result.data.access_token;
    setRefreshToken(result.data.refresh_token);
    appStore.user = await getUser();
    refreshRouter();
    var redirect = router.currentRoute.value.query?.redirect ?? "/";
    router.push(redirect);
  }
  return result;
};

const logout = () => {
  const appStore = useAppStore();
  appStore.token = null;
  removeRefreshToken();
  router.push({ path: "/login", query: { redirect: router.currentRoute.value.fullPath } });
};

const getUser = async () => {
  const result = await post("user/info");
  const user = result.data;
  user.roles = Enumerable.from(user.userRoles)
    .select((o) => o.role)
    .toArray();
  user.permissions = Enumerable.from(user.roles)
    .selectMany((o) => o.rolePermissions)
    .select((o) => o.permission)
    .orderBy((o) => o.order)
    .toArray();
  return user;
};

const hasPermission = (to) => {
  const appStore = useAppStore();
  const permission = to.meta?.permission;
  if (permission) {
    const hasPermission = Enumerable.from(appStore.user.permissions).any((o) => o.number === permission);
    return hasPermission;
  } else {
    return true;
  }
};

const refreshTokenKey = "refresh_token";

const getRefreshToken = () => localStorage.getItem(refreshTokenKey);

const setRefreshToken = (refreshToken) => localStorage.setItem(refreshTokenKey, refreshToken);

const removeRefreshToken = () => localStorage.removeItem(refreshTokenKey);

export { isLogin, login, logout, getUser, hasPermission };
