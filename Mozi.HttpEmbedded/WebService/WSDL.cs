namespace Mozi.HttpEmbedded.WebService
{
    /// <summary>
    /// WSDL描述文档
    /// </summary>
    public class WSDL
    {

        public string NS_tm = "http://microsoft.com/wsdl/mime/textMatching/";
        public string NS_soapenc = "http://schemas.xmlsoap.org/soap/encoding/";
        public string NS_mime = "http://schemas.xmlsoap.org/wsdl/mime/";
        public string NS_tns = "Service/ServiceCommon/User";
        public string NS_soap = "http://schemas.xmlsoap.org/wsdl/soap/";
        public string NS_s = "http://www.w3.org/2001/XMLSchema";
        public string NS_soap12 = "http://schemas.xmlsoap.org/wsdl/soap12/";
        public string NS_http = "http://schemas.xmlsoap.org/wsdl/http/";

        public string Prefix = "wsdl";
        public string WSDLNamespace = "http://schemas.xmlsoap.org/wsdl/";
        public string Namespace = "http://tempurl.org";

        public string Documentation { get; set; }
        

        public static string CreateDocument(WSDL document)
        {
            var text = "";
            return text;
        }
        public class Document
        {
            public string Name { get; set; }
            public string Namespace = "http://schemas.xmlsoap.org/wsdl/";
        }
        private class Types
        {
            public string Schema { get; set; }
            public string Prefix = "xs";
            private class Element
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
