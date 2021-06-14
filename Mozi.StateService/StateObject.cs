using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Mozi.StateService
{
    /// <summary>
    /// 通讯缓冲对象
    /// </summary>
    public class StateObject:IDisposable
    {
        public string Id { get; set; }   //连接标识符
        public Socket WorkSocket = null;
        public int RemotePort = 0;
        public const int BufferSize = 1024;
        public List<byte> Data = new List<byte>();
        public byte[] Buffer = new byte[BufferSize * 4];
        private bool disposedValue;

        public string IP { get; set; }

        //TODO 此处没有完整处理包体，会有多读取的冗余数据
        public void ResetBuffer(int count)
        {
            byte[] data = new byte[count > 0 ? count : Buffer.Length];
            Array.Copy(Buffer, data, data.Length);
            Data.AddRange(data);
            Buffer = new byte[BufferSize];
        }

        ~StateObject()
        {
            Buffer = null;
            Data = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~StateObject()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
