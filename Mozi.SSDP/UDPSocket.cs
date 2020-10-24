﻿using System;
using System.Net;
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

        //private EndPoint _remoteEndPoint=new IPEndPoint(IPAddress.Any, 0);

        public UDPSocket()
        {
          
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
        public void StartServer(int port)
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
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, _iport);
            //允许端口复用
            _sc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            MulticastOption mcastOpt = new MulticastOption(IPAddress.Parse("239.255.255.250"), IPAddress.Any);
            // Add membership to the group.
            _sc.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, mcastOpt);

            _sc.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, 0);
            _sc.Bind(endpoint);

            EndPoint _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            //回调服务器启动事件
            UDPStateObject so = new UDPStateObject()
            {
                WorkSocket = _sc,
                Id = Guid.NewGuid().ToString(),
                //IP = ((System.Net.IPEndPoint)client.RemoteEndPoint).Address.ToString(),
                //RemotePort = ((System.Net.IPEndPoint)client.RemoteEndPoint).Port,
                RemoteEndPoint= _remoteEndPoint
            };
            if (OnServerStart != null)
            {
                OnServerStart(this, new ServerArgs() { StartTime = DateTime.Now, StopTime = DateTime.MinValue });
            }
            try
            {

                _sc.BeginReceiveFrom(so.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ref _remoteEndPoint, CallbackReceive, so);
                if (OnReceiveStart != null)
                {
                    OnReceiveStart.BeginInvoke(this, new DataTransferArgs(), null, null);
                }
            }
            catch (Exception ex)
            {
                var ex2 = ex;
            }
        }
        /// <summary>
        /// 接收数据回调
        /// </summary>
        /// <param name="iar"></param>
        protected void CallbackReceive(IAsyncResult iar)
        {
            UDPStateObject so = (UDPStateObject)iar.AsyncState;
            Socket client = so.WorkSocket;

            EndPoint remote = (IPEndPoint)so.RemoteEndPoint;

            int iByteRead = client.EndReceiveFrom(iar,ref remote);
            
            //EndPoint remoteEndPoint = remote;

            if (iByteRead > 0)
            {
                //置空数据连接
                so.ResetBuffer(iByteRead);
                if (client.Available > 0)
                {
                    //Thread.Sleep(10);
                    so.RemoteEndPoint = remote;
                    _sc.BeginReceiveFrom(so.Buffer, 0, so.Buffer.Length, SocketFlags.None, ref remote, CallbackReceive, so);
                }
                else
                {
                    InvokeAfterReceiveEnd(so, client, (IPEndPoint)remote);
                }
            }
            else
            {
                InvokeAfterReceiveEnd(so, client, (IPEndPoint)remote);
            }
        }
        private void InvokeAfterReceiveEnd(UDPStateObject so, Socket client,EndPoint remote)
        {
            if (AfterReceiveEnd != null)
            {
                AfterReceiveEnd.BeginInvoke(this,
                    new DataTransferArgs()
                    {
                        Data = so.Data.ToArray(),
                        IP = ((IPEndPoint)remote).Address.ToString(),
                        Port = ((IPEndPoint)remote).Port,
                        Socket = so.WorkSocket
                    }, null, null);
            }
            EndPoint _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            UDPStateObject stateobject = new UDPStateObject()
            {
                WorkSocket = _sc,
                Id = Guid.NewGuid().ToString(),
                //IP = ((System.Net.IPEndPoint)client.RemoteEndPoint).Address.ToString(),
                //RemotePort = ((System.Net.IPEndPoint)client.RemoteEndPoint).Port,
                RemoteEndPoint = _remoteEndPoint
            };
            _sc.BeginReceiveFrom(so.Buffer, 0, so.Buffer.Length, SocketFlags.None, ref _remoteEndPoint, CallbackReceive, so);
        }
    }
}