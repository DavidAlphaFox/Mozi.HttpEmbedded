using System;
using System.Net;
using System.Threading;
using Mozi.HttpEmbedded;

namespace Mozi.SSDP
{
    /// <summary>
    /// SSDP协议实现
    /// </summary>
    public class Service
    {
        private RequestMethod MSEARCH = new RequestMethod("M-SEARCH");
        private RequestMethod NOTIFY = new RequestMethod("NOTIFY");
        private const string QueryPath = "*";

        private UDPSocket _socket;
        private Timer _timer;
        private IPEndPoint _remoteEP;

        /// <summary>
        /// 广播消息周期
        /// </summary>
        public int NotificationPeriod = 30 * 1000;
        /// <summary>
        /// 查询周期
        /// </summary>
        public int DiscoverPeriod = 30 * 1000;
        /// <summary>
        /// 缓存时间
        /// </summary>
        public int CacheTimeout = 3600;
        /// <summary>
        /// 默认查询包
        /// </summary>
        public DiscoverPackage PackDefaultDiscover = new DiscoverPackage() 
        {
            MX=3,
            ST= "upnp:rootdevice"
        };
        /// <summary>
        /// 默认在线消息包
        /// </summary>
        public NotificationPackage PackDefaultAlive = new NotificationPackage()
        {
            CacheTimeout = 3600,
            Location = "",
            Server = "",
            NT="upnp:rootdevice",
            USN=""
        };
        /// <summary>
        /// 默认离线消息包
        /// </summary>
        public ByebyePackage PackDefaultByebye = new ByebyePackage() 
        { 
        
        };

