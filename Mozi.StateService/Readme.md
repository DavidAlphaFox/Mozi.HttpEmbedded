# Mozi.StateService ��������

## ��Ŀ����

Mozi.StateService��һ�������������������UDP��������Ŀ��Ҫ�����ն˼��������������������: 

### HeartBeatService
    �����ͻ���  
    �ն˵��ô��������ʱ���������������֪ͨ��

### HeartBeatGateway
    ��������  
    �����ն�������Ϣ��������ն�����״̬����
## ʹ��˵��

~~~csharp

        static HeartBeatGateway hg = new HeartBeatGateway();

        static void Main(string[] args)
        {
            //����״̬����
            HeartBeatService state = new HeartBeatService()
            {
                Port = 13453,
                RemoteHost = $"{port}"
            };

            state.ApplyDevice("Mozi", "80018001", "1.2.4");
            state.SetState(ClientLifeState.Alive);
            state.Init();
            state.Activate();

            //�л��ն�״̬
            state.SetState(ClientLifeState.Idle);

            //������������
            hg.OnClientStateChange += Hg_OnClientStateChange;
            hg.Start(13453);
            Console.ReadLine();
        }
~~~
### By [Jason][1] on Jun. 5,2021

[1]:mailto:brotherqian@163.com