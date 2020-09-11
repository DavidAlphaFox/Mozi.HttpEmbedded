using System;
using System.Collections.Generic;
using Mozi.HttpEmbedded.Cookie;
using Mozi.HttpEmbedded.Encode;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// HTTP��Ӧ
    /// </summary>
    public class HttpResponse
    {
        private byte[] _body=new byte[0];
        /// <summary>
        /// Э��汾
        /// </summary>
        public HttpVersion ProxyVersion { get; set; }
        /// <summary>
        /// ״̬��
        /// </summary>
        public StatusCode Status { get; private set; }

        public int ContengLength = 0;

        public string Server = "";

        /// <summary>
        /// ����ͷ
        /// </summary>
        public TransformHeader Headers { get; private set; }
        /// <summary>
        /// ѹ������
        /// </summary>
        public string ContentEncoding { get; set; }
        /// <summary>
        /// ����������
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
        /// ����Э��汾
        /// </summary>
        /// <param name="version"></param>
        public void SetVersion(HttpVersion version)
        {
            ProxyVersion = version;
        }
        /// <summary>
        /// ����״̬
        /// </summary>
        /// <param name="status"></param>
        public void SetStatus(StatusCode status)
        {
            Status = status;
        }
        /// <summary>
        /// ����ͷ����Ϣ
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
        public HttpResponse AddHeader(HeaderProperty head, string value)
        {
            Headers.Add(head,value);
            return this;
        }
        /// <summary>
        /// ����ͷ����Ϣ
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
        /// д���ֽ�����
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
        /// д���ı�
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public HttpResponse Write(string data)
        {
            Write(StringEncoder.Encode(data));
            return this;
        }
        /// <summary>
        /// �ӻ�������ȡ������
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer()
        {
            List<byte> data=new List<byte>();
            //ע��״̬��Ϣ
            data.AddRange(GetStatusLine()); 
            data.AddRange(TransformHeader.Carriage);
            //ע������С �ֽڳ���
            if (_body != null)
            {
                AddHeader(HeaderProperty.ContentLength, _body.Length.ToString());
            }
            //ע����Ӧʱ��
            AddHeader(HeaderProperty.Date, DateTime.Now.ToUniversalTime().ToString("r"));
            //ע��Ĭ��ͷ��
            data.AddRange(Headers.GetBuffer());
            //ע��Cookie
            data.AddRange(Cookies.GetBuffer());
            //ע��ָ��
            data.AddRange(TransformHeader.Carriage);
            //ע����Ӧ����
            data.AddRange(_body);
            return data.ToArray();
        }
        /// <summary>
        /// ��Ӧ״̬
        /// </summary>
        /// <returns></returns>
        public byte[] GetStatusLine()
        {
            return StringEncoder.Encode(string.Format("HTTP/{0} {1} {2}", ProxyVersion.Version, Status.Code, Status.Text));
        }
        /// <summary>
        /// �ض���302
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