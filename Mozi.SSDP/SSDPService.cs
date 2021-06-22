using System;
using System.Net;
using System.Threading;
using Mozi.HttpEmbedded;

namespace Mozi.SSDP
{
    public delegate void ServiceFound(object sender);

    public delegate void ServiceMessageReceive(object sender,HttpRequest request,string host);

    //TODO 进一步完善SSDP协议并进行良好的封装

    /// <summary>
    /// SSDP协议实现
    /// <para>
    ///     功能包含：发现，在线广播，离线广播
    /// </para>
    /// <para>
    /// Simple Service Discovery Protocol
    /// </para>
    /// </summary>
    public class SSDPService
    {
        private const string QueryPath = "*";

        private UDPSocket _socket;
        private Timer _timer;
        private IPEndPoint _remoteEP;

        private string _server = " Mozi/1.2.4 UPnP/1.0 Mozi.SSDP/1.2.4";
        private string _uuid = "";

        #region
        public string UUID { get { return _uuid; } set { _uuid = value; } }
        public string Server { get { return _server; } set { _server = value; } }
        public string Location { get; set; }
        #endregion

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

        public string DeviceName = "";


        /// <summary>
        /// 是否接受回环地址消息
        /// <para>
        /// 激活定时服务前启用此参数
        /// </para>
        /// </summary>
        public bool AllowLoopbackMessage { get; set; }

        /// <summary>
        /// 默认查询包
        /// </summary>
        public SearchPackage PackDefaultDiscover = new SearchPackage() 
        {
            MX=3,
            ST= "ssdp:all"
        };
        /// <summary>
        /// 默认在线消息包
        /// </summary>
        public AlivePackage PackDefaultAlive = new AlivePackage()
        {
            CacheTimeout = 3600,
            Location = "",
            Server = "",
            NT= "ssdp:all",
            USN="",
        };
        /// <summary>
        /// 默认离线消息包
        /// </summary>
        public ByebyePackage PackDefaultByebye = new ByebyePackage() 
        { 
            
        };

        public event ServiceFound OnServiceFound;

        public event ServiceMessageReceive OnServiceMessageReceive;

        /// <summary>
        /// 构造函数
        /// <para>
        /// 从实例创建开始就可以开始接受组播数据
        /// </para>
        /// </summary>
        public SSDPService()
        {
            _socket = new UDPSocket();
            _socket.AfterReceiveEnd += _socket_AfterReceiveEnd;
            _remoteEP = new IPEndPoint(IPAddress.Parse(SSDPProtocol.MulticastAddress), SSDPProtocol.ProtocolPort);
            _timer = new Timer(TimeoutCallback, null, Timeout.Infinite, Timeout.Infinite);
        }
        /// <summary>
        /// 数据接收时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void _socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            HttpRequest req = HttpRequestU.Parse(args.Data); 
            Console.WriteLine("*********收到数据[{0}]*********\r\n{1}\r\n*******END********", args.IP,System.Text.Encoding.UTF8.GetString(args.Data));
        }
        /// <summary>
        /// 包解析
        /// </summary>
        private static void ParsePackage()
        {

        }
        /// <summary>
        /// 激活服务
        /// <para>
        /// 启动一个定时器，定时发送在线消息
        /// </para>
        /// </summary>
        /// <returns></returns>
        public SSDPService Activate()
        {            
            _socket.AllowLoopbackMessage = AllowLoopbackMessage;
            _socket.StartServer(SSDPProtocol.ProtocolPort);
            //是否接受回环消息
            _timer.Change(0, NotificationPeriod);
            return this;
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        public SSDPService Inactivate()
        {
            _socket.StopServer();
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            return this;
        }
        //M-SEARCH * HTTP/1.1
        //S: uuid:ijklmnop-7dec-11d0-a765-00a0c91e6bf6
        //Host: 239.255.255.250:1900
        //MAN: "ssdp:discover"
        //ST: ge:fridge
        //      -ssdp:all 搜索所有设备和服务
        //      -upnp:rootdevice 仅搜索网络中的根设备
        //      -uuid:device-UUID 查询UUID标识的设备
        //      -urn:schemas-upnp-org:device:device-Type:version 查询device-Type字段指定的设备类型，设备类型和版本由UPNP组织定义。
        //      -urn:schemas-upnp-org:service:service-Type:version 查询service-Type字段指定的服务类型，服务类型和版本由UPNP组织定义。
        //MX: {seconds}
        //      --{seconds} in [1,120]

        /// <summary>
        /// 发送查询消息
        /// </summary>
        public void Discover(SearchPackage dp)
        {
            HttpRequestU request = new HttpRequestU();
            request.SetPath("*").SetMethod(RequestMethodUPnP.MSEARCH);
            request.SetHeaders(dp.GetHeaders());

            byte[] data = request.GetBuffer();
            
            _socket.SocketMain.SendTo(data, _remoteEP);
        }

        //NOTIFY * HTTP/1.1     
        //HOST: 239.255.255.250:1900    
        //CACHE-CONTROL: max-age = {seconds}   
        //      --{seconds}>=1800s
        //LOCATION: URL for UPnP description for root device
        //NT: search target
        //      --upnp:rootdevice
        //      --uuid:device-UUID
        //      --urn:schemas-upnp-org:device:deviceType:v
        //      --urn:schemas-upnp-org:service:serviceType:v
        //      --urn:domain-name:device:deviceType:v
        //      --urn:domain-name:service:serviceType:v
        //NTS: ssdp:alive 
        //SERVER: OS/version UPnP/1.0 product/version 
        //USN: advertisement UUID
        //      --uuid:device-UUID::upnp:rootdevice 
        //      --uuid:device-UUID
        //      --uuid:device-UUID::urn:schemas-upnp-org:device:deviceType:v
        //      --uuid:device-UUID::urn:schemas-upnp-org:service:serviceType:v
        //      --uuid:device-UUID::urn:domain-name:device:deviceType:v
        //      --uuid:device-UUID::urn:domain-name:service:serviceType:v

        /// <summary>
        /// 发送存在通知
        /// </summary>
        public void NotifyAlive(AlivePackage np)
        {
            HttpRequestU request = new HttpRequestU();
            request.SetPath("*").SetMethod(RequestMethodUPnP.NOTIFY);
            request.SetHeaders(np.GetHeaders());
            byte[] data = request.GetBuffer();
            _socket.SocketMain.SendTo(data, _remoteEP);
        }

        //NOTIFY * HTTP/1.1     
        //HOST:    239.255.255.250:1900
        //NT: search target
        //NTS: ssdp:byebye
        //USN: uuid:advertisement UUID

        /// <summary>
        /// 离线通知
        /// </summary>
        public void NotifyLeave(ByebyePackage bp)
        {
            HttpRequestU request = new HttpRequestU();
            request.SetPath("*").SetMethod(RequestMethodUPnP.NOTIFY);
            request.SetHeaders(bp.GetHeaders());
            byte[] data = request.GetBuffer();
            _socket.SocketMain.SendTo(data, _remoteEP);
        }

        //HTTP/1.1 200 OK
        //CACHE-CONTROL: max-age = seconds until advertisement expires
        //DATE: when reponse was generated
        //EXT:
        //LOCATION: URL for UPnP description for root device
        //SERVER: OS/Version UPNP/1.0 product/version
        //ST: search target
        //USN: advertisement UUID

        /// <summary>
        /// 响应 MS-SEARCH 查找
        /// </summary>
        public void EchoDiscover()
        {
            HttpResponse resp = new HttpResponse();
            resp.AddHeader(HeaderProperty.Host, $"{SSDPProtocol.MulticastAddress}:{SSDPProtocol.ProtocolPort}");
            resp.AddHeader(HeaderProperty.CacheControl, $"max-age = {CacheTimeout}");
            resp.AddHeader("EXT", "");
            resp.AddHeader(HeaderProperty.Location, "http://127.0.0.1/desc.xml");
            resp.AddHeader("Server", _server);
            resp.AddHeader("ST", "mozi-embedded:simplehost");
            resp.AddHeader("USN", "mozi-embedded:simplehost");
            resp.SetStatus(StatusCode.Success);
            byte[] data = resp.GetBuffer();
            _socket.SocketMain.SendTo(data, _remoteEP);
        }
        internal void Subscribe()
        {

        }
        internal void UnSubscribe()
        {

        }
        /// <summary>
        /// 定时器回调函数
        /// </summary>
        /// <param name="state"></param>
        private void TimeoutCallback(object state)
        {
            Discover(PackDefaultDiscover);
            NotifyAlive(PackDefaultAlive);
        }
    }

