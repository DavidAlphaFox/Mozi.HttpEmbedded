﻿using System.IO;
using System.IO.Compression;

namespace Mozi.HttpEmbedded.Compress
{
    /// <summary>
    /// GZip压缩
    /// </summary>
    public static class GZip
    {
        public static byte[] Zip(byte[] value)
        {
            //Transform string into byte[]    
            byte[] byteArray;
 
            MemoryStream ms = new MemoryStream();
            GZipStream sw = new GZipStream(ms,CompressionMode.Compress);
 
            sw.Write(value, 0, value.Length);
            byteArray = ms.ToArray();
            sw.Dispose();
            ms.Dispose();
            return byteArray;

        }

        public static byte[] UnZip(byte[] value)
        {
            //Transform string into byte[]  
            byte[] byteArray;

            //Prepare for decompress  
            MemoryStream ms = new MemoryStream(value);
            MemoryStream msOut=new MemoryStream();
            GZipStream sr = new GZipStream(ms,
            CompressionMode.Decompress);
            sr.CopyTo(msOut);
            byteArray = msOut.ToArray();
            sr.Dispose();
            ms.Dispose();
            msOut.Dispose();
            return byteArray;
        }
    }
}