using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded.Secure
{
    /// <summary>
    /// 传输加密类型
    /// </summary>
    public class TransmitionCryptType : AbsClassEnum
    {
        private string _tag = "";

        protected override string Tag { get { return _tag; } }

        public static readonly TransmitionCryptType SSL30 = new TransmitionCryptType("SSL30");
        public static readonly TransmitionCryptType TLS10 = new TransmitionCryptType("TLS10");
        public static readonly TransmitionCryptType TLS11 = new TransmitionCryptType("TLS11");
        public static readonly TransmitionCryptType TLS12 = new TransmitionCryptType("TLS12");

        public TransmitionCryptType(string tag)
        {
            _tag = tag;
        }
    }
}
