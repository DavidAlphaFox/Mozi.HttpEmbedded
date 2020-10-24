using System.Net;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// UDP通讯缓冲对象
    /// </summary>
    public class UDPStateObject : StateObject
    {
        public EndPoint RemoteEndPoint { get; set; }
    }
}
