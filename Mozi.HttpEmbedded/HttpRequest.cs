using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

        public ProtocolType Protocol { get; private set; }
        /// <summary>
        /// Э��汾
        /// </summary>
        public HttpVersion ProtocolVersion { get; private set; }
        /// <summary>
        /// ����·��
        /// </summary>
        public String Path { get; private set; }
        /// <summary>
        /// ��ѯ�ַ���
        /// </summary>
        public String QueryString { get; private set; }
        /// <summary>
        /// ��ѯ �����ɺ��Դ�Сд
        /// </summary>
        public Dictionary<String, String> Query = new Dictionary<string, string>(new StringCompareIgnoreCase());
        /// <summary>
        /// POST �����ɺ��Դ�Сд
        /// </summary>
        public Dictionary<String, String> Post = new Dictionary<string, string>(new StringCompareIgnoreCase()); 

        /// <summary>
        /// �ɽ���ѹ���㷨
        /// </summary>
        public String AcceptEncoding { get; private set; }
        /// <summary>
        /// Դ��ַ
        /// </summary>
        public String Host { get; private set; }
        /// <summary>
        /// �ͻ�����Ϣ
        /// </summary>
        public String UserAgent { get; private set; }
        /// <summary>
        /// ���󷽷�
        /// </summary>
        public RequestMethod Method { get;  private set; }
        /// <summary>
        /// ��������
        /// </summary>
        public String ContentType { get; private set; }        
        /// <summary>
        /// ���ݴ�С
        /// </summary>
        public String ContentLength { get; private set; }
        /// <summary>
        /// ����ͷ
        /// </summary>
        public TransformHeader Headers { get; private set; }
        /// <summary>
        /// ͨѶ����
        /// </summary>
        public byte[] PackedData { get; private set; }
        /// <summary>
        /// ��������
        /// </summary>
        public byte[] FirstLineData { get; private set; }
        /// <summary>
        /// �����ַ���
        /// </summary>
        public String FirstLineString { get; private set; }
        /// <summary>
        /// ����ͷ����
        /// </summary>
        public byte[] HeaderData { get; private set; }
        /// <summary>
        /// ����������
        /// </summary>
        public byte[] Body { get; private set; }
        /// <summary>
        /// �ļ�
        /// </summary>
        public FileCollection Files { get; private set; }

        /// <summary>
        /// Cookie
        /// </summary>
        public RequestCookie Cookies { get; private set; }

        public HttpRequest()
        {
            Headers=new TransformHeader();
            Files=new FileCollection();
            Cookies = new RequestCookie();
        }
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static HttpRequest Parse(byte[] data)
        {
            HttpRequest req = new HttpRequest() {PackedData = data};
            //����ͷ �ж�ͷ��ѹ��
            int posCR = 0;
            int posCaret = 0;
            int count = 0;

            int index=0;
            while (Array.IndexOf(data,ASCIICode.CR,posCR+1)>0)
            {
                posCR = Array.IndexOf(data, ASCIICode.CR, posCR + 1);
                //GET / HTTP/1.1\r\nHost: 127.0.0.1:9000\r\n
                //User-Agent: Mozilla/5.0 (Windows NT 5.2; rv:22.0) Gecko/20100101 Firefox/22.0\r\n
                //Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8\r\n
                //Accept-Language: zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3\r\n
                //Accept-Encoding: gzip, deflate\r\n
                //Authorization: Basic c2Rmc2RmOnNmc2Rm\r\n
                //Connection: keep-alive\r\n
                //Cache-Control: max-age=0

                //GET /amazeui/login.html HTTP/1.1\r\n
                //Host: 100.100.0.105:9000\r\n
                //User-Agent: Mozilla/5.0 (Windows NT 6.3; Win64; x64; rv:79.0) Gecko/20100101 Firefox/79.0\r\n
                //Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8\r\n
                //Accept-Language: zh-CN,zh;q=0.8,zh-TW;q=0.7,zh-HK;q=0.5,en-US;q=0.3,en;q=0.2\r\n
                //Accept-Encoding: gzip, deflate\r\nConnection: keep-alive\r\n
                //Upgrade-Insecure-Requests: 1\r\n
                //Authorization: Basic YWRtaW46YWRtaW4=\r\n\r\n
                //��������CR

                byte[] fragement=new byte[posCR-posCaret];
                Array.Copy(data,posCaret,fragement,0,posCR-posCaret);
                if (index == 0)
                {
                    SplitFirstLine(ref req, fragement);
                }
                else
                {
                    SplitHeaders(ref req, fragement);
                }

                if (Array.IndexOf(data, ASCIICode.CR, posCR + 1) == posCR + 2)
                {
                    break;
                }
                //�����ָ��ֽڶ�
                posCaret = posCR+2;
                index++;
            }
            //����Cookie
            SplitCookie(ref req);
            //��������
            if (data.Length > posCR + 4)
            {
                req.Body=new byte[data.Length-(posCR+4)];
                Array.Copy(data, posCR + 4,req.Body,0,req.Body.Length);
                SplitPayload(ref req,req.Body);
            }
            return req;
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
        /// <summary>
        /// ����������
        /// ����Content-Type
        /// //TODO ���жϰ����Ƿ񾭹�ѹ��
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void SplitPayload(ref HttpRequest req,byte[] data)
        {
            String formType = req.Headers.GetValue(HeaderProperty.ContentType.PropertyTag);
            if (formType != null)
            {
                if (formType.Contains("application/x-www-form-urlencoded"))
                {
                    SplitPayloadFormUrl(ref req, data);
                }
                else if (formType.Contains("multipart/form-data"))
                {
                    SplitPayloadFormData(ref req, data);
                }
                else
                {
                    SplitPayloadText(ref  req,data);
                }
            }
        }
        /// <summary>
        /// ���������� application/x-www-form-urlencoded
        /// ���������������ڲ�ѯ�ַ���
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void SplitPayloadFormUrl(ref HttpRequest req, byte[] data)
        {
             req.Post=UrlEncoder.ParseQuery(StringEncoder.Decode(data));
        }
        /// <summary>
        /// ���������� multipart/form-data
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void SplitPayloadFormData(ref HttpRequest req, byte[] data)
        {
            String contentType=req.Headers.GetValue(HeaderProperty.ContentType.PropertyTag);
            String boundary = "";
            String[] values=contentType.Split(new[]{(char)ASCIICode.SEMICOLON}, StringSplitOptions.RemoveEmptyEntries);

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
            int indBoundary=-1, indEnd=-1,indCR=-1,indFragFirst=0,indFragNext=0;

            while (Array.IndexOf(data, ASCIICode.MINUS, indBoundary+1) >= 0)
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
                        if ((data.Length>=indBoundary+bBound.Length)&&data[indBoundary + i + 2] != bBound[i])
                        {
                            isFragStart = false;
                        }
                    }
                    if (isFragStart)
                    {
                        indFragNext = indBoundary;
                        //�����ָ�
                        indBoundary += bBound.Length+2+2;
                    }
                    //�ָ���Ϣ��
                    if (isFragStart && indFragNext > 0)
                    {
                        //Console.WriteLine("����Ƭ��{0}-{1},����{2}Byte", indFragFirst, indFragNext, indFragNext - indFragFirst);
                        
                        int posCR = 0;
                        int posCaret = 0;
                        int index = 0;

                        byte[] fragbody=new byte[indFragNext-indFragFirst];

                        Array.Copy(data, indFragFirst,fragbody,0,indFragNext-indFragFirst);

                        File file=new File();
                        bool isFile = false;

                        String fieldName = String.Empty;
                        String fileName = String.Empty;

                        while (Array.IndexOf(fragbody, ASCIICode.CR, posCR + 1) > 0)
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
                                    String disposition = StringEncoder.Decode(fragement);
                                    
                                    String[] headers = disposition.Split(new[] {(char) ASCIICode.SEMICOLON},StringSplitOptions.RemoveEmptyEntries);
                                    fieldName = headers[1].Trim().Replace("name=","").Trim((char)ASCIICode.QUOTE);

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
                                file.FileName = fileName;
                                file.FileData = postField;
                                file.FileIndex = req.Files.Length;
                                req.Files.Append(file);
                                FileStream fs =new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\" + file.FileName.ToString(),FileMode.OpenOrCreate);
                                fs.Write(file.FileData, 0, file.FileData.Length);
                                fs.Flush();
                                fs.Dispose();
                            }
                            else
                            {
                                req.Query.Add(fieldName, StringEncoder.Decode(postField));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("{0}",indBoundary);
                }
            }
        }
        //TODO ������չ֧�ţ���ʱ��ʵ��
        /// <summary>
        /// ���������� �ı�����
        /// application/json
        /// text/plain
        /// text/xml
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void SplitPayloadText(ref HttpRequest req, byte[] data)
        {
            
        }
        /// <summary>
        /// ����ͷ����
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void SplitHeaders(ref HttpRequest req, byte[] data)
        {
            HeaderProperty hp = HeaderProperty.Parse(data);
            req.Headers.Add(hp.PropertyTag, hp.PropertyValue);
            #if DEBUG
                Console.WriteLine("{0}:{1}",hp.PropertyTag,hp.PropertyValue);
            #endif
        }

        /// <summary>
        /// ����Cookie
        /// </summary>
        /// <param name="req"></param>
        private static void SplitCookie(ref HttpRequest req)
        {
            if (req.Headers.Contains(HeaderProperty.Cookie.PropertyTag))
            {
                req.Cookies = RequestCookie.Parse(req.Headers.GetValue(HeaderProperty.Cookie.PropertyTag));
            }
        }
        /// <summary>
        /// ������������
        /// ���� ��ѯ Э��
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void SplitFirstLine(ref HttpRequest req, byte[] data)
        {
            //������ʼ��
            req.FirstLineData = data;
            req.FirstLineString = Encoding.UTF8.GetString(data);
            String[] sFirst = req.FirstLineString.Split(new[] { (char)ASCIICode.SPACE }, StringSplitOptions.None);
            //���� ��ѯ Э�� 
            String sMethod = sFirst[0];
            String sUrl = sFirst[1];
            String sProtocol = sFirst[2];
            RequestMethod rm = AbsClassEnum.Get<RequestMethod>(sMethod);
            req.Method = rm;

            String[] urls = sUrl.Split(new[] { (char)ASCIICode.QUESTION }, StringSplitOptions.RemoveEmptyEntries);
            req.Path = urls[0];
            if (urls.Length > 1)
            {
                req.QueryString = urls[1];
                req.Query = UrlEncoder.ParseQuery(urls[1]);
            }

            String sProtoType = sProtocol.Substring(0, sProtocol.IndexOf((char)ASCIICode.DIVIDE));
            String sProtoVersion = sProtocol.Substring(sProtocol.IndexOf((char)ASCIICode.DIVIDE) + 1);
            req.Protocol = AbsClassEnum.Get<ProtocolType>(sProtoType);
            req.ProtocolVersion = AbsClassEnum.Get<HttpVersion>(sProtoVersion);
        }
    }
}