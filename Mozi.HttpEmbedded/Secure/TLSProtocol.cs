using System;
using System.Collections.Generic;

namespace Mozi.HttpEmbedded.Secure
{

    internal abstract class TLSPackage
    {
        //1字节
        public byte ContentType { get; set; }
        //2字节 { major,minor }
        public ushort Version { get; set; }
        //2字节
        public ushort Length { get; set; }

    }
    internal class HelloPackage : TLSPackage
    {
        public ShakePackage ShakeProtocol { get; set; }
    }
    /// <summary>
    /// Server Hello包
    /// <para>
    ///     Server Hello
    //      Certificate
    //      Server Key Exchange
    //      Server Hello Done
    /// </para>
    /// </summary>
    internal class ServerHelloPackage : TLSPackage
    {

    }
    internal class CertificatePackage : TLSPackage
    {

    }
    internal class ServerKeyExchangePackage : TLSPackage
    {

    }
    internal class SeverHellDonePackage : TLSPackage
    {

    }
    internal class ApplicationData
    {

    }
    internal class ApplicationDataPackage : TLSPackage
    {

    }
    internal class EncryptedAlertPackage:TLSPackage
    {

    }
    internal class NewSessionPackage : TLSPackage
    {

    }

    internal class ChangeCipherSpecPackage : TLSPackage
    {

    }

    internal class EncryptedHandShakePackage : TLSPackage
    {

    }
    /// <summary>
    /// TLS协议解析
    /// </summary>
    internal class TLSProtocol:TLSPackage
    {

        public static object Parse(byte data)
        {
            object result=null;
            //循环提取消息
            return result;
        }
        /// <summary>
        /// 解析协议包
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static HelloPackage ParseClientHello(byte[] data)
        {
            HelloPackage proto = new HelloPackage();
            //ContentType
            proto.ContentType = data[0];
            //Version
            proto.Version = BitConverter.ToUInt16(data, 1);
            //Protocol Content Length
            var arrLength1 = new byte[2];
            Array.Copy(data, 3, arrLength1, 0,2);
            Array.Reverse(arrLength1);
            proto.Length = BitConverter.ToUInt16(arrLength1,0);
            //ShakeProtocol
            proto.ShakeProtocol = new ClientHandShakePackage();

            var blockContent = new byte[proto.Length];
            Array.Copy(data, 5, blockContent, 0, blockContent.Length);

            var shake = (ClientHandShakePackage)proto.ShakeProtocol;
            shake.HandShakeType = blockContent[0];
            var arrLength2 = new byte[3];
            Array.Copy(data, 1, arrLength2, 0, 3);
            Array.Reverse(arrLength2);
            shake.Length = BitConverter.ToUInt16(arrLength2,0);
            shake.Version = BitConverter.ToUInt16(data, 4);
            //Random
            var blockRandom = new byte[32];
            Array.Copy(blockContent, 6, blockRandom, 0,32);
            shake.Random = new RandomInfo();
            var random = shake.Random;

            var blockTime = new byte[4];
            var blockSead = new byte[28];
            Array.Copy(blockRandom, blockTime, 4);
            Array.Reverse(blockTime);
            Array.Copy(blockRandom, 0, blockSead, 0, 28);
            random.Timestamp = BitConverter.ToUInt32(blockTime, 0);
            random.Random = blockSead;

            //Seesion
            shake.SessionIdLength = blockContent[38];
            if (shake.SessionIdLength > 0)
            {
                var arrSession = new byte[shake.SessionIdLength];
                Array.Copy(blockContent, 39, arrSession, 0, shake.SessionIdLength);
            }
            //CipherSuites
            var arrLength3 = new byte[2];
            Array.Copy(blockContent, 39+shake.SessionIdLength, arrLength3, 0, 2);
            Array.Reverse(arrLength3);
            shake.CipherSuitesLength = BitConverter.ToUInt16(arrLength3, 0);
            var blockCipherSuites = new byte[shake.CipherSuitesLength];
            Array.Copy(blockContent, 39 + shake.SessionIdLength + 2, blockCipherSuites, 0, shake.CipherSuitesLength);
            //解析
            shake.CipherSuites = new List<CipherSuiteType>();
            for(int i = 0; i < shake.CipherSuitesLength / 2; i++)
            {
                var arrCipher = new byte[2];
                Array.Copy(blockCipherSuites, i * 2, arrCipher,0, 2);
                Array.Reverse(arrCipher);
                shake.CipherSuites.Add((CipherSuiteType)Enum.Parse(typeof(CipherSuiteType), BitConverter.ToUInt16(arrCipher, 0).ToString()));
            }
            //CompressionMethod
            shake.CompressionMethodsLength = blockContent[39 + shake.SessionIdLength + 2 +shake.CipherSuitesLength];
            var blockCompressionMethod = new byte[shake.CompressionMethodsLength];
            Array.Copy(blockContent, 39 + shake.SessionIdLength + 2+ shake.CipherSuitesLength, blockCompressionMethod, 0, shake.CompressionMethodsLength);
            shake.CompressionMethods = new List<CompressMethodInfo>();
            for(int i = 0; i < shake.CompressionMethodsLength; i++)
            {
                shake.CompressionMethods.Add(new CompressMethodInfo() {
                    Method = blockContent[39 + shake.SessionIdLength + 2 + shake.CipherSuitesLength+1+i]
                }); 
            }
            //Extension
            var arrLenght4 = new byte[2];
            Array.Copy(blockContent, 39 + shake.SessionIdLength + 2 + shake.CipherSuitesLength + shake.CompressionMethodsLength + 1, arrLenght4, 0, 2);
            Array.Reverse(arrLenght4);
            shake.ExtensionsLength = BitConverter.ToUInt16(arrLenght4, 0);
            //忽略扩展
            shake.Extensions.Add(new ExtensionInfo() { 
                ExtensionLength=1,
                ExtensionType=511
            });
            return proto;
        }

