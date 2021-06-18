using Mozi.HttpEmbedded.Generic;

namespace Mozi.SSDP
{
    /// <summary>
    /// SSDP消息类型
    /// </summary>
    public class SSDPType : AbsClassEnum
    {
        public static SSDPType Discover = new SSDPType("discover");
        public static SSDPType All = new SSDPType("all");
        public static SSDPType Alive = new SSDPType("alive");
        public static SSDPType Byebye = new SSDPType("byebye");

        private readonly string _name;

        //discover all alive byebye
        public SSDPType(string name)
        {
            _name = name;
        }
        public override string ToString()
        {
            return $"ssdp:{_name}";
        }
        protected override string Tag => _name;
    }
}
