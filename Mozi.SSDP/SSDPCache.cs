namespace Mozi.SSDP
{
    /// <summary>
    /// 缓存
    /// </summary>
    public class SSDPCache
    {
        public string USN { get; set; }
        public string ServiceType { get; set; }
        public int Expiration { get; set; }
        public string Location { get; set; }
    }
}
