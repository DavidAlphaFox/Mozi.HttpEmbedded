using System;
using System.Collections.Generic;
using System.IO;
using Mozi.HttpEmbedded.Cookie;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.Source;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// HTTP响应
    /// </summary>
    public class HttpResponse
    {
        private byte[] _body = new byte[0];
        private string _contentType = "text/plain";

        private string _charset = "";

        /// <summary>
        /// HTTP协议版本
        /// </summary>
        public HttpVersion ProtocolVersion { get; set; }
        /// <summary>
        /// 状态码
        /// </summary>
        public StatusCode Status { get; private set; }
        /// <summary>
        /// 内容长度
        /// </summary>
        public int ContentLength { get { return _body.Length; } }
        /// <summary>
        /// Mime类型
        /// </summary>
        public string ContentType { get { return _contentType; } private set { _contentType = value; } }
        /// <summary>
        /// 请求头
        /// </summary>
        public TransformHeader Headers { get; private set; }
        /// <summary>
        /// 压缩类型
        /// </summary>
        public string ContentEncoding { get; set; }
        //内容编码集
        public string Charset { get { return _charset; } set { _charset = value; } }
        /// <summary>
        /// 文档是否被压缩过
        /// </summary>
        public bool ContentEncoded { get; private set; }
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
            Headers = new TransformHeader();
            ProtocolVersion = HttpVersion.Version11;
            Cookies = new ResponseCookie();
        }
        /// <summary>
        /// 设置协议版本
        /// </summary>
        /// <param name="version"></param>
        public void SetVersion(HttpVersion version)
        {
            ProtocolVersion = version;
        }
        /// <summary>
        /// 设置Cookie
        /// <see cref="ResponseCookie.Set(string, string)"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetCookie(string name,string value)
        {
            Cookies.Set(name, value);
        }
        /// <summary>
        /// 设置Cookie
        /// <see cref="ResponseCookie.Set(string, string, string, string)"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="domain"></param>
        /// <param name="path"></param>
        /// <param name="value"></param>
        public void SetCookie(string name,string domain,string path,string value)
        {
            Cookies.Set(name, domain, path, value);
        }
        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="status"></param>
        public HttpResponse SetStatus(StatusCode status)
        {
            Status = status;
            return this;
        }
        /// <summary>
        /// 设置文档类型
        /// <para>
        /// 默认为text/plain
        /// </para>
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public HttpResponse SetContentType(string contentType)
        {
            _contentType = contentType;
            return this;
        }
        /// <summary>
        /// 增加头部信息
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
        public HttpResponse AddHeader(HeaderProperty head, string value)
        {
            Headers.Add(head, value);
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
            Headers.Add(item, value);
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
                Array.Copy(data, _body, data.Length);
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
        /// 写入压缩的数据
        /// </summary>
        /// <param name="body"></param>
        internal void CompressBody(byte[] body)
        {
            _body = body;
            ContentEncoded = true;
        }
        //TODO 此处需要调试
        //Transfer-Encoding
        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public HttpResponse SendFile(string filepath)
        {
            FileInfo fi = new FileInfo(filepath);
            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            SetContentType(Mime.Default);
            //RFC6266
            AddHeader(HeaderProperty.ContentDisposition, string.Format("attachment; filename=\"{0}\"; filename*=utf-8''{1}",fi.Name,UrlEncoder.Encode(fi.Name)));
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
        //TODO 2021/05/10 如果压缩介入，就要对包体进行压缩
        /// <summary>
        /// 从缓冲区中取出数据
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer()
        {
            List<byte> data = new List<byte>();
            //注入状态信息
            data.AddRange(GetStatusLine());
            data.AddRange(TransformHeader.Carriage);
            //注入包体大小 字节长度
            AddHeader(HeaderProperty.ContentLength, _body.Length.ToString());
            //注入文档类型
            AddHeader(HeaderProperty.ContentType, _contentType+(!string.IsNullOrEmpty(Charset)?";"+ Charset : ""));
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
            return StringEncoder.Encode(string.Format("HTTP/{0} {1} {2}", ProtocolVersion.Version, Status.Code, Status.Text));
        }
        /// <summary>
        /// 重定向302
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public StatusCode Redirect(string path)
        {
            Headers.Add(HeaderProperty.Location.PropertyName, path);
            return StatusCode.Found;
        }

        ~HttpResponse()
        {
            _body = null;
        }
    }
}