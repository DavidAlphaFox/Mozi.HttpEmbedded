using System;
using System.Net.Sockets;

namespace Mozi.HttpEmbedded
{
     public delegate void ServerStart(object sender, ServerArgs args);

     public delegate void AfterServerStop(object sender, ServerArgs args);

     public delegate void ClientDisConnect(object sender, ClientConnectArgs ars);

     public delegate void ClientConnect(object sender, ClientConnectArgs args);

     public delegate void ReceiveStart(object sender, DataTransferArgs args);

     public delegate void ReceiveEnd(object sender, DataTransferArgs args);
     
     public class ServerArgs:EventArgs{

         public DateTime StartTime { get; set; }
         public DateTime StopTime  { get; set; }
     }
     
     public class ClientConnectArgs:EventArgs 
     {
         private Socket _socket;

         public Socket Client 
         {
             get { return _socket; }
             set { _socket = value; }
         }

     }

     public class DataTransferArgs:EventArgs 
     {
        public byte[] Data { get; set; }
         //IPV4
        public string IP { get; set; }
        public int Port { get; set; }
        public Socket Socket { get; set; }
        public Socket Client { get; internal set; }
        public StateObject State { get; internal set; }

        ~DataTransferArgs()
        {
             Data = null;
        }
     }
}
