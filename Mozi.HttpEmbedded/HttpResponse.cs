using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mozi.HttpEmbedded.Cookie;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.Generic;
using Mozi.HttpEmbedded.Source;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// HTTP��Ӧ
    /// </summary>
    public class HttpResponse
    {
        private byte[] _body = new byte[0];
        private string _contentType = "text/plain";

        private string _charset = "";

        /// <summary>
        /// HTTPЭ��汾
        /// </summary>
        public HttpVersion ProtocolVersion { get; set; }
        /// <summary>
        /// ״̬��
        /// </summary>
        public StatusCode Status { get; private set; }
        /// <summary>
        /// ���ݳ���
        /// </summary>
        public int ContentLength { get { return _body.Length; } }
        /// <summary>
        /// Mime����
        /// </summary>
        public string ContentType { get { return _contentType; } private set { _contentType = value; } }
        /// <summary>
        /// ����ͷ
        /// </summary>
        public TransformHeader Headers { get; private set; }
        /// <summary>
        /// ѹ������
        /// </summary>
        public string ContentEncoding { get; set; }
        //���ݱ��뼯
        public string Charset { get { return _charset; } set { _charset = value; } }
        /// <summary>
        /// �ĵ��Ƿ�ѹ����
        /// </summary>
        public bool ContentEncoded { get; private set; }
        /// <summary>
        /// ����������
        /// </summary>
        public byte[] Body { get { return _body; } private set { _body = value; } }
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
        /// ����Э��汾
        /// </summary>
        /// <param name="version"></param>
        public void SetVersion(HttpVersion version)
        {
            ProtocolVersion = version;
        }
        /// <summary>
        /// ����Cookie
        /// <see cref="ResponseCookie.Set(string, string)"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetCookie(string name,string value)
        {
            Cookies.Set(name, value);
        }
        /// <summary>
        /// ����Cookie
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
        /// ����״̬
        /// </summary>
        /// <param name="status"></param>
        public HttpResponse SetStatus(StatusCode status)
        {
            Status = status;
            return this;
        }
        /// <summary>
        /// �����ĵ�����
        /// <para>
        /// Ĭ��Ϊtext/plain
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
        /// ����ͷ����Ϣ
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
        public HttpResponse AddHeader(HeaderProperty head, string value)
        {
            Headers.Add(head, value);
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
            Headers.Add(item, value);
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
                Array.Copy(data, _body, data.Length);
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
        /// д��ѹ��������
        /// </summary>
        /// <param name="body"></param>
        internal void CompressBody(byte[] body)
        {
            _body = body;
            ContentEncoded = true;
        }
        //TODO �˴���Ҫ����
        //Transfer-Encoding
        /// <summary>
        /// �����ļ�
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
        //TODO 2021/05/10 ���ѹ�����룬��Ҫ�԰������ѹ��
        /// <summary>
        /// �ӻ�������ȡ������
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer()
        {
            List<byte> data = new List<byte>();
            //ע��״̬��Ϣ
            data.AddRange(GetStatusLine());
            data.AddRange(TransformHeader.Carriage);
            //ע������С �ֽڳ���
            AddHeader(HeaderProperty.ContentLength, _body.Length.ToString());
            //ע���ĵ�����
            AddHeader(HeaderProperty.ContentType, _contentType+(!string.IsNullOrEmpty(Charset)?";"+ Charset : ""));
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
            return StringEncoder.Encode(string.Format("HTTP/{0} {1} {2}", ProtocolVersion.Version, Status.Code, Status.Text));
        }
        /// <summary>
        /// �ض���302
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public StatusCode Redirect(string path)
        {
            Headers.Add(HeaderProperty.Location.PropertyName, path);
            return StatusCode.Found;
        }
        /// <summary>
        /// HttpResponse�������
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static HttpResponse Parse(byte[] data)
        {
            HttpResponse resp = new HttpResponse();
            //��Ӧ��
            //ͷ
            //����
            //����ͷ
            int posCR = 0;
            int posCaret = 0;
            int index = 0;
            int dataLength = data.Length;
            while ((posCR < dataLength) && Array.IndexOf(data, ASCIICode.CR, posCR + 1) > 0)
            {
                posCR = Array.IndexOf(data, ASCIICode.CR, posCR + 1);

                //��������CR
                byte[] fragement = new byte[posCR - posCaret];
                Array.Copy(data, posCaret, fragement, 0, posCR - posCaret);
                if (index == 0)
                {
                    ParseRequestLine(ref resp, fragement);
                }
                else
                {
                    ParseHeaders(ref resp, fragement);
                }

                if ((Array.IndexOf(data, ASCIICode.CR, posCR + 1) == posCR + 2))
                {
                    break;
                }
                //�����ָ��ֽڶ�
                posCaret = posCR + 2;
                index++;
                //TODO �ÿն���
            }

            //����Cookie
            //�������� ���ز���
            if (data.Length > posCR + 4)
            {
                resp.Body = new byte[data.Length - (posCR + 4)];
                //TODO �˴�����������һ�����ݶ��󣬵����ڴ�ռ�ù���
                Array.Copy(data, posCR + 4, resp.Body, 0, resp.Body.Length);
            }
            return resp;
        }

        /// <summary>
        /// ������������
        /// Э�� ״̬ ���� 
        /// </summary>
        /// <param name="resp"></param>
        /// <param name="data"></param>
        private static void ParseRequestLine(ref HttpResponse resp, byte[] data)
        {
            //������ʼ��
            var RequestLineString = Encoding.UTF8.GetString(data);
            string[] sFirst = RequestLineString.Split(new[] { (char)ASCIICode.SPACE }, StringSplitOptions.None);
            //Э�� ״̬ ���� 
            string sProtocol = sFirst[0];
            string sStatusCode = sFirst[1];
            string sStatusDescription = sFirst[2];

            string sProtoType = sProtocol.Substring(0, sProtocol.IndexOf((char)ASCIICode.DIVIDE));
            string sProtoVersion = sProtocol.Substring(sProtocol.IndexOf((char)ASCIICode.DIVIDE) + 1);

            resp.Status = AbsClassEnum.Get<StatusCode>(sStatusCode);
            resp.ProtocolVersion = AbsClassEnum.Get<HttpVersion>(sProtoVersion);

        }
        /// <summary>
        /// ����ͷ����
        /// </summary>
        /// <param name="resp"></param>
        /// <param name="data"></param>
        private static void ParseHeaders(ref HttpResponse resp, byte[] data)
        {
            HeaderProperty hp = HeaderProperty.Parse(data);            
            
            #if DEBUG
                Console.WriteLine("{0}:{1}",hp.PropertyTag,hp.PropertyValue);
            #endif
            if (!hp.PropertyName.Equals(HeaderProperty.SetCookie))
            {
                resp.Headers.Add(hp.PropertyName, hp.PropertyValue);
            }
            else
            {
                ParseCookie(ref resp, hp.PropertyValue);
            }
        }
        /// <summary>
        /// ����Cookie
        /// </summary>
        /// <param name="resp"></param>
        /// <param name="setCookieValue"></param>
        private static void ParseCookie(ref HttpResponse resp,string setCookieValue)
        {
            HttpCookie cookie = HttpCookie.Parse(setCookieValue);
            resp.Cookies.Set(cookie.Name, cookie.Domain, cookie.Path, cookie.Value);
        }
        ~HttpResponse()
        {
            _body = null;
        }
    }
}