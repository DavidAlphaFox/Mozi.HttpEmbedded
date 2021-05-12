using System;
using System.Collections.Generic;
using Mozi.HttpEmbedded.Encode;

namespace Mozi.HttpEmbedded.Cookie
{
    /// <summary>
    /// Cookie
    /// </summary>
    /// <remarks>
    /// name/domain/path 3要素为Cookie的约束条件
    /// </remarks>
    public class HttpCookie
    {
         public string Name           { get; set; }
         public string Value          { get; set; }
         [Obsolete("HTTP/1.1不再使用此值")]
         public string Expires        { get; set; }
         public int    MaxAge         { get; set; }
         public string Path           { get; set; }
         public string Domain         { get; set; }
         public SameSiteMode SameSite { get; set; }
         public bool   Secure         { get; set; }
         public bool   HttpOnly       { get; set; }
         //Secure和HttpOnly

        public HttpCookie()
        {
            
        }

        public HttpCookie(string name,string value)
        {
            Name = name;
            Value = value;
        }
        /// <summary>
        /// 转为Cookie格式字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
           List<string> data = new List<string> {string.Format("{0}={1}", Name, Value)};
           if (MaxAge != 0)
           {
               data.Add(string.Format("max-age={0}",MaxAge)); 
           }
           data.Add(string.Format("path={0}",Path??"/"));
           data.Add(string.Format("domain={0}",Domain??""));
           if (SameSite != SameSiteMode.None)
           {
               data.Add(string.Format("SameSite={0}",Enum.GetName(typeof(SameSiteMode),SameSite)));
           }
           if (Secure)
           {
               data.Add("Secure");
           }
           if (HttpOnly)
           {
               data.Add("HttpOnly");
           }
           return string.Join(new string(new []{(char)ASCIICode.SPACE,(char)ASCIICode.SEMICOLON}), data);
        }
    }
    /// <summary>
    /// HttpCookie扩展类
    /// </summary>
    public static class HttpCookieExtension
    {
        /// <summary>
        /// 判断Cookie是否为同一属性 name/domain/path
        /// </summary>
        /// <param name="hc"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool Equals(this HttpCookie hc, HttpCookie target)
        {
            if (hc != null && target != null)
            {
                hc.Path = hc.Path ?? "/";
                target.Path = hc.Path ?? "/";
                hc.Domain = hc.Domain ?? "";
                target.Domain = target.Domain ?? "";
                return hc.Name.Equals(target.Name) && hc.Path.Equals(target.Name) && hc.Domain.Equals(target.Domain);
            }
            return false;
        }
    }
    public enum SameSiteMode
    {
        //
        // 摘要:
        //     No SameSite field will be set, the client should follow its default cookie policy.
        Unspecified = -1,
        //
        // 摘要:
        //     Indicates the client should disable same-site restrictions.
        None = 0,
        //
        // 摘要:
        //     Indicates the client should send the cookie with "same-site" requests, and with
        //     "cross-site" top-level navigations.
        Lax = 1,
        //
        // 摘要:
        //     Indicates the client should only send the cookie with "same-site" requests.
        Strict = 2
    }
    /// <summary>
    /// 请求Cookie
    /// </summary>
    public class RequestCookie
    {
        private Dictionary<string, string> _data = new Dictionary<string, string>();

        private void Append(string name, string value)
        {
            if (_data.ContainsKey(name))
            {
                _data[name] = value;
            }
            else
            {
                _data.Add(name,value);
            }
        }
        /// <summary>
        /// 解析请求Cookie
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static RequestCookie Parse(string data)
        {
            RequestCookie hc = new RequestCookie();
            string[] kps = data.Split(new string[] {"; "}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var kp in kps)
            {
                var startIndex = kp.IndexOf((char)ASCIICode.EQUAL);
                hc.Append(kp.Substring(0,startIndex),kp.Substring(startIndex+1));
            }
            return hc;
        }
        /// <summary>
        /// 取出缓冲区数据
        /// </summary>
        /// <returns></returns>
        public List<byte> GetBuffer()
        {
            List<byte> data = new List<byte>();
            foreach (var cookie in _data)
            {
                data.AddRange(StringEncoder.Encode(string.Format("{0}: {1}={2}", HeaderProperty.Cookie.PropertyName, cookie.Key,cookie.Value)));
                data.AddRange(TransformHeader.Carriage);
            }
            return data;
        }
        public string Get(string name)
        {
            return _data[name];
        }

        public string this[string ind]
        {
            get { return _data[ind]; }
        }
    }
    /// <summary>
    /// 响应Cookie
    /// </summary>
    public class ResponseCookie
    {
        private readonly List<HttpCookie> _cookies = new List<HttpCookie>();
        /// <summary>
        /// 设置Cookie
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpCookie Set(string key, string value)
        {
            HttpCookie cookie = new HttpCookie() { Name = key, Value = UrlEncoder.Encode(value) };
            if (_cookies.Exists(x => x.Equals(cookie)))
            {
                cookie = _cookies.Find(x => x.Equals(cookie));
                cookie.Value = value;
            }
            else
            {
                _cookies.Add(cookie);
            }
            return cookie;
        }
        /// <summary>
        /// 删除Cookie
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public HttpCookie Delete(string key)
        {
            HttpCookie cookie = Set(key, "");
            cookie.MaxAge = -1000;
            return cookie;
        }
        /// <summary>
        /// 取出缓冲区数据
        /// </summary>
        /// <returns></returns>
        public List<byte> GetBuffer()
        {
            List<byte> data=new List<byte>();
            foreach (var cookie in _cookies)
            {
                data.AddRange(StringEncoder.Encode(string.Format("{0}: {1}",HeaderProperty.SetCookie.PropertyName, cookie.ToString())));
                data.AddRange(TransformHeader.Carriage);
            }
            return data;
        }
    }
}