        public Service()
        {
            _socket = new UDPSocket();
            _socket.AfterReceiveEnd += _socket_AfterReceiveEnd;
            _remoteEP = new IPEndPoint(IPAddress.Parse(SSDPProtocol.MulticastAddress), SSDPProtocol.ProtocolPort);
            _timer = new Timer(TimeoutCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void _socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            Console.WriteLine("*********收到数据*********,{0}\n{1}\n*******END********", args.IP,System.Text.Encoding.UTF8.GetString(args.Data));
        }

        /// <summary>
        /// 激活
        /// </summary>
        /// <returns></returns>
        public Service Active()
        {
            _socket.StartServer(SSDPProtocol.ProtocolPort);
            _timer.Change(0, NotificationPeriod);
            return this;
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        public Service Showdown()
        {
            _socket.StopServer();
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            return this;
        }
        //M-SEARCH* HTTP/1.1
        //S: uuid:ijklmnop-7dec-11d0-a765-00a0c91e6bf6
        //Host: 239.255.255.250:1900
        //MAN: "ssdp:discover"
        //ST: ge:fridge
        //MX: 3

        //各HTTP协议头的含义：

        //HOST：设置为协议保留多播地址和端口，必须是：239.255.255.250:1900（IPv4）或FF0x::C(IPv6)
        //MAN：设置协议查询的类型，必须是：ssdp:discover
        //MX：设置设备响应最长等待时间。设备响应在0和这个值之间随机选择响应延迟的值，这样可以为控制点响应平衡网络负载。
        //ST：设置服务查询的目标，它必须是下面的类型：
        //-ssdp:all 搜索所有设备和服务
        //-upnp:rootdevice 仅搜索网络中的根设备
        //-uuid:device-UUID 查询UUID标识的设备
        //-urn:schemas-upnp-org:device:device-Type:version 查询device-Type字段指定的设备类型，设备类型和版本由UPNP组织定义。
        //-urn:schemas-upnp-org:service:service-Type:version 查询service-Type字段指定的服务类型，服务类型和版本由UPNP组织定义。
        
        /// <summary>
        /// 发送查询消息
        /// </summary>
        public void Discover(DiscoverPackage dp)
        {
            HttpRequestU request = new HttpRequestU();
            request.SetPath("*").SetMethod(MSEARCH);
            request.SetHeaders(dp.GetHeaders());

            byte[] data = request.GetBuffer();
            
            _socket.SocketMain.SendTo(data, _remoteEP);
        }

        //NOTIFY* HTTP/1.1     
        //HOST: 239.255.255.250:1900    
        //CACHE-CONTROL: max-age = seconds until advertisement expires    
        //LOCATION: URL for UPnP description for root device
        //NT: search target
        //NTS: ssdp:alive 
        //SERVER: OS/versionUPnP/1.0product/version 
        //USN: advertisement UUI

        /// <summary>
        /// 发送存在通知
        /// </summary>
        public void Notification(NotificationPackage np)
        {
            HttpRequestU request = new HttpRequestU();
            request.SetPath("*").SetMethod(NOTIFY);
            request.SetHeaders(np.GetHeaders());
            byte[] data = request.GetBuffer();
            _socket.SocketMain.SendTo(data, _remoteEP);
        }

        //NOTIFY* HTTP/1.1     
        //HOST:    239.255.255.250:1900
        //NT: search target
        //NTS: ssdp:byebye
        //USN: uuid:advertisement UUID

        /// <summary>
        /// 离线通知
        /// </summary>
        public void Leave(ByebyePackage bp)
        {
            HttpRequestU request = new HttpRequestU();
            request.SetPath("*").SetMethod(NOTIFY);
            request.SetHeaders(bp.GetHeaders());
            byte[] data = request.GetBuffer();
            _socket.SocketMain.SendTo(data, _remoteEP);
        }

        //CACHE-CONTROL: max-age = seconds until advertisement expires
        //DATE: when reponse was generated
        //EXT:
        //LOCATION: URL for UPnP description for root device
        //SERVER: OS/Version UPNP/1.0 product/version
        //ST: search target
        //USN: advertisement UUID

        public void EchoDiscover()
        {
            HttpResponse resp = new HttpResponse();
            resp.AddHeader(HeaderProperty.Host, $"{SSDPProtocol.MulticastAddress}:{SSDPProtocol.ProtocolPort}");
            resp.AddHeader(HeaderProperty.CacheControl, $"max-age={CacheTimeout}");
            resp.AddHeader("EXT", "");
            resp.AddHeader(HeaderProperty.Location, "http://127.0.0.1/desc.xml");
            resp.AddHeader("Server", "");
            resp.AddHeader("ST", "mozi-embedded:simplehost");
            resp.AddHeader("USN", "mozi-embedded:simplehost");
            resp.SetStatus(StatusCode.Success);
            byte[] data = resp.GetBuffer();
            _socket.SocketMain.SendTo(data, _remoteEP);
        }

        public void TimeoutCallback(object state)
        {
            Discover(PackDefaultDiscover);
            Notification(PackDefaultAlive);
        }
    }

    public class NotificationPackage:ByebyePackage
    {
        public int CacheTimeout { get; set; }
        public string Location { get; set; }
        public string Server { get; set; }

        public new TransformHeader GetHeaders()
        {
            TransformHeader headers = new TransformHeader();
            headers.Add(HeaderProperty.Host, $"{ MulticastAddress }:{ ProtocolPort }");
            headers.Add("SERVER", $"{ Server }");
            headers.Add("NT", $"{ NT }");
            headers.Add("NTS", SSDPType.Alive.ToString());
            headers.Add("USN", $"{ USN }");
            headers.Add("LOCATION", $"{ Location }");
            headers.Add(HeaderProperty.CacheControl, $"max-age= { CacheTimeout }");
            return headers;
        }
    }
    /// <summary>
    /// 查询头信息
    /// </summary>
    public class DiscoverPackage:AdvertisePackage
    {
        public string S { get; set; }
        //-ssdp:all 搜索所有设备和服务
        //-upnp:rootdevice 仅搜索网络中的根设备
        //-uuid:device-UUID 查询UUID标识的设备
        //-urn:schemas-upnp-org:device:device-Type:version 查询device-Type字段指定的设备类型，设备类型和版本由UPNP组织定义。
        //-urn:schemas-upnp-org:service:service-Type:version 查询service-Type字段指定的服务类型，服务类型和版本由UPNP组织定义。
        public string ST { get; set; }
        /// <summary>
        /// 查询等待时间
        /// </summary>
        public int MX { get; set; }
        public  TransformHeader GetHeaders()
        {
            TransformHeader headers = new TransformHeader();
            headers.Add(HeaderProperty.Host, $"{MulticastAddress}:{ProtocolPort}");
            headers.Add("S", $"{S}");
            headers.Add("MAN", SSDPType.Discover.ToString());
            headers.Add("ST", $"{ST}");
            headers.Add("MX", $"{MX}");
            return headers;
        }
    }
    /// <summary>
    /// 离线头信息
    /// </summary>
    public class ByebyePackage:AdvertisePackage
    {
        public string NT { get; set; }
        //public string NTS { get; set; }
        public string USN { get; set; }

        public TransformHeader GetHeaders()
        {
            TransformHeader headers = new TransformHeader();
            headers.Add(HeaderProperty.Host, $"{MulticastAddress}:{ProtocolPort}");
            headers.Add("NT", $"{NT}");
            headers.Add("NTS", SSDPType.Byebye.ToString());
            headers.Add("USN", $"{USN}");
            return headers;
        }
    }

    public class AdvertisePackage
    {
        //NOTIFY* HTTP/1.1     
        //HOST: 239.255.255.250:1900    
        //CACHE-CONTROL: max-age = seconds until advertisement expires    
        //LOCATION: URL for UPnP description for root device
        //NT: search target
        //NTS: ssdp:alive 
        //SERVER: OS/versionUPnP/1.0product/version 
        //USN: advertisement UUI
        public string MulticastAddress { get; set; }
        public int ProtocolPort { get; set; }

        public AdvertisePackage()
        {
            MulticastAddress = SSDPProtocol.MulticastAddress;
            ProtocolPort = SSDPProtocol.ProtocolPort;
        }
    }

    public class SSDPProtocol
    {
        //组播地址
        public const string MulticastAddress = "239.255.255.250";
        //组播地址IPv6
        public const string MulticastAddressIPv6 = "FF0x::C";
        //组播端口
        public const int ProtocolPort = 1900;
    }
}
