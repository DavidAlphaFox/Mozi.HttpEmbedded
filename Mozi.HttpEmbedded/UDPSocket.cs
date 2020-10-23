using System;
using System.Net.Sockets;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// UDP套接字
    /// </summary>
    public class UDPSocket
    {
        protected int _iport = 80;

        protected Socket _sc;

        public UDPSocket()
        {
          
        }

        public new void StartServer(int port)
        {
            _iport = port;
            if (_sc == null)
            {
                _sc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
            }
            else
            {
                _sc.Close();
            }
            System.Net.IPEndPoint endpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, _iport);
            //允许端口复用
            _sc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _sc.Bind(endpoint);
            //回调服务器启动事件
            StateObject so = new StateObject()
            {
                //WorkSocket = client,
                Id = Guid.NewGuid().ToString(),
                //IP = ((System.Net.IPEndPoint)client.RemoteEndPoint).Address.ToString(),
                //RemotePort = ((System.Net.IPEndPoint)client.RemoteEndPoint).Port,
            };
            if (OnServerStart != null)
            {
                OnServerStart(this, new ServerArgs() { StartTime = DateTime.Now, StopTime = DateTime.MinValue });
            }
            try
            {

                _sc.BeginReceive(so.Buffer, 0, StateObject.BufferSize, SocketFlags.None, CallbackReceive, so);
                if (OnReceiveStart != null)
                {
                    OnReceiveStart.BeginInvoke(this, new DataTransferArgs(),null,null);
                }
            }
            catch (Exception ex)
            {
                var ex2 = ex;
            }
        }

        /// <summary>
        /// 服务器启动事件
        /// </summary>
        public event ServerStart OnServerStart;
        /// <summary>
        /// 数据接收开始事件
        /// </summary>
        public event ReceiveStart OnReceiveStart;
        /// <summary>
        /// 数据接收完成事件
        /// </summary>
        public event ReceiveEnd AfterReceiveEnd;
        /// <summary>
        /// 服务器停用事件
        /// </summary>
        public event AfterServerStop AfterServerStop;

        /// <summary>
        /// 端口
        /// </summary>
        public int Port
        {
            get { return _iport; }
        }
        public Socket SocketMain
        {
            get { return _sc; }
        }
        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void StopServer()
        {
            try
            {
                _sc.Shutdown(SocketShutdown.Both);
                if (AfterServerStop != null)
                {
                    AfterServerStop(_sc, null);
                }
            }
            catch
            {

            }
        }
        /// <summary>
        /// 接收数据回调
        /// </summary>
        /// <param name="iar"></param>
        protected void CallbackReceive(IAsyncResult iar)
        {
            StateObject so = (StateObject)iar.AsyncState;
            Socket client = so.WorkSocket;
            if (client.Connected)
            {
                int iByteRead = client.EndReceive(iar);

                if (iByteRead > 0)
                {
                    //置空数据连接
                    so.ResetBuffer(iByteRead);
                    if (client.Available > 0)
                    {
                        //Thread.Sleep(10);
                        client.BeginReceive(so.Buffer, 0, StateObject.BufferSize, SocketFlags.None, CallbackReceive, so);
                    }
                    else
                    {
                        InvokeAfterReceiveEnd(so, client);
                    }
                }
                else
                {
                    InvokeAfterReceiveEnd(so, client);
                }
            }
            else
            {
                InvokeAfterReceiveEnd(so, client);
            }
        }
        private void InvokeAfterReceiveEnd(StateObject so, Socket client)
        {
            if (AfterReceiveEnd != null)
            {
                AfterReceiveEnd.BeginInvoke(this,
                    new DataTransferArgs()
                    {
                        Data = so.Data.ToArray(),
                        IP = so.IP,
                        Port = so.RemotePort,
                        Socket = so.WorkSocket
                    }, null, null);
            }
        }
    }
}
