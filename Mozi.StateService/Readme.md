#Mozi.StateService 心跳服务

#项目介绍

Mozi.StateService是一个心跳服务组件，包含两个主要的应用对象
1.HeartBeatService
    心跳客户端
2.HeartBeatGateway
    心跳网关

##使用说明

~~~csharp

        static HeartBeatGateway hg = new HeartBeatGateway();

        static void Main(string[] args)
        {
            //开启状态服务
            HeartBeatService state = new HeartBeatService()
            {
                Port = 13453,
                RemoteHost = "100.100.0.111"
            };

            state.ApplyDevice("Mozi", "80018001", "1.2.3");
            state.SetState(ClientLifeState.Alive);
            state.Init();
            state.Activate();
            state.SetState(ClientLifeState.Idle);

            //服务网关
            hg.OnClientStateChange += Hg_OnClientStateChange;
            hg.Start(13453);
            Console.ReadLine();
        }
~~~
### By [Jason][1] on Jun. 5,2021

[1]:mailto:brotherqian@163.com