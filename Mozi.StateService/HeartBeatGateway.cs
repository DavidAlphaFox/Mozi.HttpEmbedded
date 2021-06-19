using System;
using System.Collections.Generic;

namespace Mozi.StateService
{

    public delegate void ClientUserChange(object sender, ClientAliveInfo client, string oldUser, string newUser);

    public delegate void ClientLifeStateChange(object sender, ClientAliveInfo clientInfo, ClientLifeState oldState, ClientLifeState newState);

    public delegate void ClientOnlineStateChange(object sender, ClientAliveInfo clientInfo, ClientState oldState, ClientState newState);

    public delegate void ClientAddRemove(object sender, ClientAliveInfo clientInfo);

    public delegate void ClientMessageReceive(object sender, ClientAliveInfo clientInfo,string host,int port);

    public class ClientStateChangeArgs : EventArgs
    {
        public HeartBeatPackage BeatPackage { get; set; }
    }

    public class ClientAliveInfo
    {
        public ClientLifeState State   { get; set; }
        public ClientState ClientState { get; set; }
        public string DeviceName  { get; set; }
        public string DeviceId    { get; set; }
        public string AppVersion  { get; set; }
        public string UserName    { get; set; } 
        public DateTime BeatTime  { get; set; }
        public DateTime OnTime    { get; set; }
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
    /// <summary>
    /// 终端状态
    /// </summary>
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
        private List<ClientAliveInfo> _clients = new List<ClientAliveInfo>();

        /// <summary>
        /// 服务端端口
        /// </summary>
        public  int Port { get { return _port; } }
        /// <summary>
        /// 终端加入事件
        /// </summary>
        public event ClientAddRemove OnClientJoin;
        /// <summary>
        /// 终端通知状态变更
        /// </summary>
        public event ClientLifeStateChange OnClientLifeStateChange;
        /// <summary>
        /// 终端在线状态变更事件
        /// </summary>
        public event ClientOnlineStateChange OnClientOnlineStateChange;
        /// <summary>
        /// 终端在线用户变更
        /// </summary>
        public event ClientUserChange OnClientUserChange;
        /// <summary>
        /// 终端消息接收事件
        /// </summary>
        public event ClientMessageReceive OnClientMessageReceive;
        /// <summary>
        /// 终端列表
        /// </summary>
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
        public void SetUserName(ref ClientAliveInfo client, string userName)
        {
            if (client != null)
            {
                var clientOldUserName = client.UserName;
                client.UserName = userName;
                if (client.UserName != clientOldUserName && OnClientUserChange != null)
                {
                    try
                    {
                        //触发终端状态变更事件
                        OnClientUserChange.BeginInvoke(this, client, clientOldUserName, client.UserName, null, null);
                    }
                    finally
                    {

                    }
                }
            }
        }
        private void SetClientLifeState(ref ClientAliveInfo client, ClientLifeState state)
        {
            if (client != null)
            {
                var clientOldState = client.State;
                client.State = state;
                if (client.State != clientOldState && OnClientLifeStateChange != null)
                {
                    try
                    {
                        //触发终端状态变更事件
                        OnClientLifeStateChange.BeginInvoke(this, client, clientOldState, client.State, null, null);
                    }
                    finally
                    {

                    }
                }
            }
        }
        /// <summary>
        /// 设置终端状态
        /// <para>
        /// 如非必要不要调用这个方法
        /// </para>
        /// </summary>
        /// <param name="ca"></param>
        /// <param name="state"></param>
        public void SetClientState(ClientAliveInfo ca,ClientState state)
        {
            var client = _clients.Find(x => x.DeviceName.Equals(ca.DeviceName) && x.DeviceId.Equals(ca.DeviceId));
            if (client != null)
            {
                var clientOldState = client.ClientState;
                client.ClientState = state;
                if (client.ClientState != clientOldState && OnClientOnlineStateChange != null)
                {
                    try
                    {
                        //触发终端状态变更事件
                        OnClientOnlineStateChange.BeginInvoke(this, client, clientOldState, client.ClientState, null, null);
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
        /// <summary>
        /// 服务端检活
        /// </summary>
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
                client.UserName = ca.UserName;
                SetUserName(ref client, ca.UserName);
                SetClientLifeState(ref client,ca.State);
            }
            else
            {
                ca.OnTime = DateTime.Now;
                _clients.Add(ca);
                client = ca;
                //终端加入事件
                if (OnClientJoin != null)
                {
                    OnClientJoin.BeginInvoke(this, ca,null,null);
                }
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
                    State=(ClientLifeState)Enum.Parse(typeof(ClientLifeState),pg.StateName.ToString())
                };
                var client=UpdateClient(ca);
                if (OnClientMessageReceive != null)
                {
                    OnClientMessageReceive.BeginInvoke(this, client,args.IP,args.Port, null, null);
                }
            }
            catch(Exception ex)
            {
                var ex2 = ex;
            }
        }
    }
}
