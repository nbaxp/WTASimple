import qs from "../lib/qs/shim.js";
import { isLogin } from "../api/user.js";
import { useAppStore } from "../store/index.js";

const requestSettings = {
  baseURL: "/api",
};

const addToken = async (options) => {
  if (await isLogin()) {
    const appStore = useAppStore();
    options.headers ??= {};
    options.headers.Authorization = `Bearer ${appStore.token}`;
  }
};

const getUrl = (url) => {
  if (url.indexOf("/") === 0) {
    return url;
  }
  return `${requestSettings.baseURL}/${url}`;
};

const getResult = async (response) => {
  const messages = new Map([
    [200, "操作成功"],
    [201, "已创建"],
    [204, "无返回值"],
    [400, "请求参数错误"],
    [401, "未登录"],
    [403, "权限不足"],
    [500, "服务器异常"],
  ]);
  const result = {
    status: response.status,
    message: messages.get(response.status),
  };
  if (response.status == 200) {
    result.data = await response.json();
  } else if (response.status === 400 || response.status === 500) {
    result.errors = await response.json();
  }
  return result;
};

const get = async (url, data, options, withoutToken = false) => {
  url = getUrl(url);
  if (data) {
    url = `${url}?${qs.stringify(data)}`;
  }
  const defaultOptions = {
    headers: {},
  };
  if (options) {
    Object.assign(defaultOptions, options);
  }
  if (!withoutToken) {
    await addToken(defaultOptions);
  }
  const response = await fetch(url, defaultOptions);
  return getResult(response);
};

const post = async (url, data, options, withoutToken = false) => {
  url = getUrl(url);
  const defaultOptions = {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
  };
  if (options) {
    Object.assign(defaultOptions, options);
  }
  if (data && !defaultOptions.body) {
    if (defaultOptions.headers["Content-Type"] === "application/x-www-form-urlencoded") {
      defaultOptions.body = qs.stringify(data);
    } else {
      defaultOptions.body = JSON.stringify(data);
    }
  }
  if (!withoutToken) {
    await addToken(defaultOptions);
  }
  const response = await fetch(url, defaultOptions);
  return getResult(response);
};

export { requestSettings, get, post };
