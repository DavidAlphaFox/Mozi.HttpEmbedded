﻿using System;
using System.Collections.Generic;

namespace Mozi.StateService
{

    public delegate void ClientUserChange(object sender, ClientAliveInfo client, string oldUser, string newUser);

    public delegate void ClientLifeStateChange(object sender, ClientAliveInfo clientInfo, ClientLifeState oldState, ClientLifeState newState);

    public delegate void ClientOnlineStateChange(object sender, ClientAliveInfo clientInfo, ClientOnlineState oldState, ClientOnlineState newState);

    public delegate void ClientJoinQuit(object sender, ClientAliveInfo clientInfo);

    public delegate void ClientMessageReceive(object sender, ClientAliveInfo clientInfo,string host,int port);

    public class ClientStateChangeArgs : EventArgs
    {
        public HeartBeatPackage BeatPackage { get; set; }
    }
    /// <summary>
    /// 终端在线信息
    /// </summary>
    public class ClientAliveInfo
    {
        public ClientLifeState State   { get; set; }
        public ClientOnlineState ClientState { get; set; }
        public string DeviceName  { get; set; }
        public string DeviceId    { get; set; }
        public string AppVersion  { get; set; }
        public string UserName    { get; set; }
        /// <summary>
        /// 数据包发送计数
        /// </summary>
        public int BeatCount      { get; set; }
        public DateTime BeatTime  { get; set; }
        public DateTime OnTime    { get; set; }
        public DateTime LeaveTime { get; set; }

        public ClientAliveInfo()
        {
            BeatTime = DateTime.MinValue;
            OnTime = DateTime.MinValue;
            LeaveTime = DateTime.MinValue;
            State = ClientLifeState.Unknown;
            ClientState = ClientOnlineState.Unknown;
        }
    }
    /// <summary>
    /// 终端状态
    /// </summary>
    public enum ClientOnlineState
    {
        Unknown=0,
        On=1,
        Offline=2,
        Lost=3,
        Lazy=4    
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
        public DateTime StartTime { get; private set; }
        /// <summary>
        /// 终端加入事件
        /// </summary>
        public event ClientJoinQuit OnClientJoin;
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
        public bool EnableCount { get; set; }
        /// <summary>
        /// 统计
        /// </summary>
        public ClientStateStatistics Statistics = new ClientStateStatistics();
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
        /// <summary>
        /// 启动网关
        /// </summary>
        /// <param name="port"></param>
        public void Start(int port)
        {
            _port = port;
            _socket.Start(_port);
            StartTime = DateTime.Now;
        }
        /// <summary>
        /// 网关下线
        /// </summary>
        public void Shutdown()
        {
            _socket.Shutdown();
            StartTime = DateTime.MinValue;
        }
        /// <summary>
        /// 设置用户名
        /// </summary>
        /// <param name="client"></param>
        /// <param name="userName"></param>
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
        /// <summary>
        /// 刷新终端心跳状态
        /// </summary>
        /// <param name="client"></param>
        /// <param name="state"></param>
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
                //开启统计功能
                if (EnableCount)
                {
                    Statistics.UpdateClientLiftState(client, state);
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
        public void SetClientState(ClientAliveInfo ca,ClientOnlineState state)
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
                        //触发终端在线状态变更事件
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
        /// <para>若终端长时间无心跳包，可将终端标记为已不可用</para>
        /// </summary>
        /// <param name="ca"></param>
        public void SetClientLost(ClientAliveInfo ca)
        {
            SetClientState(ca, ClientOnlineState.Lost);
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
                    SetClientState(client, ClientOnlineState.Offline);
                }
            }
        }
        /// <summary>
        /// 保存终端信息
        /// </summary>
        /// <param name="ca"></param>
        public ClientAliveInfo UpsertClient(ClientAliveInfo ca)
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
            client.BeatCount++;
            //统一设置跳动时间
            client.BeatTime = DateTime.Now;
           
