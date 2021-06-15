using System;
using System.Collections.Generic;

namespace Mozi.StateService
{
    //public delegate void ClientStateChange(object sender, ClientStateChangeArgs args);

    public delegate void ClientStateChange(object sender, ClientAliveInfo clientInfo, ClientState oldState, ClientState newState);

    public class ClientStateChangeArgs : EventArgs
    {
        public HeartBeatPackage BeatPackage { get; set; }
    }
    public class ClientAliveInfo 
    {
        internal ClientLifeState State   { get; set; }
        public ClientState ClientState { get; set; }
        public string DeviceName { get; set; }
        public string DeviceId   { get; set; }
        public string AppVersion { get; set; }
        public string UserName   { get; set; } 
        public DateTime BeatTime { get; set; }
        public DateTime OnTime   { get; set; }
        public DateTime LeaveTime { get; set; }

        public ClientAliveInfo()
        {
            BeatTime = DateTime.MinValue;
            OnTime = DateTime.MinValue;
            LeaveTime = DateTime.MinValue;
            State = ClientLifeState.Unknown;
            ClientState = ClientState.Unknown;
        }
    }

    public enum ClientState
    {
        Unknown=0,
        On=1,
        Offline=2,
        Dead=3
    }
    /// <summary>
    /// 心跳网关服务器
    /// </summary>
    public class HeartBeatGateway
    {
        private readonly UDPSocket _socket;
        private int _timeoutOffline = 180;
        private int _port = 13453;
        public  int Port { get { return _port; } }

        //public event ClientStateChange OnClientAlive;
        //public event ClientStateChange OnClientLeave;
        public event ClientStateChange OnClientStateChange;

        private List<ClientAliveInfo> _clients = new List<ClientAliveInfo>();

        public List<ClientAliveInfo> Clients { get { return _clients; } }

        public HeartBeatGateway()
        {
            _socket = new UDPSocket();
            _socket.AfterReceiveEnd += _socket_AfterReceiveEnd;
        }
        /// <summary>
        /// 以默认端口启动<see cref="F:Port"/>
        /// </summary>
        public void Start()
        {
            Start(_port);
        }
        public void Start(int port)
        {
            _port = port;
            _socket.Start(_port);
        }

        public void Shutdown()
        {
            _socket.Shutdown();
        }
        /// <summary>
        /// 设置终端状态
        /// </summary>
        /// <param name="ca"></param>
        /// <param name="state"></param>
        public void SetClientState(ClientAliveInfo ca,ClientState state)
        {
            var client = _clients.Find(x => x.DeviceName.Equals(ca.DeviceName) && x.DeviceId.Equals(ca.DeviceId));
            if (client != null)
            {
                var clientOldState = client.ClientState;
                ca.ClientState = state;
                if (client.ClientState != clientOldState && OnClientStateChange != null)
                {
                    try
                    {
                        OnClientStateChange.BeginInvoke(this, client, clientOldState, client.ClientState, null, null);
                    }
                    finally
                    {

                    }
                }
            }
        }
        /// <summary>
        /// 将终端置于失效状态
        /// </summary>
        /// <param name="ca"></param>
        public void SetClientDead(ClientAliveInfo ca)
        {
            SetClientState(ca, ClientState.Dead);
        }

        public void CheckClientState()
        {
            foreach(var client in _clients)
            {
                if ((DateTime.Now - client.BeatTime).TotalSeconds > _timeoutOffline)
                {
                    SetClientState(client, ClientState.Offline);
                }
            }
        }

        /// <summary>
        /// 保存终端信息
        /// </summary>
        /// <param name="ca"></param>
        public ClientAliveInfo UpdateClient(ClientAliveInfo ca)
        {
            var client = _clients.Find(x => x.DeviceName.Equals(ca.DeviceName) && x.DeviceId.Equals(ca.DeviceId));
            if (client != null)
            {
                client.AppVersion = ca.AppVersion;
                client.State = ca.State;
                client.UserName = ca.UserName;
            }
            else
            {
                ca.OnTime = DateTime.Now;
                _clients.Add(ca);
                client = ca;
            }
            //统一设置跳动时间
            client.BeatTime = DateTime.Now;
           
            if (client.State == ClientLifeState.Byebye)
            {
                client.LeaveTime = DateTime.Now;
                SetClientState(client, ClientState.Offline);
            }
            else
            {
                SetClientState(client, ClientState.On);
            }
            return client;
        }
        /// <summary>
        /// 数据接收完成回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void _socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            try
            {
                GC.Collect();
                HeartBeatPackage pg = HeartBeatPackage.Parse(args.Data);
                ClientAliveInfo ca = new ClientAliveInfo()
                {
                    DeviceName = pg.DeviceName,
                    DeviceId = pg.DeviceId,
                    AppVersion=pg.AppVersion,
                    UserName=pg.UserName,
                    State=(ClientLifeState)Enum.Parse(typeof(ClientLifeState),pg.StateName.ToCharString())
                };
                var client=UpdateClient(ca);
                Console.WriteLine("{4:yyyyMMdd HH:mm:ss}|设备{0},编号{1},状态{2},版本{3},{5}", client.DeviceName, client.DeviceId, client.State, client.AppVersion, client.BeatTime,args.IP);
            }
            catch(Exception ex)
            {
                var ex2 = ex;
            }
        }
    }
}
