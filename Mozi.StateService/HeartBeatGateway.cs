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

        //public event ClientStateChange OnClientAlive;
        //public event ClientStateChange OnClientLeave;
        public event ClientStateChange OnClientStateChange;

        private List<ClientAliveInfo> _clients = new List<ClientAliveInfo>();

        public HeartBeatGateway()
        {
            _socket = new UDPSocket();
            _socket.AfterReceiveEnd += _socket_AfterReceiveEnd;
        }

        public void Start(int port)
        {
            _socket.Start(port);
        }

        public void Shutdown()
        {
            _socket.Shutdown();
        }
        public void SetClientState(ClientAliveInfo ca,ClientState state)
        {

        }
        public void UpdateClient(ClientAliveInfo ca,ClientLifeState state)
        {
            var client = _clients.Find(x => x.DeviceName.Equals(ca.DeviceName) && x.DeviceId.Equals(ca.DeviceId));
            if (_clients != null)
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

            var clientOldState = client.ClientState;
           
            if (state == ClientLifeState.Byebye)
            {
                client.ClientState = ClientState.Offline;
                client.LeaveTime = DateTime.Now;
            }
            else
            {
                client.ClientState = ClientState.On;
            }

            if (client.ClientState != clientOldState&&OnClientStateChange!=null)
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

        private void _socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            try
            {
                HeartBeatPackage pg = HeartBeatPackage.Parse(args.Data);
                ClientAliveInfo ca = new ClientAliveInfo()
                {
                    DeviceName = pg.DeviceName,
                    DeviceId = pg.DeviceId,
                };
                Console.WriteLine("设备{0},编号{1},状态{2},版本{3},时间{4},{5}", pg.DeviceName,pg.DeviceId,pg.StateName,pg.AppVersion,pg.Timestamp,args.IP);
            }
            catch(Exception ex)
            {
                var ex2 = ex;
            }
        }
    }
}
