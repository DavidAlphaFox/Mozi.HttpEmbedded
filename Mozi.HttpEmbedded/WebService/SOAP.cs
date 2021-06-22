using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded.WebService
{
    class SOAPEnvelope
    {
        public string xsi = "http://www.w3.org/2001/XMLSchema-instance";
        public string xsd="";
        public string soap = "";
        public string encodingStyle = "";

        public SOAPHeader Header { get; set; }
        public SOAPBody Body { get; set; }
    }
    class SOAPHeader
    {
        SOAPHeaderChild[] Childs { get; set; }
    }
    class SOAPHeaderChild
    {
        public string actor {get;set;}
        public string mustUnderstand { get; set; }  //"0"|"1"
        public string encodingStyle { get; set; }
    }
    class SOAPBody
    {

        public SOAPFault Fault { get; set; }
    }

    class SOAPFault
    {
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
        public static SOAPVersion Ver11 = new SOAPVersion("1.1", "http://schemas.xmlsoap.org/soap/envelope/");
        /// <summary>
        /// application/soap+xml
        /// </summary>
        public static SOAPVersion Ver12 = new SOAPVersion("1.2", "http://www.w3.org/2003/05/soap-envelope");

        public string Version { get { return _vervalue; } }

        protected override string Tag { get { return _vervalue; } }

        private string _vervalue = "";
        private string _namespace = "";

        public SOAPVersion(string verValue,string nameSpace)
        {
            _vervalue = verValue;
            _namespace = nameSpace;
        }
    }
}
