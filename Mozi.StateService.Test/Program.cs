﻿using System;
using System.Collections.Generic;

namespace Mozi.StateService.Test
{
    class Program
    {
        static readonly HeartBeatGateway hg = new HeartBeatGateway();

        static void Main(string[] args)
        {

            ////开启状态服务
            //HeartBeatService state = new HeartBeatService()
            //{
            //    Port = 13453,
            //    RemoteHost = $"{path}"
            //};

            //state.ApplyDevice("Mozi", "80018001", "1.2.3");
            //state.SetState(ClientLifeState.Alive);
            //state.Init();
            //state.Activate();
            //state.SetState(ClientLifeState.Idle);

            //服务网关
            hg.OnClientOnlineStateChange += Hg_OnClientStateChange;
            hg.OnClientMessageReceive += Hg_OnClientMessageReceive;
            hg.OnClientUserChange += Hg_OnClientUserChange;
            hg.OnClientJoin += Hg_OnClientJoin;
            hg.OnClientLifeStateChange += Hg_OnClientLifeStateChange;
            hg.Start(13453);
            
            Console.ReadLine();
        }
        /// <summary>
        /// 终端心跳状态变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="clientInfo"></param>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        private static void Hg_OnClientLifeStateChange(object sender, ClientAliveInfo clientInfo, ClientLifeState oldState, ClientLifeState newState)
        {
            
        }

        /// <summary>
        /// 新增终端事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="clientInfo"></param>
        private static void Hg_OnClientJoin(object sender, ClientAliveInfo clientInfo)
        {
           
        }
        /// <summary>
        /// 终端登录用户变更事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="client"></param>
        /// <param name="oldUser"></param>
        /// <param name="newUser"></param>
        private static void Hg_OnClientUserChange(object sender, ClientAliveInfo client, string oldUser, string newUser)
        {
            
        }
        /// <summary>
        /// 终端消息接收事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="client"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        private static void Hg_OnClientMessageReceive(object sender, ClientAliveInfo client,string host,int port)
        {
            Console.WriteLine("{4:MMdd HH:mm:ss}|N:{0},ID:{1},S:{2},V:{3},{5},{6}", client.DeviceName, client.DeviceId, client.State, client.AppVersion, client.BeatTime, host,client.UserName);

            //对终端累计忙时进行统计，此处需要终端配合进行[闲/忙]切换

        }

        private static void Hg_OnClientStateChange(object sender, ClientAliveInfo clientInfo, ClientOnlineState oldState, ClientOnlineState newState)
        {
            Console.Title = hg.Clients.Count.ToString();
        }
    }
}