    public class AlivePackage:ByebyePackage
    {
        public int CacheTimeout { get; set; }
        public string Location { get; set; }
        public string Server { get; set; }

        public new TransformHeader GetHeaders()
        {
            TransformHeader headers = new TransformHeader();
            headers.Add(HeaderProperty.Host, $"{MulticastAddress}:{ProtocolPort }");
            headers.Add("SERVER", $"{Server}");
            headers.Add("NT", $"{NT}");
            headers.Add("NTS", SSDPType.Alive.ToString());
            headers.Add("USN", $"{USN}");
            headers.Add("LOCATION", $"{Location}");
            headers.Add(HeaderProperty.CacheControl, $"max-age = {CacheTimeout}");
            return headers;
        }
        public static new AlivePackage Parse(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 查询头信息
    /// <para>
    ///     发送包设置<see cref="SearchPackage.MAN"/>参数无效，默认为 "ssdp:discover"
    /// </para>
    /// </summary>
    public class SearchPackage:AdvertisePackage
    {
        public string MAN { get; set; }
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
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public  TransformHeader GetHeaders()
        {
            TransformHeader headers = new TransformHeader();
            headers.Add(HeaderProperty.Host, $"{MulticastAddress}:{ProtocolPort}");
            headers.Add("S", $"{S}");
            headers.Add("MAN", "\""+SSDPType.Discover.ToString()+"\"");
            headers.Add("ST", $"{ST}");
            headers.Add("MX", $"{MX}");
            return headers;
        }
        public static SearchPackage Parse(byte[] data)
        {
            throw new NotImplementedException();
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
            headers.Add("NTS", "\"" + SSDPType.Byebye.ToString()+"\"");
            headers.Add("USN", $"{USN}");
            return headers;
        }

        public static ByebyePackage Parse(byte[] data)
        {
            throw new NotImplementedException();
        }
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
    /// 公告包
    /// </summary>
    public class AdvertisePackage
    {
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
        //组播地址，固定地址
        public const string MulticastAddress = "239.255.255.250";
        //组播地址IPv6
        public const string MulticastAddressIPv6 = "FF0x::C";
        //组播端口
        public const int ProtocolPort = 1900;
    }
}
