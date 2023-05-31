import qs from '../lib/qs/shim.js';

const get = async (url,data)=>
{
  url = `${url}?${qs.stringify(data)}`;
  return await (await fetch(url,{
    method:'GET',
    headers:{
      'Content-type': 'application/json',
      'Authorization':localStorage.getItem("token")
    }
  })).tojson();
}

const post = async (url,data)=>
{
  const init = {
    method:'POST',
    body:JSON.stringify(data),
    headers:{
      'Content-type': 'application/json',
      'Authorization':localStorage.getItem("access_token")
    }
  };
  const response =await fetch(url,init);
  if(response.status===400)
  {
    console.log('输入错误');
  }
  else if(response.status===401)
  {
    alert('未登录');
  }
  else if(response.status==403)
  {
    alert('权限不足');
  }
  else if(response.status===500)
  {
    alert('服务端异常');
  }
  return await response.json();
}

export {get,post}
