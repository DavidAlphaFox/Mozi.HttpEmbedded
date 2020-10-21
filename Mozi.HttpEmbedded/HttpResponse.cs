using System;
using System.Collections.Generic;
using System.IO;
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
        /// <summary>
        /// 内容长度
        /// </summary>
        public int ContengLength {  get  { return _body.Length; } }
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
        //TODO 此处需要调试
        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public HttpResponse SendFile(string filepath)
        {
            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            MemoryStream ms = new MemoryStream();
            try
            {
                byte[] buffer = new byte[1024];
                int readCount = 0;
                while ((readCount = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, readCount);
                }
                Write(ms.GetBuffer());
            }
            finally
            {
                ms.Close();
                fs.Close();
            }
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
            AddHeader(HeaderProperty.ContentLength, _body.Length.ToString());
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