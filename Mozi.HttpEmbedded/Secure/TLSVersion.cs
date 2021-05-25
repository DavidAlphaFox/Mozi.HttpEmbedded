using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded.Secure
{
    /// <summary>
    /// 传输加密类型
    /// </summary>
    public class TLSVersion : AbsClassEnum
    {
        private string _tag = "";
        private int _code = 0;

        protected override string Tag { get { return _tag; } }
        public int Code { get { return _code; } }

        public static readonly TLSVersion SSL30 = new TLSVersion("SSL30",0);
        public static readonly TLSVersion TLS10 = new TLSVersion("TLS10",259);
        public static readonly TLSVersion TLS11 = new TLSVersion("TLS11",0);
        public static readonly TLSVersion TLS12 = new TLSVersion("TLS12",0);
        public static readonly TLSVersion TLS13 = new TLSVersion("TLS13", 0);

        public TLSVersion(string tag,int code)
        {
            _tag = tag;
            _code = code;
        }
    }
}
