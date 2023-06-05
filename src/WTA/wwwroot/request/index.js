import qs from '../lib/qs/shim.js';
import { isLogin } from '../api/user.js';

const requestSettings = {
  baseURL: '/api',
};

const addToken = async (options) => {
  if (isLogin()) {
    options.headers.Authorization = `Bearer ${localStorage.getItem('access_token')}`;
  }
};

const getUrl = (url) => {
  if (url.indexOf('/') === 0) {
    return url;
  }
  return `${requestSettings.baseURL}/${url}`;
};

const getResult = async (response) => {
  const messages = new Map([
    [200, '操作成功'],
    [201, '已创建'],
    [204, '无返回值'],
    [400, '请求参数错误'],
    [401, '未登录'],
    [403, '权限不足'],
    [500, '服务器异常'],
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

const request = async (url, data, options) => {
  url = getUrl(url);
  if (options && options.method === 'GET') {
    return await get(url, data, options);
  }
  const defaultOptions = {
    method: 'POST',
    body: JSON.stringify(data),
    headers: {
      'Content-Type': 'application/json',
    },
  };
  if (options) {
    Object.assign(defaultOptions, options);
  }
  addToken(defaultOptions);
  const response = await fetch(url, defaultOptions);
  return getResult(response);
};

const get = async (url, data, options) => {
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
  addToken(defaultOptions);
  const response = await fetch(url, defaultOptions);
  return getResult(response);
};

const post = async (url, data, options) => {
  url = getUrl(url);
  const defaultOptions = {
    method: 'POST',
    body: JSON.stringify(data),
    headers: {
      'Content-Type': 'application/json',
    },
  };
  if (options) {
    Object.assign(defaultOptions, options);
  }
  addToken(defaultOptions);
  const response = await fetch(url, defaultOptions);
  return getResult(response);
};

export { requestSettings, request, get, post };