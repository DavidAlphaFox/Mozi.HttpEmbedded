using System;

namespace Mozi.SSDP
{
    public class Subscribe:AbsAdvertisePackage
    {
        public string UserAgent { get; set; }
        public TargetDesc NT { get; set; }
        public TargetDesc SID { get; set; }
        public int Timeout { get; set; }
        public string StateVar { get; set; }
        /// <summary>
        /// 回调地址
        /// </summary>
        public string Callback { get; set; }
    }

    public class SubscribeResponsePackage
    {
        public DateTime Date { get; set; }
        public string Server { get; set; }
        public TargetDesc SID { get; set; }
        public int TIMEOUT { get; set; }
        public string AcceptedStateVar { get; set; }
    }
}
