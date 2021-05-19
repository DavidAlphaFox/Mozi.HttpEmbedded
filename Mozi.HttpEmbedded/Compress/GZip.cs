using System.IO;
using System.IO.Compression;

namespace Mozi.HttpEmbedded.Compress
{
    /// <summary>
    /// GZip压缩
    /// </summary>
    public static class GZip
    {
        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] value)
        {
            //Transform string into byte[]    
            byte[] byteArray;
            MemoryStream ms = new MemoryStream();
            GZipStream sw = new GZipStream(ms, CompressionMode.Compress);
            sw.Write(value, 0, value.Length);
            sw.Flush();
            sw.Dispose();
            byteArray = ms.ToArray();
            ms.Dispose();
            return byteArray;

        }
        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] value)
        {
            byte[] byteArray;
            MemoryStream ms = new MemoryStream(value);
            MemoryStream msOut = new MemoryStream();
            GZipStream sr = new GZipStream(ms, CompressionMode.Decompress);
            sr.CopyTo(msOut);
            sr.Dispose();
            byteArray = msOut.ToArray();
            ms.Dispose();
            msOut.Dispose();
            return byteArray;
        }
    }
}
