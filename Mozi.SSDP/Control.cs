using System;
using Mozi.HttpEmbedded.WebService;

namespace Mozi.SSDP
{
    public class ControlActionPackage:AbsAdvertisePackage
    {
        public string ContentType { get; set; }
        public DateTime Date { get; set; }
        public int ContentLength {get;set;}
        public string UserAgent { get; set; }
        //SOAPACTION:"urn:schema-upnp-org:service:serviceType:v#actionName"
        public USNDesc USN { get; set; }
        public string ActionName { get; set; }
        public SoapEnvelope Body { get; set; }
    }
    public class ControlActionResponsePackage : AbsAdvertisePackage
    {
        public string TransferEncoding { get; set; }
        public DateTime Date { get; set; }
        public int ContentLength { get; set; }
        public string Server { get; set; }
    }
    public class ControlQueryPackage
    {

    }

}
