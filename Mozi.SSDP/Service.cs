using System.Net;
using System.Net.Sockets;
using Mozi.HttpEmbedded;

namespace Mozi.SSDP
{
    /// <summary>
    /// SSDP协议实现
    /// </summary>
    public class Service
    {
        private string BroadcastAddress = "239.255.255.250";
        private int ProtocolPort = 1900;
        private RequestMethod MSEARCH = new RequestMethod("M-SEARCH");
        private RequestMethod NOTIFY = new RequestMethod("NOTIFY");
        private const string QueryPath = "*";

        private Socket _socket;

        public Service()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Udp);
            _socket.Bind(new IPEndPoint(IPAddress.Any, ProtocolPort));
        }
        /// <summary>
        /// 激活
        /// </summary>
        /// <returns></returns>
        public Service Active()
        {
            _socket.Listen(10);
            return this;
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        public Service Showdown()
        {
            _socket.Shutdown(SocketShutdown.Both);
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
        public void Discover()
        {
            HttpRequestU request = new HttpRequestU();
            request.SetPath("*").SetMethod(MSEARCH);
            TransformHeader headers = new TransformHeader();

            headers.Add(HeaderProperty.Host, $"{BroadcastAddress}:{ProtocolPort}");
            headers.Add("S", "uuid:ijklmnop-7dec-11d0-a765-00a0c91e6bf6");
            headers.Add("MAN", "\"ssdp:discover\"");
            headers.Add("ST", "mozi-embedded:simplehost");
            headers.Add("MX", "3");
            request.SetHeaders(headers);
            _socket.Send(request.GetBuffer());
        }

        //NOTIFY* HTTP/1.1
        //Host: 239.255.255.250:reservedSSDPport
        //NT: blenderassociation:blender
        //NTS: ssdp:alive
        //USN: someunique:idscheme3
        //AL: <blender:ixl><http://foo/bar>
        //Cache-Control: max-age = 7393
        
        /// <summary>
        /// 发送存在通知
        /// </summary>
        public void Notification()
        {
            HttpRequestU request = new HttpRequestU();
            request.SetPath("*").SetMethod(NOTIFY);
            TransformHeader headers = new TransformHeader();
            headers.Add(HeaderProperty.Host, $"{BroadcastAddress}:{ProtocolPort}");
            headers.Add("NT","mozi-embedded:simplehost");
            headers.Add("NTS", SSDPType.Alive.ToString());
            headers.Add("USN", "mozi-embedded:simplehost");
            headers.Add("AL", "");
            headers.Add(HeaderProperty.CacheControl, "max-age= 3600");
            request.SetHeaders(headers);
            _socket.Send(request.GetBuffer());
        }

        //NOTIFY* HTTP/1.1
        //Host: 239.255.255.250:reservedSSDPport
        //NT: someunique:idscheme3
        //NTS: ssdp:byebye
        //USN: someunique:idscheme3

        /// <summary>
        /// 离线通知
        /// </summary>
        public void Leave()
        {
            HttpRequestU request = new HttpRequestU();
            request.SetPath("*").SetMethod(NOTIFY);
            TransformHeader headers = new TransformHeader();
            headers.Add(HeaderProperty.Host, $"{BroadcastAddress}:{ProtocolPort}");
            headers.Add("NT", "mozi-embedded:simplehost");
            headers.Add("NTS", SSDPType.Byebye.ToString());
            headers.Add("USN", "mozi-embedded:simplehost");
            request.SetHeaders(headers);
            _socket.Send(request.GetBuffer());
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
            resp.AddHeader(HeaderProperty.Host, $"{BroadcastAddress}:{ProtocolPort}");
            resp.AddHeader(HeaderProperty.CacheControl, "max-age=3600");
            resp.AddHeader("EXT", "");
            resp.AddHeader(HeaderProperty.Location, "http://127.0.0.1/desc.xml");
            resp.AddHeader("Server", "");
            resp.AddHeader("ST", "mozi-embedded:simplehost");
            resp.AddHeader("USN", "mozi-embedded:simplehost");
            resp.SetStatus(StatusCode.Success);
            _socket.Send(resp.GetBuffer());
        }
    }
}
