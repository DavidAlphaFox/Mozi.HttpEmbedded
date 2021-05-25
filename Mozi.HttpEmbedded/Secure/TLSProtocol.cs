using System;
using System.Collections.Generic;

namespace Mozi.HttpEmbedded.Secure
{
    /// <summary>
    /// TLS协议解析
    /// </summary>
    internal class TLSProtocol
    {
        //1字节
        public byte ContentType { get; set; }
        //2字节 { major,minor }
        public ushort Version { get; set; }
        //2字节
        public ushort Length { get; set; }

        public HandShakeProtocol ShakeProtocol { get; set; }
        /// <summary>
        /// 解析协议包
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TLSProtocol Parse(byte[] data)
        {
            TLSProtocol proto = new TLSProtocol();
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
            proto.ShakeProtocol = new HandShakeProtocol();

            var arrContent = new byte[proto.Length];
            Array.Copy(data, 5, arrContent, 0, arrContent.Length);

            var shake = proto.ShakeProtocol;
            shake.HandShakeType = arrContent[0];
            var arrLength2 = new byte[3];
            Array.Copy(data, 1, arrLength2, 0, 3);
            Array.Reverse(arrLength2);
            shake.Length = BitConverter.ToUInt16(arrLength2,0);
            shake.Version = BitConverter.ToUInt16(data, 4);
            //Random
            var arrRandom = new byte[32];
            Array.Copy(arrContent, 6, arrRandom, 0,32);
            shake.Random = new RandomInfo();
            var random = shake.Random;

            var arrTime = new byte[4];
            var arrSead = new byte[28];
            Array.Copy(arrRandom, arrTime, 4);
            Array.Reverse(arrTime);
            Array.Copy(arrRandom, 0, arrSead, 0, 28);
            random.Timestamp = BitConverter.ToUInt32(arrTime, 0);
            random.Random = arrSead;

            //Seesion
            shake.SessionId = arrContent[38];
            //CipherSuites
            var arrLength3 = new byte[2];
            Array.Copy(arrContent, 39, arrLength3, 0, 2);
            Array.Reverse(arrLength3);
            shake.CipherSuitesLength = BitConverter.ToUInt16(arrLength3, 0);
            var arrCipherSuites = new byte[shake.CipherSuitesLength];
            Array.Copy(arrContent, 39+2, arrCipherSuites, 0, shake.CipherSuitesLength);
            //解析
            shake.CipherSuites = new List<CipherSuite>();
            for(int i = 0; i < shake.CipherSuitesLength / 2; i++)
            {
                var arrCipher = new byte[2];
                Array.Copy(arrCipherSuites, i * 2, arrCipher,0, 2);
                Array.Reverse(arrCipher);
                shake.CipherSuites.Add((CipherSuite)Enum.Parse(typeof(CipherSuite), BitConverter.ToUInt16(arrCipher, 0).ToString()));
            }
            //CompressionMethod
            shake.CompressionMethodsLength = arrContent[39 + 2 +shake.CipherSuitesLength];
            var arrCompressionMethod = new byte[shake.CompressionMethodsLength];
            Array.Copy(arrContent, 39 + 2+ shake.CipherSuitesLength, arrCompressionMethod, 0, shake.CompressionMethodsLength);
            shake.CompressionMethods = new List<CompressMethod>();
            for(int i = 0; i < shake.CompressionMethodsLength; i++)
            {
                shake.CompressionMethods.Add(new CompressMethod() {
                    Method = arrContent[39 + 2 + shake.CipherSuitesLength+1+i]
                }); 
            }
            //Extension
            var arrLenght4 = new byte[2];
            Array.Copy(arrContent, 39 + 2 + shake.CipherSuitesLength + shake.CompressionMethodsLength + 1, arrLenght4, 0, 2);
            Array.Reverse(arrLenght4);
            shake.ExtensionsLength = BitConverter.ToUInt16(arrLenght4, 0);
            //忽略扩展
            shake.Extension = new ExtensionInfo() { 
                ExtensionLength=1,
                ExtensionType=511
            };
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
    internal class HandShakeProtocol
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
        public byte SessionId { get; set; }
        //2字节
        public UInt16 CipherSuitesLength { get; set; }
        //每2个字节区分一个CipherSuite
        public List<CipherSuite> CipherSuites { get; set; }
        //1字节
        public byte CompressionMethodsLength { get; set; }
        public List<CompressMethod> CompressionMethods { get; set; }
        //2字节
        public UInt16 ExtensionsLength { get; set; }
        public ExtensionInfo Extension { get; set; }

     }
        //1字节
     internal class CompressMethod
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
