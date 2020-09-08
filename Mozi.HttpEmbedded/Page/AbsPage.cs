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

        public abstract void Get();

        public abstract void Post();
    }
}
