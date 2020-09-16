using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 协议类型
    /// </summary>
    public class ProtocolType : AbsClassEnum
    {
        public static  ProtocolType HTTP  = new ProtocolType("HTTP");
        public static  ProtocolType HTTPS = new ProtocolType("HTTPS");

        private string _proxy;

        public string Proxy
        {
            get { return _proxy; }
            set { _proxy = value; }
        }

        protected override string Tag { get { return Proxy; } }

        private ProtocolType(string typeName)
        {
            _proxy = typeName;
        }
    }
}