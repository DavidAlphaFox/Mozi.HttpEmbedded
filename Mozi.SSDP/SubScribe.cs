using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    public class SubscribeReaponsePackage
    {

    }
}
