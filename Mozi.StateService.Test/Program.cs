using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mozi.StateService.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ////开启状态服务
            //HeartBeatService state = new HeartBeatService()
            //{
            //    Port = 13453,
            //    RemoteHost = "100.100.0.105"
            //};

            //state.ApplyDevice("Mozi.StateService", "80018001", "1.2.3");
            //state.SetState("alive");
            //state.Init();
            //state.Activate();

            HeartBeatGateway hg = new HeartBeatGateway();
            hg.Start(13453);
            Console.ReadLine();
        }
    }
}
