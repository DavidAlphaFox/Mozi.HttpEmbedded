using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Mozi.StateService
{
    //TODO 是否考虑建立双向心跳服务，类似于握手
    //TODO 是否考虑设置一个备份地址
    /// <summary>
    /// 状态服务
    /// </summary>
    public class HeartBeatService
    {
        

        private int _port = DefaultPort;

        private string _host = "127.0.0.1";

        protected Socket _sc;

        private bool active = false;

        private readonly Timer _timeLooper;

        private int _interval = 30 * 1000;

        private readonly HeartBeatPackage _sp = new HeartBeatPackage
        {
            DeviceName = "Mozi",
            DeviceId = "00010001",
            StateName = 0x31,
            Version =0x31,
            AppVersion = "1.0.0",
            UserName="",
        };

        private  IPEndPoint _endPoint;
        /// <summary>
        /// 套接字是否已初始化
        /// </summary>
        private bool _socketInitialized = false;

        public const int DefaultPort = 13453; 

        public HeartBeatService()
        {
            InitRemoteEndpoint();
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
        /// <summary>
        ///服务器地址 
        /// </summary>
        public string RemoteHost
        {
            get { return _host; }
            set {
                _host = value;
                InitRemoteEndpoint();
            }
        }
        /// <summary>
        /// 状态变更通知
        /// <para>
        /// 开启此参数会立即向服务器发出数据包
        /// </para>
        /// </summary>
        public bool StateChangeNotifyImmediately { get; set; }
        public bool UserChangeNotifyImmediately { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port
        {
            get { return _port; }
            set { 
                _port = value;
                InitRemoteEndpoint();
            }
        }
        /// <summary>
        /// 设置心跳周期 默认30秒
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
        /// <summary>
        /// 定时回调
        /// </summary>
        /// <param name="sender"></param>
        private void TimerCallbackInvoker(object sender)
        {
            if (active)
            {
                SendPack();
            }
            else
            {

            }
        }
        /// <summary>
        /// 配置终端信息
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="deviceId"></param>
        /// <param name="appVersion"></param>
        /// <returns></returns>
        public HeartBeatService ApplyDevice(string deviceName, string deviceId,string appVersion)
        {
            _sp.DeviceName = deviceName;
            _sp.DeviceId = deviceId;
            _sp.AppVersion = appVersion;
            return this;
        }
        /// <summary>
        /// 设置用户名
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public HeartBeatService SetUserName(string userName)
        {
            var oldUserName = _sp.UserName;
            _sp.UserName = userName;
            if (oldUserName != _sp.UserName && UserChangeNotifyImmediately)
            {
                SendPack();
            }
            return this;
        }
        /// <summary>
        /// 设置终端状态
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public HeartBeatService SetState(ClientLifeState stateName)
        {
            var oldState = _sp.StateName;
            _sp.StateName = (byte)stateName;
            if (oldState != _sp.StateName&&StateChangeNotifyImmediately)
            {
                SendPack();
            }
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
            //允许端口复用
            _sc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _sc.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, 100);
        }
        /// <summary>
        /// 初始化终结点
        /// </summary>
        private void InitRemoteEndpoint()
        {
            _endPoint = new IPEndPoint(IPAddress.Parse(_host), _port);
            _socketInitialized = true;
        }
        /// <summary>
        /// 发送数据包
        /// </summary>
        private void SendPack()
        {
            if (!string.IsNullOrEmpty(_host)&&_socketInitialized)
            {
                _sc.SendTo(_sp.Pack(), _endPoint);
            }
        }
        /// <summary>
        /// 终端状态 Alive
        /// </summary>
        public void Alive()
        {
            SetState(ClientLifeState.Alive);
        }
        /// <summary>
        /// 终端状态 Leave
        /// </summary>
        public void Leave()
        {
            SetState(ClientLifeState.Byebye);
        }
        /// <summary>
        /// 终端状态 Busy
        /// </summary>
        public void Busy()
        {
            SetState(ClientLifeState.Busy);
        }
        /// <summary>
        /// 终端状态 Idle
        /// </summary>
        public void Idle()
        {
            SetState(ClientLifeState.Idle);
        }
    }
    /// <summary>
    /// 客户机状态类型
    /// </summary>
    public enum ClientLifeState
    {
        Unknown=0,
        Alive=0x31,
        Byebye= 0x32,
        Busy= 0x33,
        Idle= 0x34
    }

    //statename:alive|byebye|busy|idle|offline

    /// <summary>
    /// 状态数据协议包
    /// <para>
    ///     <see cref="DeviceName"/>和<see cref="DeviceId"/>为主键值，区分业务类型和终端标识
    /// </para>
    /// <para>
    /// 所有字符串均按ASCII编码，字符集不能超过ASCII
    /// </para>
    /// </summary>
    public class HeartBeatPackage
    {
        public byte Version { get; set; }
        public ushort PackageLength { get; set; }
        public byte StateName { get; set; }
        public ushort DeviceNameLength { get; set; }
        public string DeviceName { get; set; }
        public ushort DeviceIdLength { get; set; }
        public string DeviceId { get; set; }
        public ushort AppVersionLength { get; set; }
        public string AppVersion { get; set; }        
        public ushort UserNameLength { get; set; }
        public string UserName { get; set; }
        /// <summary>
        /// UTC时间戳 
        /// </summary>
        public long Timestamp { get; set; }

        public byte[] Pack()
        {
            List<byte> arr = new List<byte>();

            byte[] deviceName = DeviceName.ToBytes();
            byte[] deviceId = DeviceId.ToBytes();
            byte[] appVersion = AppVersion.ToBytes();
            byte[] userName = UserName.ToBytes();

            Timestamp = DateTime.Now.ToTimestamp();

            arr.AddRange(Timestamp.ToBytes());
            arr.InsertRange(0, userName);
            arr.InsertRange(0, ((ushort)userName.Length).ToBytes());

            arr.InsertRange(0, appVersion);
            arr.InsertRange(0, ((ushort)appVersion.Length).ToBytes());

            arr.InsertRange(0, deviceId);
            arr.InsertRange(0, ((ushort)deviceId.Length).ToBytes());

            arr.InsertRange(0, deviceName);
            arr.InsertRange(0, ((ushort)deviceName.Length).ToBytes());

            arr.Insert(0, StateName);

            arr.InsertRange(0, ((ushort)arr.Count).ToBytes());
            arr.Insert(0, Version);
            return arr.ToArray();
        }

        public static HeartBeatPackage Parse(byte[] pg)
        {
            HeartBeatPackage state = new HeartBeatPackage
            {
                Version = pg[0],
                PackageLength = pg.ToUInt16(1)
            };
            byte[] body = new byte[state.PackageLength];
            Array.Copy(pg, 1+2, body, 0, body.Length);

            state.StateName = body[0];
            state.DeviceNameLength = body.ToUInt16(1);
            state.DeviceIdLength = body.ToUInt16(2 * 1 + 1 + state.DeviceNameLength);
            state.AppVersionLength = body.ToUInt16(2 * 2 + 1 + state.DeviceNameLength + state.DeviceIdLength);
            state.UserNameLength = body.ToUInt16(2 * 3 + 1 + state.DeviceNameLength + state.DeviceIdLength+state.AppVersionLength);
            state.Timestamp = body.ToInt64(2 * 4 + 1 + state.DeviceNameLength + state.DeviceIdLength + state.AppVersionLength+state.UserNameLength);

            byte[] deviceName = new byte[state.DeviceNameLength];
            byte[] deviceId = new byte[state.DeviceIdLength];
            byte[] appVersion = new byte[state.AppVersionLength];
            byte[] userName = new byte[state.UserNameLength];

            Array.Copy(body, 2 * 1 + 1, deviceName, 0, state.DeviceNameLength);
            Array.Copy(body, 2 * 2 + 1 + state.DeviceNameLength, deviceId, 0, state.DeviceIdLength);
            Array.Copy(body, 2 * 3 + 1 + state.DeviceNameLength + state.DeviceIdLength, appVersion, 0, state.AppVersionLength);
            Array.Copy(body, 2 * 4 + 1 + state.DeviceNameLength + state.DeviceIdLength, userName, 0, state.UserNameLength);

            state.DeviceName = deviceName.AsString();
            state.DeviceId = deviceId.AsString();
            state.AppVersion = appVersion.AsString();
            state.UserName = userName.AsString();

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

        public static byte ToCharByte(this ClientLifeState state)
        {
            return ((int)state).ToCharByte();
        }
    }
}
