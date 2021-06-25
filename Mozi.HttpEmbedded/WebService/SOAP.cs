using System.Collections.Generic;
using System.IO;
using System.Xml;
using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded.WebService
{
    /// <summary>
    /// SOAP envelope封装
    /// </summary>
    public class SOAPEnvelope
    {
        public string NS_XSI = "http://www.w3.org/2001/XMLSchema-instance";
        public string NS_XSD= "http://www.w3.org/2001/XMLSchema";
        public string EncodingStyle = "http://www.w3.org/2001/12/soap-encoding";

        /// <summary>
        /// SOAP版本
        /// </summary>
        public SOAPVersion Version = SOAPVersion.Ver11;

        public SOAPHeader Header { get; set; }
        public SOAPBody Body { get; set; }

        public SOAPEnvelope()
        {
            Body = new SOAPBody();
        }
        /// <summary>
        /// 构造xml文档
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        public static string CreateDocument(SOAPEnvelope envelope)
        {
            MemoryStream ms = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(ms, System.Text.Encoding.UTF8);
            writer.WriteStartDocument(true);
            writer.WriteStartElement(envelope.Version.Prefix, "Envelope", envelope.Version.Namespace);
            writer.WriteAttributeString(envelope.Version.Prefix, "encodingStyle",null,envelope.EncodingStyle);
            writer.WriteAttributeString("xmlns", "xsi",null,envelope.NS_XSI);
            writer.WriteAttributeString("xmlns", "xsd", null,envelope.NS_XSD);
            //header
            if (envelope.Header != null)
            {
                writer.WriteStartElement(envelope.Version.Prefix, "Header", "");
                if (envelope.Header.Childs != null && envelope.Header.Childs.Length > 0)
                {

                }
                writer.WriteEndElement();
            }

            //body
            writer.WriteStartElement(envelope.Version.Prefix, "Body", null);
            //bodyelements
            writer.WriteStartElement(envelope.Body.Prefix, envelope.Body.Method, envelope.Body.Namespace);
            foreach (var r in envelope.Body.Items)
            {
                writer.WriteElementString(envelope.Body.Prefix, r.Key,null, r.Value);
            }
            //fault
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
            string text = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return text;
        }
        //TODO 这种写法有问题，暂时无法生成完整的XML文档，后期再想办法解决
        /// <summary>
        /// 构造xml文档
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        internal static string CreateDocument1(SOAPEnvelope envelope)
        {
            XmlDocument doc = new XmlDocument();

            var declare = doc.CreateXmlDeclaration("1.0", "utf-8", "yes");
            doc.AppendChild(declare);


            //envelope
            var nodeEnvelope = doc.CreateNode(XmlNodeType.Element, envelope.Version.Prefix,"Envelope", envelope.Version.Namespace);
            var encodingStyle =doc.CreateAttribute("soap","encodingStyle",envelope.EncodingStyle);
            var xsi = doc.CreateAttribute("xmlns", "xsi",null);
            xsi.Value = envelope.NS_XSI;
            var xsd = doc.CreateAttribute("xsd","xsd", null);

            nodeEnvelope.Attributes.Append((XmlAttribute)xsi);
            nodeEnvelope.Attributes.Append((XmlAttribute)xsd);
            nodeEnvelope.Attributes.Append((XmlAttribute)encodingStyle);

            //header
            if (envelope.Header != null)
            {
                var nodeHeader = doc.CreateElement(envelope.Version.Prefix,"Header", "");
                if (envelope.Header.Childs != null && envelope.Header.Childs.Length > 0)
                {

                }
                nodeEnvelope.AppendChild(nodeHeader);
            }
            //body
            var nodeBody = doc.CreateElement(envelope.Version.Prefix,"Body", "");
            //bodyelements
            var nodeBodyMethod = doc.CreateElement(envelope.Body.Prefix,envelope.Body.Method, envelope.Body.Namespace);
            foreach(var r in envelope.Body.Items)
            {
                var nodeItem = doc.CreateElement(envelope.Body.Prefix,r.Key, " ");
                nodeItem.InnerText = r.Value;
                nodeBodyMethod.AppendChild(nodeItem);
            }

            nodeBody.AppendChild(nodeBodyMethod);
            nodeEnvelope.AppendChild(nodeBody);
            doc.AppendChild(nodeEnvelope);
            return doc.OuterXml;
        }
        /// <summary>
        /// 解析SOAP文件
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static XmlDocument ParseDocument(string content)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);
            return doc;
        }
    }
    public class SOAPHeader
    {
        public SOAPHeaderChild[] Childs { get; set; }
    }
    public class SOAPHeaderChild
    {
        public string Name { get; set; }
        public string actor {get;set;}
        public string mustUnderstand { get; set; }  //"0"|"1"
        public string encodingStyle { get; set; }
    }
    public class SOAPBody
    {
        public SOAPFault Fault { get; set; }

        public string Prefix = "m";
        public string Method = "";
        public string Namespace = "Mozi/WebService/Soap";
        public Dictionary<string, string> Items = new Dictionary<string, string>();
    }

    public class SOAPFault
    {
        //VersionMismatch SOAP Envelope 元素的无效命名空间被发现
        //MustUnderstand Header 元素的一个直接子元素（带有设置为 "1" 的 mustUnderstand 属性）无法被理解。
        //Client 消息被不正确地构成，或包含了不正确的信息。
        //Server 服务器有问题，因此无法处理进行下去。
        public string faultcode { get; set; }   
        public string faultstring { get; set; }
        public string faultactor { get; set; }
        public string detail { get; set; }
    }
    /// <summary>
    /// SOAP协议版本
    /// </summary>
    public class SOAPVersion : AbsClassEnum
    {
        /// <summary>
        /// text/xml
        /// </summary>
        public static SOAPVersion Ver11 = new SOAPVersion("1.1","soap", "http://schemas.xmlsoap.org/soap/envelope/");
        /// <summary>
        /// application/soap+xml
        /// </summary>
        public static SOAPVersion Ver12 = new SOAPVersion("1.2","soap12", "http://www.w3.org/2003/05/soap-envelope");
        public static SOAPVersion Ver12Dotnet = new SOAPVersion("1.2", "soap2", "http://www.w3.org/2003/05/soap-envelope");
        public string Version { get { return _vervalue; } }
        public string Namespace { get { return _namespace; } }
        public string Prefix { get { return _prefix; } }
        protected override string Tag { get { return _vervalue; } }

        private string _vervalue = "";
        private string _namespace = "";
        private string _prefix = "";

        public SOAPVersion(string verValue,string prefix,string nameSpace)
        {
            _vervalue = verValue;
            _prefix = prefix;
            _namespace = nameSpace;
        }
    }
}
