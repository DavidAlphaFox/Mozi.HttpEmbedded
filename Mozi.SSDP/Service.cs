using System.Net;
using System.Net.Sockets;
using Mozi.HttpEmbedded;
using Mozi.HttpEmbedded.Generic;

namespace Mozi.SSDP
{
    /// <summary>
    /// SOCKET
    /// </summary>
    public class Service
    {
        //
        private string BroadcastAddress = "239.255.255.250";
        private int ProtocolPort = 1900;
        private RequestMethod MSEARCH = new RequestMethod("M-SEARCH");
        private RequestMethod NOTIFY = new RequestMethod("NOTIFY");
        private const string QueryPath = "*";
        private HttpEmbedded.HttpVersion QueryProtocolType = HttpEmbedded.HttpVersion.Version11;
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
        /// <summary>
        /// 发送查询消息
        /// </summary>
        public void Discover()
        {
            HttpRequestU request = new HttpRequestU();
            request.SetPath("*").SetMethod(MSEARCH);
        }
        /// <summary>
        /// 发送存在通知
        /// </summary>
        public void Notification()
        {
            HttpResponse resp = new HttpResponse();

        }
        /// <summary>
        /// 离线通知
        /// </summary>
        public void Leave()
        {

        }
        public void HandleDiscoverMessage()
        {

        }
    }

    public class SSDPType : AbsClassEnum
    {

        public SSDPType Discover = new SSDPType("discover");
        public SSDPType All = new SSDPType("all");
        public SSDPType Alive = new SSDPType("alive");
        public SSDPType Byebye = new SSDPType("byebye");

        private string _name;

        //discover all alive byebye
        public SSDPType(string name)
        {
            _name = name;
        }

        protected override string Tag => _name;
    }
    /// <summary>
    /// 缓存
    /// </summary>
    public class SSDPCache
    {
        public string USN { get; set; }
        public string ServiceType { get; set; }
        public int Expiration { get; set; }
        public string Location { get; set; }
    }
    /// <summary>
    /// 请求包
    /// </summary>
    public class HttpRequestU:HttpRequest
    {
        public HttpRequestU SetPath(string path)
        {
            Path = path;
            return this;
        }

        public HttpRequestU SetMethod(RequestMethod method)
        {
            Method = method;
            return this;
        }
    }
}
