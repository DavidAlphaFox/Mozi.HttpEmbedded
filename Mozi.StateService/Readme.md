#Mozi.StateService ��������

#��Ŀ����

Mozi.StateService��һ�������������������������Ҫ��Ӧ�ö���
1.HeartBeatService
    �����ͻ���
2.HeartBeatGateway
    ��������

##ʹ��˵��

~~~csharp

        static HeartBeatGateway hg = new HeartBeatGateway();

        static void Main(string[] args)
        {
            //����״̬����
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

            //��������
            hg.OnClientStateChange += Hg_OnClientStateChange;
            hg.Start(13453);
            Console.ReadLine();
        }
~~~
### By [Jason][1] on Jun. 5,2021

[1]:mailto:brotherqian@163.com