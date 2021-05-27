using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Mozi.HttpEmbedded.Cookie;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// HTTP����
    /// </summary>
    public class HttpRequest
    {
        public ProtocolType Protocol { get; protected set; }
        /// <summary>
        /// Э��汾
        /// </summary>
        public HttpVersion ProtocolVersion { get; protected set; }
        /// <summary>
        /// ����·��
        /// </summary>
        public string Path { get; protected set; }
        /// <summary>
        /// ��ѯ�ַ���
        /// </summary>
        public string QueryString { get; protected set; }
        /// <summary>
        /// ��ѯ �����ɺ��Դ�Сд
        /// </summary>
        public Dictionary<string, string> Query = new Dictionary<string, string>(new StringCompareIgnoreCase());
        /// <summary>
        /// POST �����ɺ��Դ�Сд
        /// </summary>
        public Dictionary<string, string> Post = new Dictionary<string, string>(new StringCompareIgnoreCase());
        /// <summary>
        /// �ɽ���ѹ���㷨
        /// </summary>
        public string AcceptEncoding { get; protected set; }
        /// <summary>
        /// �����ַ
        /// </summary>
        public string Host { get; protected set; }
        /// <summary>
        /// �ͻ�����Ϣ
        /// </summary>
        public string UserAgent { get; protected set; }
        /// <summary>
        /// ���󷽷�
        /// </summary>
        public RequestMethod Method { get; protected set; }
        /// <summary>
        /// ��������
        /// </summary>
        public string ContentType { get; protected set; }
        /// <summary>
        /// ���ݱ�������
        /// </summary>
        public string ContentCharset { get; protected set; }
        /// <summary>
        /// ���ݴ�С
        /// </summary>
        public string ContentLength { get; protected set; }
        /// <summary>
        /// ����ͷ
        /// </summary>
        public TransformHeader Headers { get; protected set; }
        /// <summary>
        /// ͨѶ����
        /// </summary>
        public byte[] PackedData { get; protected set; }
        /// <summary>
        /// ԭʼ������������
        /// </summary>
        public byte[] FirstLineData { get; protected set; }
        /// <summary>
        /// ԭʼ���������ַ���
        /// </summary>
        public string FirstLineString { get; protected set; }
        /// <summary>
        /// ����ͷ����
        /// </summary>
        public byte[] HeaderData { get; protected set; }
        /// <summary>
        /// ����������
        /// </summary>
        public byte[] Body { get; protected set; }
        /// <summary>
        /// �ļ�
        /// </summary>
        public FileCollection Files { get; protected set; }
        /// <summary>
        /// Cookie
        /// </summary>
        public RequestCookie Cookies { get; protected set; }
        /// <summary>
        /// �ͻ���IP��ַ
        /// </summary>
        public string ClientAddress { get; internal set; }
        /// <summary>
        /// �ͻ����Ƿ���ͨ����֤
        /// </summary>
        public bool IsAuthorized { get; internal set; }
        /// <summary>
        /// �ͻ������ܵ�����ѡ��
        /// </summary>
        public LanguageOrder[] AcceptLanguage { get; private set; }
        /// <summary>
        /// �������Դ��ַ
        /// </summary>
        public string Referer { get; private set; }
        /// <summary>
        /// ���캯��
        /// </summary>
        public HttpRequest()
        {
            ProtocolVersion = HttpVersion.Version11;
            Headers = new TransformHeader();
            Files = new FileCollection();
            Cookies = new RequestCookie();
            Body = new byte[] { };
        }
        /// <summary>
        /// ��������
        /// <code>
        ///                     
        ///  GET / HTTP/1.1\r\nHost: 127.0.0.1:9000\r\n
        ///  User-Agent: Mozilla/5.0 (Windows NT 5.2; rv:22.0) Gecko/20100101 Firefox/22.0\r\n
        ///  Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8\r\n
        ///  Accept-Language: zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3\r\n
        ///  Accept-Encoding: gzip, deflate\r\n
        ///  Authorization: Basic c2Rmc2RmOnNmc2Rm\r\n
        ///  Connection: keep-alive\r\n
        ///  Cache-Control: max-age=0
        ///  
        /// </code>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static HttpRequest Parse(byte[] data)
        {
            HttpRequest req = new HttpRequest() { PackedData = data };
            //����ͷ
            int posCR = 0;
            int posCaret = 0;
            int count = 0;

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
                    ParseFirstLine(ref req, fragement);
                }
                else
                {
                    ParseHeaders(ref req, fragement);
                }

                if ((Array.IndexOf(data, ASCIICode.CR, posCR + 1) == posCR + 2))
                {
                    break;
                }
                //�����ָ��ֽڶ�
                posCaret = posCR + 2;
                index++;
            }

            //ͷ����Ϣ�ֽ�
            //HOST
            ParseHeaderHost(ref req);
            //User-Agent
            ParseHeaderUserAgent(ref req);
            //Accept-Language
            ParseHeaderAcceptLanguage(ref req);
            //Referer
            ParseHeaderReferer(ref req);
            //Content-Type
            ParseHeaderContentType(ref req);
            //����Cookie
            ParseCookie(ref req);
            //TODO �˴��Ƿ���Ҫ�ֱ�GET/POST
            //�������� ���ز���
            if (data.Length > posCR + 4)
            {
                req.Body = new byte[data.Length - (posCR + 4)];
                //TODO �˴�����������һ�����ݶ��󣬵����ڴ�ռ�ù���
                Array.Copy(data, posCR + 4, req.Body, 0, req.Body.Length);
                ParsePayload(ref req, req.Body);
            }
            return req;
        }

        //TODO ���жϰ����Ƿ񾭹�ѹ��
        /// <summary>
        /// ����������
        /// ����Content-Type
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void ParsePayload(ref HttpRequest req, byte[] data)
        {
            string formType = req.Headers.GetValue(HeaderProperty.ContentType.PropertyName);
            if (formType != null)
            {
                if (formType.Contains("application/x-www-form-urlencoded"))
                {
                    ParsePayloadFormUrl(ref req, data);
                }
                else if (formType.Contains("multipart/form-data"))
                {
                    ParsePayloadFormData(ref req, data);
                }
                else
                {
                    ParsePayloadText(ref req, data);
                }
            }
        }
        /// <summary>
        /// ���������� application/x-www-form-urlencoded
        /// ���������������ڲ�ѯ�ַ���
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void ParsePayloadFormUrl(ref HttpRequest req, byte[] data)
        {
            req.Post = UrlEncoder.ParseQuery(StringEncoder.Decode(data));
        }
        //TODO �ļ���Ӧд�뻺����,���������ڴ�ռ�ù���
        //DONE �˴����ܽ���һ���ļ��������޸Ĵ���
        //TODO multipart/form-data Ҳ���ύ�ı����ݣ��˴�δ�ܺܺõĴ���
        /// <summary>
        /// ���������� multipart/form-data
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        /// <example>
        /// <code>
        ///   --{boundary}
        ///   Content-Disposition: form-data; name="{file.fieldname}"; filename="{file.name}"
        ///   Content-Type: application/octet-stream
        ///   
        ///   {file.binary}
        ///   
        ///   {boundary}
        ///   Content-Disposition: form-data; name="{file.fieldname}"; filename="{file.name}"
        ///   Content-Type: application/octet-stream
        ///   {file.binary}
        ///   
        ///   {boundary}--
        /// </code>
        /// </example>
        private static void ParsePayloadFormData(ref HttpRequest req, byte[] data)
        {
            string contentType = req.Headers.GetValue(HeaderProperty.ContentType.PropertyName);
            string boundary = "";
            string[] values = contentType.Split(new[] { ((char)ASCIICode.SEMICOLON).ToString() + ((char)ASCIICode.SPACE).ToString() }, StringSplitOptions.RemoveEmptyEntries);

            //ȡ�÷ָ����boundary
            foreach (var s in values)
            {
                if (s.Trim().StartsWith("boundary"))
                {
                    boundary = s.Trim().Replace("boundary=", "");
                    break;
                }
            }

            byte[] bBound = StringEncoder.Encode(boundary);

            //�ָ� form-data
            int indBoundary = -1, indEnd = -1, indCR = -1, indFragFirst = 0, indFragNext = 0;

            while ((indBoundary + 1) < data.Length && Array.IndexOf(data, ASCIICode.MINUS, indBoundary + 1) >= 0)
            {
                try
                {
                    indFragFirst = indFragNext;
                    indBoundary = Array.IndexOf(data, ASCIICode.MINUS, indBoundary + 1);

                    //ѭ�����ָ��
                    bool isFragStart = true;
                    bool isFragEnd = false;

                    for (int i = 0; i < bBound.Length; i++)
                    {
                        if ((data.Length >= indBoundary + bBound.Length) && data.Length >= (indBoundary + i + 2 - 1) && data[indBoundary + i + 2] != bBound[i])
                        {
                            isFragStart = false;
                        }
                    }
                    if (isFragStart)
                    {
                        indFragNext = indBoundary;
                        //�����ָ�
                        indBoundary += bBound.Length + 2 + 2;
                    }
                    //�ָ���Ϣ��
                    if (isFragStart && indFragNext > 0)
                    {
                        //Console.WriteLine("����Ƭ��{0}-{1},����{2}Byte", indFragFirst, indFragNext, indFragNext - indFragFirst);
                        int posCR = 0;
                        int posCaret = 0;
                        int index = 0;

                        byte[] fragbody = new byte[indFragNext - indFragFirst];

                        Array.Copy(data, indFragFirst, fragbody, 0, indFragNext - indFragFirst);

                        File file = new File();
                        bool isFile = false;

                        string fieldName = string.Empty;
                        string fileName = string.Empty;

                        while ((posCR + 1) < fragbody.Length && Array.IndexOf(fragbody, ASCIICode.CR, posCR + 1) > 0)
                        {
                            posCR = Array.IndexOf(fragbody, ASCIICode.CR, posCR + 1);
                            //��������CR
                            if (posCR > 0 && Array.IndexOf(fragbody, ASCIICode.CR, posCR + 1) != posCR + 2)
                            {
                                byte[] fragement = new byte[posCR - posCaret];
                                Array.Copy(fragbody, posCaret, fragement, 0, posCR - posCaret);
                                //1��Ϊ������Ϣ
                                if (index == 1)
                                {
                                    //����������Ϣ
                                    //Content-Disposition: form-data; name="fieldNameHere"; filename="fieldName.ext"
                                    string disposition = StringEncoder.Decode(fragement);

                                    string[] headers = disposition.Split(new[] { ((char)ASCIICode.SEMICOLON).ToString() + ((char)ASCIICode.SPACE).ToString() }, StringSplitOptions.RemoveEmptyEntries);
                                    fieldName = headers[1].Trim().Replace("name=", "").Trim((char)ASCIICode.QUOTE);

                                    if (headers.Length > 2)
                                    {
                                        fileName = headers[2];
                                        if (fileName.Contains("filename="))
                                        {
                                            isFile = true;
                                            fileName = fileName.Trim().Replace("filename=", "").Trim((char)ASCIICode.QUOTE);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                            //�����ָ��ֽڶ�
                            posCaret = posCR + 2;
                            index++;
                        }
                        //��������
                        if (data.Length > posCR + 4)
                        {
                            var postField = new byte[fragbody.Length - (posCR + 4)];
                            Array.Copy(fragbody, posCR + 4, postField, 0, postField.Length);

                            if (isFile)
                            {
                                file.FileName = HtmlEncoder.EntityCodeToString(fileName);
                                file.FileData = postField;
                                file.FileIndex = req.Files.Length;
                                file.FieldName = fieldName;
                                //file.FileTempSavePath = AppDomain.CurrentDomain.BaseDirectory + @"Temp\" + file.FileName.ToString();
                                req.Files.Append(file);
                                ////д����ʱĿ¼
                                //FileStream fs = new FileStream(file.FileTempSavePath, FileMode.OpenOrCreate);
                                //int length = fragbody.Length - (posCR + 4);
                                //int blockSize = 1024;
                                //int count = length / blockSize;
                                //byte[] blockData = new byte[blockSize];
                                //int mode = length % blockSize;
                                //for (int i = 0; i < count; i++)
                                //{
                                //    Array.Copy(fragbody, posCR + 4 + i * blockData.Length, blockData, 0, blockData.Length);
                                //    fs.Write(blockData, 0, blockData.Length);
                                //}
                                //if (mode > 0)
                                //{
                                //    Array.Resize(ref blockData, mode);
                                //    Array.Copy(fragbody, posCR + 4 + blockSize * count, blockData, 0, blockData.Length);
                                //    fs.Write(blockData, 0, blockData.Length);
                                //}
                                //fs.Flush();
                                //fs.Dispose();
                            }
                            else
                            {
                                //var postField = new byte[fragbody.Length - (posCR + 4)];
                                req.Query.Add(fieldName, StringEncoder.Decode(postField));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception occurs while parsing multipart/form-data:{0}", ex.Message);
                }
            }
        }
        //TODO ������չʵ�֣���ʱ��ʵ��
        /// <summary>
        /// ���������� �ı�����
        ///     application/json
        ///     text/plain
        ///     text/xml
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void ParsePayloadText(ref HttpRequest req, byte[] data)
        {

        }
        /// <summary>
        /// ����ͷ����
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void ParseHeaders(ref HttpRequest req, byte[] data)
        {
            HeaderProperty hp = HeaderProperty.Parse(data);
            req.Headers.Add(hp.PropertyName, hp.PropertyValue);
            #if DEBUG
                Console.WriteLine("{0}:{1}",hp.PropertyTag,hp.PropertyValue);
            #endif
        }
        /// <summary>
        /// ����UserAgent
        /// </summary>
        /// <param name="req"></param>
        private static void ParseHeaderUserAgent(ref HttpRequest req)
        {
            if (req.Headers.Contains(HeaderProperty.UserAgent.PropertyName))
            {
                req.UserAgent = req.Headers.GetValue(HeaderProperty.UserAgent.PropertyName);
            }
        }
        /// <summary>
        /// ��������Ŀ��������ַ
        /// </summary>
        /// <param name="req"></param>
        private static void ParseHeaderHost(ref HttpRequest req)
        {
            if (req.Headers.Contains(HeaderProperty.Host.PropertyName))
            {
                req.Host = req.Headers.GetValue(HeaderProperty.Host.PropertyName);
            }
        }
        /// <summary>
        /// ������Դҳ���ַ
        /// </summary>
        /// <param name="req"></param>
        private static void ParseHeaderReferer(ref HttpRequest req)
        {
            if (req.Headers.Contains(HeaderProperty.Referer.PropertyName))
            {
                req.Referer = req.Headers.GetValue(HeaderProperty.Referer.PropertyName);
            }
        }
        /// <summary>
        /// ����������������
        /// 	�﷨��<code>zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3</code>
        /// 	�ָ����Ϊ,
        /// </summary>
        /// <param name="req"></param>
        private static void ParseHeaderAcceptLanguage(ref HttpRequest req)
        {
            if (req.Headers.Contains(HeaderProperty.AcceptLanguage.PropertyName))
            {
                var language=req.Headers.GetValue(HeaderProperty.AcceptLanguage.PropertyName)??"";
                var languages = language.Split(new char[] { (char)ASCIICode.COMMA },StringSplitOptions.RemoveEmptyEntries);
                req.AcceptLanguage = new LanguageOrder[languages.Length];
                try
                {
                    for (int i = 0; i < languages.Length; i++)
                    {
                        var lan = languages[i];
                        var lans = lan.Split(new char[] { (char)ASCIICode.COMMA }, StringSplitOptions.RemoveEmptyEntries);
                        req.AcceptLanguage[i] = new LanguageOrder()
                        {
                            LanguageName = lans[0],
                            Weight = lans.Length > 1 ? int.Parse(lans[1]) : 1
                        };
                    }
                }
                catch
                {

                }
            }
        }
        /// <summary>
        /// �����ĵ�����
        /// </summary>
        /// <param name="req"></param>
        private static void ParseHeaderContentType(ref HttpRequest req)
        {
            if (req.Headers.Contains(HeaderProperty.ContentType.PropertyName))
            {
                var contentType=req.Headers.GetValue(HeaderProperty.ContentType.PropertyName);
                string[] cts = contentType.Split(new[] { ((char)ASCIICode.SEMICOLON).ToString() + ((char)ASCIICode.SPACE).ToString() }, StringSplitOptions.RemoveEmptyEntries);
                if (cts.Length > 0)
                {
                    req.ContentType = cts[0];
                    if (cts.Length > 1)
                    {
                        req.ContentCharset = cts[1];
                    }
                }
            }
        }
        /// <summary>
        /// ����Cookie
        /// </summary>
        /// <param name="req"></param>
        private static void ParseCookie(ref HttpRequest req)
        {
            if (req.Headers.Contains(HeaderProperty.Cookie.PropertyName))
            {
                req.Cookies = RequestCookie.Parse(req.Headers.GetValue(HeaderProperty.Cookie.PropertyName));
            }
        }

        /// <summary>
        /// ������������
        /// ���� ��ѯ Э��
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void ParseFirstLine(ref HttpRequest req, byte[] data)
        {
            //������ʼ��
            req.FirstLineData = data;
            req.FirstLineString = Encoding.UTF8.GetString(data);
            string[] sFirst = req.FirstLineString.Split(new[] { (char)ASCIICode.SPACE }, StringSplitOptions.None);
            //���� ��ѯ Э�� 
            string sMethod = sFirst[0];
            string sUrl = sFirst[1];
            string sProtocol = sFirst[2];
            RequestMethod rm = AbsClassEnum.Get<RequestMethod>(sMethod);
            req.Method = rm;

            string[] urls = sUrl.Split(new[] { (char)ASCIICode.QUESTION }, StringSplitOptions.RemoveEmptyEntries);
            req.Path = urls[0];
            if (urls.Length > 1)
            {
                req.QueryString = urls[1];
                req.Query = UrlEncoder.ParseQuery(urls[1]);
            }

            string sProtoType = sProtocol.Substring(0, sProtocol.IndexOf((char)ASCIICode.DIVIDE));
            string sProtoVersion = sProtocol.Substring(sProtocol.IndexOf((char)ASCIICode.DIVIDE) + 1);
            req.Protocol = AbsClassEnum.Get<ProtocolType>(sProtoType);
            req.ProtocolVersion = AbsClassEnum.Get<HttpVersion>(sProtoVersion);
        }
        //TODO �˹�����Ҫ�����Խ�����֤
        /// <summary>
        /// �������ز�
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer()
        {
            List<byte> data = new List<byte>();
            //ע��״̬��Ϣ
            data.AddRange(GetFirstLine());
            data.AddRange(TransformHeader.Carriage);
            //ע��Ĭ��ͷ��
            data.AddRange(Headers.GetBuffer());
            //ע��Cookie
            data.AddRange(Cookies.GetBuffer());
            //ע��ָ��
            data.AddRange(TransformHeader.Carriage);
            //ע����Ӧ����
            data.AddRange(Body);
            return data.ToArray();
        }
        /// <summary>
        /// ��Ӧ״̬
        /// </summary>
        /// <returns></returns>
        public byte[] GetFirstLine()
        {
            return StringEncoder.Encode(string.Format("{0} {1} HTTP/{2}", Method.Name, Path, ProtocolVersion.Version));
        }
        ~HttpRequest()
        {
            PackedData = null;
            FirstLineData = null;
            Body = null;
            Headers = null;
            HeaderData = null;
            Files = null;
        }
    }
}