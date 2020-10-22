using System;
using System.Net.Sockets;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// UDP套接字
    /// </summary>
    public class UDPSocket:SocketServer
    {
        public new void StartServer(int port)
        {
            _iport = port;
            if (_sc == null)
            {
                _sc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Udp);
            }
            else
            {
                _sc.Close();
            }
            System.Net.IPEndPoint endpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, _iport);
            //允许端口复用
            _sc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _sc.Bind(endpoint);
            _sc.Listen(_maxListenCount);
            //回调服务器启动事件
            _sc.BeginAccept(new AsyncCallback(CallbackAccept), _sc);
        }
    }
}