        public static TLSProtocol ParseClientKeyExchange(byte[] data)
        {
            throw new NotImplementedException();
        }

        public static TLSProtocol ParseApplicationData(byte[] data)
        {
            throw new NotImplementedException();
        }
        public static byte[] PackSessionTicket()
        {
            throw new NotImplementedException();
        }
        //ServerHello
        public static byte[] PackServerHello()
        {
            List<byte> bytes = new List<byte>();
            //Server Hello
            //Certificate
            //Server Key Exchange
            //Server Hello Done
            return bytes.ToArray();
        }

       
    }
    internal enum TLSContentType
    {
         CHANGE_CIPHER_SPEC=20,
         ALERT=21,
         HANDSHAKE=22,
         APPLICATION_DATA=23
    }
    internal enum HandShakeType
    {
        ClientHello=1,
        ServerHello=2,
        NewSessionTicket=4,
        Certificate=11,
        ServerKeyExchange=12,
        ServerHelloDone=14,
        ClientKeyExchange=16
    }
    internal abstract class ShakePackage
    {
        //1字节
        public byte HandShakeType { get; set; }
        //3字节 uint24
        public UInt32 Length { get; set; }
        //2字节
        public UInt16 Version { get; set; }
        //32字节
        public RandomInfo Random { get; set; }
        //1字节
        public byte SessionIdLength { get; set; }
        public string SessionId { get; set; }

    }
    internal class ClientHandShakePackage:ShakePackage
    {
        //2字节
        public UInt16 CipherSuitesLength { get; set; }
        //每2个字节区分一个CipherSuite
        public List<CipherSuiteType> CipherSuites { get; set; }
        //1字节
        public byte CompressionMethodsLength { get; set; }
        //2字节一组
        public List<CompressMethodInfo> CompressionMethods { get; set; }
        //2字节
        public UInt16 ExtensionsLength { get; set; }
        public List<ExtensionInfo> Extensions { get; set; }

     }
     internal class ServerHandShakePackage : ShakePackage
     {
        public CipherSuiteType CipherSuite { get; set; }
        public CompressMethodInfo CompressMethod { get; set; }
        //2字节
        public UInt16 ExtensionsLength { get; set; }

        public List<ExtensionInfo> Extensions { get; set; }

    }

        //1字节
     internal class CompressMethodInfo
     {
        public byte Method { get; set; }
     }
     internal class ExtensionInfo
     {
        //2字节
        public UInt16 ExtensionType { get; set; }
        //2字节
        public UInt16 ExtensionLength { get; set; }
     }
     internal class RenegotiationInfo {
        public byte Renegotiation { get; set; }
     }
    /// <summary>
    /// 种子随机数 定长32字节
    /// </summary>
    internal class RandomInfo
    {
        //4字节
        public long Timestamp { get; set; }
        //28字节
        public byte[] Random { get; set; }
    }

    internal enum AlertLevel { 
        warning=1, 
        fatal=2 
    }
    internal enum AlertDescription
    {
        close_notify=0,
        unexpected_message=10,
        bad_record_mac=20,
        decryption_failed=21,
        record_overflow=22,
        decompression_failure=30,
        handshake_failure=40,
        bad_certificate=42,
        unsupported_certificate=43,
        certificate_revoked=44,
        certificate_expired=45,
        certificate_unknown=46,
        illegal_parameter=47,
        unknown_ca=48,
        access_denied=49,
        decode_error=50,
        decrypt_error=51,
        export_restriction=60,
        protocol_version=70,
        insufficient_security=71,
        internal_error=80,
        user_canceled=90,
        no_renegotiation=100
     }
    struct Alert {
        AlertLevel level;
        AlertDescription description;
    }

    //扩展类型
    enum ExtensionType
    {

    }
}
