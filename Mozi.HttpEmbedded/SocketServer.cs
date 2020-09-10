using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 异步单线程
    /// </summary>
    public class SocketServer
    {
        //private static SocketServer _mSocketServer;
        private int _iPort = 9000;
        private Dictionary<string,Socket> _socketDocker;
        private Socket sc;
        /// <summary>
        /// 服务器启动事件
        /// </summary>
        public event ServerStart OnServerStart;
        /// <summary>
        /// 客户端连接事件
        /// </summary>
        public event ClientConnect OnClientConnect;
        public event ClientDisConnect AfterClientDisConnect;
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
            get { return _iPort; }
        }

        public Socket SocketMain
        {
            get { return sc; }
        }

        public SocketServer() 
        {
            _socketDocker = new Dictionary<string, Socket>();
        }


        //TODO 测试此处是否有BUG
        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <param name="port"></param>
        public void StartServer(int port)
        {
            _iPort = port;
            if (sc == null)
            {
                sc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            }
            else
            {
                sc.Close();
            }
            System.Net.IPEndPoint endpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, _iPort);
            sc.Bind(endpoint);
            sc.Listen(_iPort);            
            //回调服务器启动事件
            if (OnServerStart != null) 
            {
                OnServerStart(this, new ServerArgs() { StartTime=DateTime.Now,StopTime=DateTime.MinValue });
            }
            sc.BeginAccept(new AsyncCallback(CallbackAccept), sc);
        }
        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void StopServer() 
        {
            _socketDocker.Clear();
            try
            {
                sc.Shutdown(SocketShutdown.Both);
                if (AfterServerStop != null) 
                {
                    AfterServerStop(sc, null);
                }
            }
            catch 
            { 
            
            }
        }
        /// <summary>
        /// 开始连接回调
        /// </summary>
        /// <param name="iar"></param>
        private void CallbackAccept(IAsyncResult iar)
        {
            Socket server = (Socket)iar.AsyncState;
            Socket client = server.EndAccept(iar);
            
            server.BeginAccept(CallbackAccept, server);
            if (OnClientConnect != null) 
            {
                OnClientConnect(this, new ClientConnectArgs() { 
                    Client=client
                });
            }
            StateObject so = new StateObject() 
            { 
                WorkSocket = client,
                Id = Guid.NewGuid().ToString(),
                IP = ((System.Net.IPEndPoint)client.RemoteEndPoint).Address.ToString(),
                RemotePort = ((System.Net.IPEndPoint)client.RemoteEndPoint).Port,
            };
            _socketDocker.Add(so.Id, client);
            try
            {
                client.BeginReceive(so.Buffer, 0, StateObject.BufferSize, 0, CallbackReceive, so);
                if (OnReceiveStart != null)
                {
                    OnReceiveStart(this, new DataTransferArgs());
                }
            }
            catch(Exception ex)
            {
                var ex2 = ex;
            }
        }
        /// <summary>
        /// 接收数据回调
        /// </summary>
        /// <param name="iar"></param>
        private void CallbackReceive(IAsyncResult iar)
        {
            StateObject so = (StateObject)iar.AsyncState;
            Socket client = so.WorkSocket;
            if (client.Connected)
            {
                int iByteRead = client.EndReceive(iar);
                   
                if (iByteRead >0)
                {
                    //置空数据连接
                    so.ResetBuffer(iByteRead);
                    if (client.Available > 0)
                    {
                        //Thread.Sleep(10);
                        client.BeginReceive(so.Buffer, 0, StateObject.BufferSize, 0, CallbackReceive, so);
                    }
                    else
                    {
                        _socketDocker.Remove(so.Id);
                        if (AfterReceiveEnd != null)
                        {
                            AfterReceiveEnd(this,
                                new DataTransferArgs()
                                {
                                    Data = so.Data.ToArray(),
                                    IP = so.IP,
                                    Port = so.RemotePort,
                                    Socket = so.WorkSocket
                                });
                        }
                    }
                }
                else 
                {
                    _socketDocker.Remove(so.Id);
                    if (AfterReceiveEnd != null)
                    {
                        AfterReceiveEnd(this,
                            new DataTransferArgs()
                            {
                                Data = so.Data.ToArray(),
                                IP = so.IP,
                                Port = so.RemotePort,
                                Socket = so.WorkSocket
                            });
                    }
                }
            }
            else 
            {
                _socketDocker.Remove(so.Id);
                if (AfterReceiveEnd != null)
                {
                    AfterReceiveEnd(this,
                        new DataTransferArgs()
                        {
                            Data = so.Data.ToArray(),
                            IP = so.IP,
                            Port = so.RemotePort,
                            Socket = so.WorkSocket
                        });
                }
                client.Dispose();
            }
        }
    }
}
