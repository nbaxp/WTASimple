import router from '../router/index.js';
import { post } from '../request/index.js';
import jwt_decode from '../lib/jwt-decode/jwt-decode.esm.js';
import qs from '../lib/qs/shim.js';

const isLogin = async () => {
  let isLogin = false;
  const accessToken = localStorage.getItem('access_token');
  if (accessToken != null) {
    var exp = new Date(jwt_decode(accessToken).exp * 1000);
    if (exp > new Date()) {
      isLogin = true;
    }
    else {
      var refreshToken = localStorage.getItem('refresh_token');
      if (refreshToken != null) {
        const refreshRespoonse = await fetch('/token/refresh', {
          method: 'POST',
          body: qs.stringify({ refreshToken }),
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
          }
        });
        if (refreshRespoonse.ok) {
          const refreshResult = await refreshRespoonse.json();
          localStorage.setItem('access_token', refreshResult.access_token);
          localStorage.setItem('refresh_token', refreshResult.refresh_token);
          isLogin = true;
        }
      }
    }
  }
  if (!isLogin) {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
  }
  return isLogin;
};

const login = async (action, data) => {
  const result = await post(action, data);
  if (!result.errors) {
    localStorage.setItem('access_token', result.access_token);
    localStorage.setItem('refresh_token', result.refresh_token);
    //const userInfo = await post('/')
    var redirect = router.currentRoute.value.query?.redirect ?? '/'
    router.push(redirect);
  }
  return result;
}

const logout = () => {
  localStorage.removeItem('access_token');
  localStorage.removeItem('refresh_token');
  router.push({ path: '/login', query: { redirect: router.currentRoute.value.fullPath } });
}

const getUser = async () => {
  return post('/user/info');
};

export { isLogin, login, logout, getUser }
