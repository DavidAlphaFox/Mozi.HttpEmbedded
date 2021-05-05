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
    /// HTTP请求
    /// </summary>
    public class HttpRequest
    {
        public ProtocolType Protocol { get; protected set; }
        /// <summary>
        /// 协议版本
        /// </summary>
        public HttpVersion ProtocolVersion { get; protected set; }
        /// <summary>
        /// 请求路径
        /// </summary>
        public string Path { get; protected set; }
        /// <summary>
        /// 查询字符串
        /// </summary>
        public string QueryString { get; protected set; }
        /// <summary>
        /// 查询 索引可忽略大小写
        /// </summary>
        public Dictionary<string, string> Query = new Dictionary<string, string>(new StringCompareIgnoreCase());
        /// <summary>
        /// POST 索引可忽略大小写
        /// </summary>
        public Dictionary<string, string> Post = new Dictionary<string, string>(new StringCompareIgnoreCase()); 

        /// <summary>
        /// 可接受压缩算法
        /// </summary>
        public string AcceptEncoding { get; protected set; }
        /// <summary>
        /// 请求地址
        /// </summary>
        public string Host { get; protected set; }
        /// <summary>
        /// 客户端信息
        /// </summary>
        public string UserAgent { get; protected set; }
        /// <summary>
        /// 请求方法
        /// </summary>
        public RequestMethod Method { get; protected set; }
        /// <summary>
        /// 内容类型
        /// </summary>
        public string ContentType { get; protected set; }        
        /// <summary>
        /// 内容大小
        /// </summary>
        public string ContentLength { get; protected set; }
        /// <summary>
        /// 请求头
        /// </summary>
        public TransformHeader Headers { get; protected set; }
        /// <summary>
        /// 通讯数据
        /// </summary>
        public byte[] PackedData { get; protected set; }
        /// <summary>
        /// 原始请求首行数据
        /// </summary>
        public byte[] FirstLineData { get; protected set; }
        /// <summary>
        /// 原始请求首行字符串
        /// </summary>
        public string FirstLineString { get; protected set; }
        /// <summary>
        /// 请求头数据
        /// </summary>
        public byte[] HeaderData { get; protected set; }
        /// <summary>
        /// 请求数据体
        /// </summary>
        public byte[] Body { get; protected set; }
        /// <summary>
        /// 文件
        /// </summary>
        public FileCollection Files { get; protected set; }

        /// <summary>
        /// Cookie
        /// </summary>
        public RequestCookie Cookies { get; protected set; }

        public HttpRequest()
        {
            ProtocolVersion = HttpVersion.Version11;
            Headers=new TransformHeader();
            Files=new FileCollection();
            Cookies = new RequestCookie();
            Body = new byte[] { };
        }
        /// <summary>
        /// 解析请求
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
            HttpRequest req = new HttpRequest() {PackedData = data};
            //解析头 判断头部压缩
            int posCR = 0;
            int posCaret = 0;
            int count = 0;

            int index=0;
            int dataLength = data.Length;
            while ((posCR< dataLength) &&Array.IndexOf(data,ASCIICode.CR,posCR+1)>0)
            {
                posCR = Array.IndexOf(data, ASCIICode.CR, posCR + 1);

                //连续两个CR
                byte[] fragement=new byte[posCR-posCaret];
                Array.Copy(data,posCaret,fragement,0,posCR-posCaret);
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
                //跳过分割字节段
                posCaret = posCR+2;
                index++;
            }
            //解析Cookie
            ParseCookie(ref req);
            //解析数据
            if (data.Length > posCR + 4)
            {
                req.Body=new byte[data.Length-(posCR+4)];
                Array.Copy(data, posCR + 4,req.Body,0,req.Body.Length);
                ParsePayload(ref req,req.Body);
            }
            return req;
        }

        /// <summary>
        /// 解析请求体
        /// 区分Content-Type
        /// //TODO 先判断包体是否经过压缩
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void ParsePayload(ref HttpRequest req,byte[] data)
        {
            string formType = req.Headers.GetValue(HeaderProperty.ContentType.PropertyTag);
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
                    ParsePayloadText(ref  req,data);
                }
            }
        }
        /// <summary>
        /// 分析请求体 application/x-www-form-urlencoded
        /// 请求体数据类似于查询字符串
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void ParsePayloadFormUrl(ref HttpRequest req, byte[] data)
        {
             req.Post=UrlEncoder.ParseQuery(StringEncoder.Decode(data));
        }
        /// <summary>
        /// 分析请求体 multipart/form-data
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void ParsePayloadFormData(ref HttpRequest req, byte[] data)
        {
            string contentType =req.Headers.GetValue(HeaderProperty.ContentType.PropertyTag);
            string boundary = "";
            string[] values=contentType.Split(new[]{(char)ASCIICode.SEMICOLON}, StringSplitOptions.RemoveEmptyEntries);

            //取得分割符号boundary

            foreach (var s in values)
            {
                if (s.Trim().StartsWith("boundary"))
                {
                    boundary = s.Trim().Replace("boundary=", "");
                    break;
                }
            }

            byte[] bBound = StringEncoder.Encode(boundary);

            //分割 form-data
            int indBoundary=-1, indEnd=-1,indCR=-1,indFragFirst=0,indFragNext=0;

            while (Array.IndexOf(data, ASCIICode.MINUS, indBoundary+1) >= 0)
            {
                try
                {
                    indFragFirst = indFragNext;
                    indBoundary = Array.IndexOf(data, ASCIICode.MINUS, indBoundary + 1);

                    //循环检测分割段
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
                        //跳过分割
                        indBoundary += bBound.Length+2+2;
                    }
                    //分割信息段
                    if (isFragStart && indFragNext > 0)
                    {
                        //Console.WriteLine("发现片段{0}-{1},长度{2}Byte", indFragFirst, indFragNext, indFragNext - indFragFirst);
                        int posCR = 0;
                        int posCaret = 0;
                        int index = 0;

                        byte[] fragbody=new byte[indFragNext-indFragFirst];

                        Array.Copy(data, indFragFirst,fragbody,0,indFragNext-indFragFirst);

                        File file=new File();
                        bool isFile = false;

                        string fieldName = string.Empty;
                        string fileName = string.Empty;

                        while (Array.IndexOf(fragbody, ASCIICode.CR, posCR + 1) > 0)
                        {
                            posCR = Array.IndexOf(fragbody, ASCIICode.CR, posCR + 1);
                            //连续两个CR
                            if (posCR > 0 && Array.IndexOf(fragbody, ASCIICode.CR, posCR + 1) != posCR + 2)
                            {
                                byte[] fragement = new byte[posCR - posCaret];
                                Array.Copy(fragbody, posCaret, fragement, 0, posCR - posCaret);
                                //1段为名称信息
                                if (index == 1)
                                {
                                    //内容描述信息
                                    //Content-Disposition: form-data; name="fieldNameHere"; filename="fieldName.ext"
                                    string disposition = StringEncoder.Decode(fragement);

                                    string[] headers = disposition.Split(new[] {(char) ASCIICode.SEMICOLON},StringSplitOptions.RemoveEmptyEntries);
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
                            //跳过分割字节段
                            posCaret = posCR + 2;
                            index++;
                        }
                        //解析数据
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
                    Console.WriteLine("Exception parse form-data:{0}",ex.Message);
                }
            }
        }
        //TODO 后续扩展支撑，暂时不实现
        /// <summary>
        /// 分析请求体 文本类型
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
        /// 解析头属性
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void ParseHeaders(ref HttpRequest req, byte[] data)
        {
            HeaderProperty hp = HeaderProperty.Parse(data);
            req.Headers.Add(hp.PropertyTag, hp.PropertyValue);
            #if DEBUG
                Console.WriteLine("{0}:{1}",hp.PropertyTag,hp.PropertyValue);
            #endif
        }

        /// <summary>
        /// 解析Cookie
        /// </summary>
        /// <param name="req"></param>
        private static void ParseCookie(ref HttpRequest req)
        {
            if (req.Headers.Contains(HeaderProperty.Cookie.PropertyTag))
            {
                req.Cookies = RequestCookie.Parse(req.Headers.GetValue(HeaderProperty.Cookie.PropertyTag));
            }
        }
        /// <summary>
        /// 解析首行数据
        /// 方法 查询 协议
        /// </summary>
        /// <param name="req"></param>
        /// <param name="data"></param>
        private static void ParseFirstLine(ref HttpRequest req, byte[] data)
        {
            //解析起始行
            req.FirstLineData = data;
            req.FirstLineString = Encoding.UTF8.GetString(data);
            string[] sFirst = req.FirstLineString.Split(new[] { (char)ASCIICode.SPACE }, StringSplitOptions.None);
            //方法 查询 协议 
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
        //TODO 此功能需要重试
        /// <summary>
        /// 将数据重播
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer()
        {
            List<byte> data = new List<byte>();
            //注入状态信息
            data.AddRange(GetStatusLine());
            data.AddRange(TransformHeader.Carriage);
            //注入默认头部
            data.AddRange(Headers.GetBuffer());
            //注入Cookie
            data.AddRange(Cookies.GetBuffer());
            //注入分割符
            data.AddRange(TransformHeader.Carriage);
            //注入响应包体
            data.AddRange(Body);
            return data.ToArray();
        }
        /// <summary>
        /// 响应状态
        /// </summary>
        /// <returns></returns>
        public byte[] GetStatusLine()
        {
            return StringEncoder.Encode(string.Format("{0} {1} HTTP/{2}", Method.Name,Path, ProtocolVersion.Version));
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