using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Mozi.HttpEmbedded;
using Mozi.HttpEmbedded.Secure;

namespace Mozi.SSDP
{
    /// <summary>
    /// 状态服务
    /// </summary>
    public class StateService
    {

        private int _port = 13453;

        private string _host = "127.0.0.1";

        protected Socket _sc;

        private bool active = false;

        private readonly Timer _timeLooper;

        private int _interval=30*1000;
        
        private readonly StatePackage _sp=new StatePackage 
        { 
            DeviceName="Mozi",
            DeviceId="00010001",
            StateName="alive"
        };

        public StateService()
        {
             _timeLooper = new Timer(TimerCallbackInvoker, this, Timeout.Infinite, Timeout.Infinite);
        }

        ~StateService()
        {
            if (_timeLooper != null)
            {
                _timeLooper.Dispose();
            }

            if (_sc != null)
            {
                _sc.Dispose();
            }
        }
        /// <summary>
        /// 服务启动事件
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
        /// 服务停用事件
        /// </summary>
        public event AfterServerStop AfterServerStop;
        public string RemoteHost
        {
            get { return _host; }
            set { _host = value; }
        }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }
        /// <summary>
        /// 设置心跳周期
        /// </summary>
        public int Interval
        {
            get { return _interval; }
            set { _interval = value; }
        }
        public Socket SocketMain
        {
            get { return _sc; }
        }

        public void Activate()
        {
            active = true;
            _timeLooper.Change(0, _interval);
        }

        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void Inactivate()
        {
            active = false;
            _timeLooper.Change(Timeout.Infinite, Timeout.Infinite);
        }
        public void TimerCallbackInvoker(object sender)
        {
            if (this.active)
            {
                Notify();
            }
            else
            {

            }
        }

        public StateService ApplyDevice(string deviceName,string deviceId)
        {
            _sp.DeviceName = deviceName;
            _sp.DeviceId = deviceId;
            return this;
        }

        public StateService SetState(string stateName)
        {
            _sp.StateName = stateName;
            return this;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="port"></param>
        public void Init()
        {
            if (_sc == null)
            {
                _sc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
            }
            else
            {
                _sc.Close();
            }
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, _port);
            //允许端口复用
            _sc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); 
            _sc.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, 100);

            //_sc.Bind(endpoint);

            ////回调服务器启动事件
            //UDPStateObject so = new UDPStateObject()
            //{
            //    WorkSocket = _sc,
            //    Id = Guid.NewGuid().ToString(),
            //    //IP = ((System.Net.IPEndPoint)client.RemoteEndPoint).Address.ToString(),
            //    //RemotePort = ((System.Net.IPEndPoint)client.RemoteEndPoint).Port,
            //    RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0)
            //};

            //if (OnServerStart != null)
            //{
            //    OnServerStart(this, new ServerArgs() { StartTime = DateTime.Now, StopTime = DateTime.MinValue });
            //}
            //try
            //{
            //    _sc.BeginReceiveFrom(so.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ref so.RemoteEndPoint, CallbackReceive, so);
            //    if (OnReceiveStart != null)
            //    {
            //        OnReceiveStart.BeginInvoke(this, new DataTransferArgs(), null, null);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    var ex2 = ex;
            //}
        }

        public void Notify()
        {
            if (!string.IsNullOrEmpty(_host))
            {
                _sc.SendTo(_sp.Pack(), new IPEndPoint(IPAddress.Parse(_host), _port));
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

            int iByteRead = client.EndReceiveFrom(iar, ref remote);

            if (iByteRead > 0)
            {
                //置空数据连接
                so.ResetBuffer(iByteRead);
                if (client.Available > 0)
                {
                    so.RemoteEndPoint = remote;
                    client.BeginReceiveFrom(so.Buffer, 0, so.Buffer.Length, SocketFlags.None, ref so.RemoteEndPoint, new AsyncCallback(CallbackReceive), so);
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
        private void InvokeAfterReceiveEnd(UDPStateObject so, Socket client, EndPoint remote)
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
            UDPStateObject stateobject = new UDPStateObject()
            {
                WorkSocket = _sc,
                Id = Guid.NewGuid().ToString(),
                //IP = ((System.Net.IPEndPoint)client.RemoteEndPoint).Address.ToString(),
                //RemotePort = ((System.Net.IPEndPoint)client.RemoteEndPoint).Port,
                RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0)
            };
            _sc.BeginReceiveFrom(so.Buffer, 0, so.Buffer.Length, SocketFlags.None, ref so.RemoteEndPoint, new AsyncCallback(CallbackReceive), so);
        }
    }

    //statename:alive|byebye|busy|idle

    public class StatePackage
    {
        public ushort PackageLength     { get; set; }
        public ushort StateNameLength   { get; set; }
        public string StateName         { get; set; }
        public ushort DeviceNameLength  { get; set; }
        public string DeviceName        { get; set; }
        public string DeviceIdLength    { get; set; }
        public string DeviceId          { get; set; }
        public long   Timestamp         { get; set; }

        public byte[] Pack()
        {
            List<byte> arr = new List<byte>();
            
            byte[] stateName = StateName.ToBytes();
            byte[] deviceName = DeviceName.ToBytes();
            byte[] deviceId = DeviceId.ToBytes();            
            Timestamp = DateTime.Now.ToTimestamp();

            arr.AddRange(Timestamp.ToBytes());

            arr.InsertRange(0, deviceId);
            arr.InsertRange(0, ((ushort)deviceId.Length).ToBytes());

            arr.InsertRange(0, deviceName);
            arr.InsertRange(0, ((ushort)deviceName.Length).ToBytes());

            arr.InsertRange(0, stateName);
            arr.InsertRange(0,((ushort)stateName.Length).ToBytes());

            arr.InsertRange(0, ((ushort)arr.Count).ToBytes());

            return arr.ToArray();
        }

        public static StateObject Parse()
        {
            StateObject state = new StateObject();
            return state;
        }
    }

    internal static class Extension
    {
        public static byte[] ToBytes(this string data)
        {
            return System.Text.Encoding.ASCII.GetBytes(data);
        }
    }
}
