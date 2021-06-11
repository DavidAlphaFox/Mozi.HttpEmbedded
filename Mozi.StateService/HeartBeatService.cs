using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Mozi.StateService
{
    /// <summary>
    /// 状态服务
    /// </summary>
    public class HeartBeatService
    {
        private int _port = 13453;

        private string _host = "127.0.0.1";

        protected Socket _sc;

        private bool active = false;

        private readonly Timer _timeLooper;

        private int _interval = 30 * 1000;

        private readonly StatePackage _sp = new StatePackage
        {
            DeviceName = "Mozi",
            DeviceId = "00010001",
            StateName = "alive",
            Version = 1,
            AppVersion = "1.0.0"
        };

        public HeartBeatService()
        {
            _timeLooper = new Timer(TimerCallbackInvoker, this, Timeout.Infinite, Timeout.Infinite);
        }

        ~HeartBeatService()
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
        /// <summary>
        /// 激活
        /// </summary>
        public void Activate()
        {
            active = true;
            _timeLooper.Change(0, _interval);
        }

        /// <summary>
        /// 停止活动
        /// </summary>
        public void Inactivate()
        {
            active = false;
            _timeLooper.Change(Timeout.Infinite, Timeout.Infinite);
        }
        public void TimerCallbackInvoker(object sender)
        {
            if (active)
            {
                Notify();
            }
            else
            {

            }
        }

        public HeartBeatService ApplyDevice(string deviceName, string deviceId)
        {
            _sp.DeviceName = deviceName;
            _sp.DeviceId = deviceId;
            return this;
        }

        public HeartBeatService SetState(string stateName)
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
                _sc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
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
    }

    //statename:alive|byebye|busy|idle

    /// <summary>
    /// 状态包
    /// </summary>
    public class StatePackage
    {
        public byte Version { get; set; }
        public ushort PackageLength { get; set; }
        public ushort StateNameLength { get; set; }
        public string StateName { get; set; }
        public ushort DeviceNameLength { get; set; }
        public string DeviceName { get; set; }
        public ushort DeviceIdLength { get; set; }
        public string DeviceId { get; set; }
        public ushort AppVersionLength { get; set; }
        public string AppVersion { get; set; }
        public long Timestamp { get; set; }

        public byte[] Pack()
        {
            List<byte> arr = new List<byte>();

            byte[] stateName = StateName.ToBytes();
            byte[] deviceName = DeviceName.ToBytes();
            byte[] deviceId = DeviceId.ToBytes();
            byte[] appVersion = AppVersion.ToBytes();

            Timestamp = DateTime.Now.ToTimestamp();

            arr.AddRange(Timestamp.ToBytes());

            arr.InsertRange(0, appVersion);
            arr.InsertRange(0, ((ushort)appVersion.Length).ToBytes());

            arr.InsertRange(0, deviceId);
            arr.InsertRange(0, ((ushort)deviceId.Length).ToBytes());

            arr.InsertRange(0, deviceName);
            arr.InsertRange(0, ((ushort)deviceName.Length).ToBytes());

            arr.InsertRange(0, stateName);
            arr.InsertRange(0, ((ushort)stateName.Length).ToBytes());

            arr.InsertRange(0, ((ushort)arr.Count).ToBytes());
            arr.Insert(0, Version);
            return arr.ToArray();
        }

        public static StatePackage Parse(byte[] pg)
        {
            StatePackage state = new StatePackage();
            state.Version = pg[0];

            int bodyLen = pg.ToUInt16(1);
            byte[] body = new byte[bodyLen];
            Array.Copy(pg, 3, body, 0, bodyLen);

            state.StateNameLength = body.ToUInt16(0);
            state.DeviceNameLength = body.ToUInt16(2 + state.StateNameLength);
            state.DeviceIdLength = body.ToUInt16(2 * 2 + state.StateNameLength + state.DeviceNameLength);
            state.AppVersionLength = body.ToUInt16(2 * 3 + state.StateNameLength + state.DeviceNameLength + state.DeviceIdLength);

            state.Timestamp = body.ToInt64(2 * 4 + state.StateNameLength + state.DeviceNameLength + state.DeviceIdLength + state.AppVersionLength);

            byte[] stateName = new byte[state.StateNameLength];
            byte[] deviceName = new byte[state.DeviceNameLength];
            byte[] deviceId = new byte[state.DeviceIdLength];
            byte[] appVersion = new byte[state.AppVersionLength];

            Array.Copy(body, 2, stateName, 0, state.StateNameLength);
            Array.Copy(body, 2 * 2 + state.StateNameLength, deviceName, 0, state.DeviceNameLength);
            Array.Copy(body, 2 * 3 + state.StateNameLength + state.DeviceNameLength, deviceId, 0, state.DeviceIdLength);
            Array.Copy(body, 2 * 4 + state.StateNameLength + state.DeviceNameLength + state.DeviceIdLength, appVersion, 0, state.AppVersionLength);

            state.StateName = stateName.AsString();
            state.DeviceName = deviceName.AsString();
            state.DeviceId = deviceId.AsString();
            state.AppVersion = appVersion.AsString();

            return state;
        }
    }

    internal static class Extension
    {
        public static byte[] ToBytes(this string data)
        {
            return System.Text.Encoding.ASCII.GetBytes(data);
        }
        public static string AsString(this byte[] data)
        {
            return System.Text.Encoding.ASCII.GetString(data);
        }
    }
}
