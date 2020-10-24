using System.Net;
using Mozi.HttpEmbedded;

namespace Mozi.SSDP
{
    /// <summary>
    /// UDP通讯缓冲对象
    /// </summary>
    public class UDPStateObject : StateObject
    {
        public EndPoint RemoteEndPoint;
    }
}
