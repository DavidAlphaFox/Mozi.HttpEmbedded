using System.Collections.Generic;

namespace Mozi.SSDP
{
    /// <summary>
    /// 缓存管理器
    /// </summary>
    public class SSDPCacheManager
    {
        public static SSDPCacheManager _cm;
        /// <summary>
        /// 单实例
        /// </summary>
        public static SSDPCacheManager Instance
        {
            get { return _cm ?? (_cm = new SSDPCacheManager()); }
        }

        private List<SSDPCache> _caches = new List<SSDPCache>();

        private SSDPCacheManager()
        {

        }
    }
}
