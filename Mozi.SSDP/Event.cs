using Mozi.HttpEmbedded.Generic;

namespace Mozi.SSDP
{
    //NOTIFY* HTTP/1.0
    //HOST: 239.255.255.246:7900 *** note the port number is different than SSDP***
    //CONTENT-TYPE: text/xml; charset="utf-8"
    //USN: Unique Service Name for the publisher
    //SVCID: ServiceID from SCPD
    //NT: upnp:event
    //NTS: upnp:propchange
    //SEQ: monotonically increasing sequence count
    //LVL: event importance
    //BOOTID.UPNP.ORG: number increased each time device sends an initial announce or update message
    //CONTENT-LENGTH: bytes in body 
    //<?xml version="1.0"?>
    //<e:propertyset xmlns:e="urn:schemas-upnp-org:event-1-0">
    //    <e:property> <variableName>new value</variableName></e:property>
    //    <!-- Other variable names and values(if any) go here. -->
    //</e:propertyset>
    public class EventPackage : AbsAdvertisePackage
    {
        public string ContentType { get; set; }
        public USNDesc USN { get; set; }
        public string SVCID { get; set; }
        public TargetDesc NT { get; set; }
        public SSDPType NTS { get; set; }
        public int SEQ { get; set; }
        public int LVL { get; set; }
        public int BootId {get;set;}
        public int ContentLength { get; set; }

        public Property[] PropertySet { get; set; }
    }

    public class Property
    {
        public Variable[] Variables { get; set; }
    }

    public class Variable
    {
        public string VariableName { get; set; }
        public string Value { get; set; }
    }

    public class EventLevel : AbsClassEnum
    {

        public static EventLevel Emergency =new EventLevel("upnp","emergency");
        public static EventLevel Fault=new EventLevel("upnp","fault");
        public static EventLevel Warning = new EventLevel("upnp","warning");
        public static EventLevel Info = new EventLevel("upnp","info");
        public static EventLevel Debug = new EventLevel("upnp","debug");
        public static EventLevel General = new EventLevel("upnp","general");

        private string _levelName = "";
        private string _domain = "";
        public override string ToString()
        {
            return _domain + ":/" + _levelName;
        }
        protected override string Tag
        {
            get { return _domain +":/"+ _levelName; }
        }

        public EventLevel(string domain,string levelName)
        {
            _domain = domain;
            _levelName = levelName;
        }
    }
}
