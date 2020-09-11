using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// TCP缓冲对象
    /// </summary>
    class StateObject
    {
        public string Id         {get;set;}   //连接标识符
        public Socket WorkSocket = null;
        public int    RemotePort = 0;
        public static int BufferSize = 1024;
        public List<byte> Data=new List<byte>(); 
        public byte[] Buffer = new byte[BufferSize*2];
        public StringBuilder sb = new StringBuilder();
        public string IP { get; set; }

        //TODO 此处没有完整处理包体，会有多读取的冗余数据
        public void ResetBuffer(int count) 
        {
            byte[] data=new byte[count>0?count:Buffer.Length];
            Array.Copy(Buffer,data,data.Length);
            Data.AddRange(data);
            Buffer = new byte[BufferSize];
        }

        ~StateObject()
        {
            Buffer = null;
        }
    }
}
