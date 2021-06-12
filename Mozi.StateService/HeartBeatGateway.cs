using System;
using System.Net;

namespace Mozi.StateService
{
    public class HeartBeatGateway
    {
        private UDPSocket _socket;
        private EndPoint _endPoint = new IPEndPoint(IPAddress.Any, 13453);

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
                StatePackage pg = StatePackage.Parse(args.Data);
                Console.WriteLine("设备：{0},编号：{1},版本：{2},时间：{3}", pg.DeviceName,pg.DeviceId,pg.AppVersion,pg.Timestamp);
            }
            catch
            {

            }
        }
    }
}
