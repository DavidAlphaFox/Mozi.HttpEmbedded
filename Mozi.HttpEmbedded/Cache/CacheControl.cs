namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 缓存管理
    /// 服务端用ETag标识缓存资源
    /// 客户机利用If-None-Match请求缓存
    /// </summary>
    public class CacheControl
    {
        private static CacheControl _control;

        public CacheControl Instance
        {
            get { return _control ?? (_control = new CacheControl()); }
        }

        private CacheControl()
        {

        }
    }
}