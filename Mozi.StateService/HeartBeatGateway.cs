using System;

namespace Mozi.StateService
{
    /// <summary>
    /// 心跳网关服务器
    /// </summary>
    public class HeartBeatGateway
    {
        private readonly UDPSocket _socket;

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

        private void _socket_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            try
            {
                HeartBeatPackage pg = HeartBeatPackage.Parse(args.Data);
                Console.WriteLine("设备：{0},编号：{1},状态：{2} 版本：{3},时间：{4}", pg.DeviceName,pg.DeviceId,pg.StateName,pg.AppVersion,pg.Timestamp);
            }
            catch(Exception ex)
            {
                var ex2 = ex;
            }
        }
    }
}
