namespace Mozi.HttpEmbedded.Page
{
    /// <summary>
    /// 页面抽象类
    /// </summary>
    public abstract class AbsPage
    {
        protected HttpRequest  Request   { get; set; }

        protected HttpResponse Response  { get; set; }

        protected HttpContext  Context   { get; set; }
        /// <summary>
        /// 重定向
        /// </summary>
        /// <param name="url"></param>
        public abstract void RedirectTo(string url);

        public abstract void Get();

        public abstract void Post();
    }
}
