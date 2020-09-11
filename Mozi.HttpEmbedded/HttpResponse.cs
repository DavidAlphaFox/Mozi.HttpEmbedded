using System;
using System.Collections.Generic;
using Mozi.HttpEmbedded.Cookie;
using Mozi.HttpEmbedded.Encode;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// HTTP响应
    /// </summary>
    public class HttpResponse
    {
        private byte[] _body=new byte[0];
        /// <summary>
        /// 协议版本
        /// </summary>
        public HttpVersion ProxyVersion { get; set; }
        /// <summary>
        /// 状态码
        /// </summary>
        public StatusCode Status { get; private set; }

        public int ContengLength = 0;

        public string Server = "";

        /// <summary>
        /// 请求头
        /// </summary>
        public TransformHeader Headers { get; private set; }
        /// <summary>
        /// 压缩类型
        /// </summary>
        public string ContentEncoding { get; set; }
        /// <summary>
        /// 请求数据体
        /// </summary>
        public byte[] Body { get { return _body; } }
        /// <summary>
        /// Cookie
        /// </summary>
        public ResponseCookie Cookies { get; private set; }

        public HttpResponse()
        {
            Headers=new TransformHeader();
            ProxyVersion = HttpVersion.Version11;
            Cookies=new ResponseCookie();
        }
        /// <summary>
        /// 设置协议版本
        /// </summary>
        /// <param name="version"></param>
        public void SetVersion(HttpVersion version)
        {
            ProxyVersion = version;
        }
        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="status"></param>
        public void SetStatus(StatusCode status)
        {
            Status = status;
        }
        /// <summary>
        /// 增加头部信息
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
        public HttpResponse AddHeader(HeaderProperty head, string value)
        {
            Headers.Add(head,value);
            return this;
        }
        /// <summary>
        /// 增加头部信息
        /// </summary>
        /// <param name="item"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpResponse AddHeader(string item, string value)
        {
            Headers.Add(item,value);
            return this;
        }
        /// <summary>
        /// 写入字节数据
        /// </summary>
        /// <returns></returns>
        public HttpResponse Write(byte[] data)
        {
            if (_body == null)
            {
                _body = data;
            }
            else
            {
                Array.Resize(ref _body, _body.Length + data.Length);
                Array.Copy(data,_body,data.Length);
            }
            return this;
        }
        /// <summary>
        /// 写入文本
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public HttpResponse Write(string data)
        {
            Write(StringEncoder.Encode(data));
            return this;
        }
        /// <summary>
        /// 从缓冲区中取出数据
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer()
        {
            List<byte> data=new List<byte>();
            //注入状态信息
            data.AddRange(GetStatusLine()); 
            data.AddRange(TransformHeader.Carriage);
            //注入包体大小 字节长度
            if (_body != null)
            {
                AddHeader(HeaderProperty.ContentLength, _body.Length.ToString());
            }
            //注入响应时间
            AddHeader(HeaderProperty.Date, DateTime.Now.ToUniversalTime().ToString("r"));
            //注入默认头部
            data.AddRange(Headers.GetBuffer());
            //注入Cookie
            data.AddRange(Cookies.GetBuffer());
            //注入分割符
            data.AddRange(TransformHeader.Carriage);
            //注入响应包体
            data.AddRange(_body);
            return data.ToArray();
        }
        /// <summary>
        /// 响应状态
        /// </summary>
        /// <returns></returns>
        public byte[] GetStatusLine()
        {
            return StringEncoder.Encode(string.Format("HTTP/{0} {1} {2}", ProxyVersion.Version, Status.Code, Status.Text));
        }
        /// <summary>
        /// 重定向302
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public StatusCode Redirect(string path)
        {
            Headers.Add(HeaderProperty.Location.PropertyTag, path);
            return StatusCode.Found;
        }

        ~HttpResponse()
        {
            _body = null;
        }
    }
}