            if (client.State == ClientLifeState.Byebye)
            {
                client.LeaveTime = DateTime.Now;
                SetClientState(client, ClientOnlineState.Offline);
            }
            else
            {
                SetClientState(client, ClientOnlineState.On);
            }
            return client;
        }
        /// <summary>
        /// 移除指定终端
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="deviceId"></param>
        public void Remove(string deviceName,string deviceId)
        {
            _clients.RemoveAll(x => x.DeviceName == deviceName && x.DeviceId == deviceId);
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
                var client=UpsertClient(ca);
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
    /// <summary>
    /// 终端状态统计
    /// <para>
    /// 最小统计粒度为 天
    /// </para>
    /// </summary>
    public class ClientStateStatistics
    {

        private readonly List<ClientStateSummary> _sum = new List<ClientStateSummary>();
        private readonly List<ClientStateDateSummary> _sumDate = new List<ClientStateDateSummary>();

        private ClientStateSummary Find(string deviceName, string deviceId)
        {
            var summary = _sum.Find(x => x.DeviceName == deviceName && x.DeviceId == deviceId);
            if (summary == null)
            {
                summary = new ClientStateSummary()
                {
                    DeviceName = deviceName,
                    DeviceId = deviceId,
                    OnlineTime = DateTime.Now
                };
                _sum.Add(summary);
            }
            return summary;
        }
        private ClientStateDateSummary FindSummaryDate(string deviceName, string deviceId, string sumDate)
        {
            var summary = _sumDate.Find(x => x.DeviceName == deviceName && x.DeviceId == deviceId && x.SummaryDate == sumDate);
            if (summary == null)
            {
                summary = new ClientStateDateSummary()
                {
                    DeviceName = deviceName,
                    DeviceId = deviceName,
                    OnlineTime = DateTime.Now
                };
                _sum.Add(summary);
            }
            return summary;
        }
        public void UpdateClientLiftState(ClientAliveInfo info, ClientLifeState newState)
        {
            var client = Find(info.DeviceName, info.DeviceId);

            //DONE 处理跨天的问题 
            //汇总统计
            //刷新繁忙时间
            if (newState == ClientLifeState.Busy && client.OnBusyTime == DateTime.MinValue)
            {
                client.OnBusyTime = DateTime.Now;
                client.BusyCount++;

                var clientDate = FindSummaryDate(info.DeviceName, info.DeviceId, DateTime.Now.ToString("yyyyMMdd"));
                clientDate.OnBusyTime = DateTime.Now;
                clientDate.BusyCount++;
            }
            else
            {
                var iOffset = (DateTime.Now - client.OnBusyTime).Days;

                client.BusyTimeTotoal += (long)(DateTime.Now - client.OnBusyTime).TotalMilliseconds;

                if (iOffset > 1)
                {
                    for (int i = 0; i <= iOffset; i++)
                    {
                        var clientDate = FindSummaryDate(info.DeviceName, info.DeviceId, client.OnBusyTime.AddDays(0).ToString("yyyyMMdd"));
                        clientDate.BusyTimeTotoal += (long)(DateTime.Now - client.OnBusyTime).TotalMilliseconds;
                        clientDate.OnBusyTime = DateTime.MinValue;
                    }
                }

                client.OnBusyTime = DateTime.MinValue;
            }
            //按日期统计
        }
        public void UpdateOnlineState(ClientAliveInfo info, ClientOnlineState oldState, ClientOnlineState newState)
        {
            //汇总统计
            //按日期统计
        }
        public List<ClientStateSummary> GetSummary()
        {
            return _sum;
        }
        public List<ClientStateDateSummary> GetDateSummary()
        {
            return _sumDate;
        }

        public void Reset()
        {
            _sum.Clear();
            _sumDate.Clear();
        }
        /// <summary>
        /// 客户机状态统计
        /// </summary>
        public class ClientStateSummary
        {
            public string DeviceName { get; set; }
            public string DeviceId { get; set; }
            public long BusyTimeTotoal { get; set; }
            public int BusyCount { get; set; }
            public int UserCount { get; set; }
            public long OnlineTimeTotal { get; set; }
            public DateTime OnBusyTime { get; set; }
            public DateTime OnlineTime { get; set; }

            public ClientStateSummary()
            {
                OnBusyTime = DateTime.MinValue;
                OnlineTime = DateTime.MinValue;
            }
        }
        /// <summary>
        /// 客户机状态日统计
        /// </summary>
        public class ClientStateDateSummary : ClientStateSummary
        {
            public string SummaryDate { get; set; }
        }
    }
}
