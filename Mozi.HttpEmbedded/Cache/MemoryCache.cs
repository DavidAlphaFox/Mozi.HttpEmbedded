namespace Mozi.HttpEmbedded.Cache
{
    public class MemoryCache
    {

    }
    /// <summary>
    /// 缓存信息
    /// </summary>
    public struct CacheInfo
    {
        public string CacheName { get; set; }
        public string CacheParam { get; set; }
        public string CacheData { get; set; }
        public string CacheSize { get; set; }
        public string CacheTime { get; set; }
        public string Expire { get; set; }
        public string Owner { get; set; }
        public byte IsPrivate { get; set; }
    }
}
