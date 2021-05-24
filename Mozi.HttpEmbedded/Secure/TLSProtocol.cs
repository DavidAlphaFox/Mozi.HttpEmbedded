using System.Collections.Generic;

namespace Mozi.HttpEmbedded.Secure
{
    /// <summary>
    /// TLS协议解析
    /// </summary>
    internal class TLSProtocol
    {
        //1字节
        int ContentType { get; set; }
        //2字节
        int Version { get; set; }
        //2字节
        int Length { get; set; }

        HandShakeProtocol ShakeProtocol { get; set; }
        /// <summary>
        /// 解析协议包
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TLSProtocol Parse(byte[] data)
        {
            TLSProtocol proto = new TLSProtocol();
            return proto;
        }
    }

    internal class HandShakeProtocol
    {
        //1字节
        int HandShakeType { get; set; }
        //3字节
        int Length { get; set; }
        //2字节
        int Version { get; set; }
        //32字节
        RandomInfo Random { get; set; }
        //1字节
        int SessionId { get; set; }
        //2字节
        int CipherSuitesLength { get; set; }
        //每2个字节区分一个CipherSuite
        List<CipherSuite> CipherSuites { get; set; }
        //1字节
        int CompressionMethodsLength { get; set; }
        //
        List<CompressMethod> CompressionMethods { get; set; }
        int ExtensionsLength { get; set; }
        ExtensionInfo Extension { get; set; }

     }
        //1字节
     internal class CompressMethod
     {
          int Method { get; set; }
     }
     internal class ExtensionInfo
     {
         int ExtensionType { get; set; }
         int ExtensionLength { get; set; }
     }
     internal class RenegotiationInfo {
         int Renegotiation { get; set; }
     }
    /// <summary>
    /// 种子随机数 定长32字节
    /// </summary>
    internal class RandomInfo
    {
        //4字节
        long Timestamp { get; set; }
        //28字节
        byte[] Random { get; set; }
    }
}
