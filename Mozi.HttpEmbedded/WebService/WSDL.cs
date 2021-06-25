using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Mozi.HttpEmbedded.WebService
{
    /// <summary>
    /// WSDL描述文档
    /// </summary>
    public class WSDL
    {

        public string NS_soapenc = "http://schemas.xmlsoap.org/soap/encoding/";
        public string NS_mime = "http://schemas.xmlsoap.org/wsdl/mime/";
        public string NS_soap = "http://schemas.xmlsoap.org/wsdl/soap/";
        public string NS_soap12 = "http://schemas.xmlsoap.org/wsdl/soap12/";
        public string NS_http = "http://schemas.xmlsoap.org/wsdl/http/";

        public string Prefix = "wsdl";
        public string WSDLNamespace = "http://schemas.xmlsoap.org/wsdl/";
        public string Namespace = "http://mozi.org";

        public string PrefixElement = "xs";
        public string PrefixElementNamespace = "http://www.w3.org/2001/XMLSchema";

        public Document Documentation { get; set; }
        
        public Types ApiTypes { get; set; }
        public WSDL()
        {
            Documentation = new Document();
            ApiTypes = new Types();
        }

        public static string CreateDocument(WSDL document)
        {
            var text = "";
            MemoryStream ms = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(ms, System.Text.Encoding.UTF8);

            //declaration 
            writer.WriteStartDocument(true);
            //definitions
            writer.WriteStartElement(document.Prefix, "definitions", document.WSDLNamespace);
            writer.WriteAttributeString("xmlns", "soapenc", null,document.NS_soapenc);
            writer.WriteAttributeString("xmlns", "mime", null, document.NS_mime);
            writer.WriteAttributeString("xmlns", "soap", null, document.NS_soap);
            writer.WriteAttributeString("xmlns", document.PrefixElement, null, document.PrefixElementNamespace);
            writer.WriteAttributeString("xmlns", "soap12", null, document.NS_soap12);
            writer.WriteAttributeString("xmlns", "http", null, document.NS_http);
            writer.WriteAttributeString("targetNamespace", document.Namespace);
            writer.WriteAttributeString("xmlns", "tns", null, document.Namespace);


            //documentation
            writer.WriteStartElement(document.Prefix, "documentation", document.Documentation.Namespace);
            writer.WriteString(document.Documentation.Name);
            writer.WriteEndElement();
            //types
            writer.WriteStartElement(document.Prefix, "types",null);
            writer.WriteStartElement(document.PrefixElement, "schema",null);
            writer.WriteAttributeString("elementFormDefault", "qualified");
            writer.WriteAttributeString("targetNamespace", document.Namespace);
            //elements

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();
            text = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return text;

        }
        public class Document
        {
            public string Name { get; set; }
            public string Namespace = "http://schemas.xmlsoap.org/wsdl/";

            public Document()
            {
                Name = "Mozi.WebService服务信息";
            }
        }
        public class Types
        {
            public List<MethodInfo> Methods = new List<MethodInfo>();

            private class Element
            {
                public string Name = "";
            }

            public class ElementSequence
            {
                public string minOccurs = "";
                public string maxOccurs = "";
                public string name = "";
                public string type = "";
            }
        }

        private class Message
        {
            public class Part
            {
                public string Name { get; set; }
                public string Element { get; set; }
            }
        }

        private class PortType
        {

        }

        private class ServiceType
        {

        }

        private class Binding
        {

        }

        private class Service
        {

        }
    }
}
