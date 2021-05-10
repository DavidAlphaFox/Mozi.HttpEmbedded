namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 缓存管理
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