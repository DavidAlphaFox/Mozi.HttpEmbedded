using System.Net.Sockets;
using Mozi.HttpEmbedded;

namespace Mozi.SSDP
{
    /// <summary>
    /// SOCKET
    /// </summary>
    public class UdpSocket
    {
        //
        private string BroadCastAddress = "239.255.255.250";
        
        private RequestMethod MSEARCH = new RequestMethod("M-SEARCH");
        private const string  QueryPath= "*";
        private HttpVersion   QueryProtocolType = Mozi.HttpEmbedded.HttpVersion.Version11;
        private Socket _socket;
        public UdpSocket()
        {
            _socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,System.Net.Sockets.ProtocolType.Udp);
        }

        /// <summary>
        /// 发送查询消息
        /// </summary>
        public void Discover()
        {
            HttpRequest request = new HttpRequest();
            request.Path = "*";
        }

        public void HandleDiscoverMessage()
        {

        }

        /// <summary>
        /// 发送存在通知
        /// </summary>
        public void Notification()
        {

        }
    }
}
