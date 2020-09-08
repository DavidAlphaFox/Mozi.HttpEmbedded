namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 请求上下文对象
    /// </summary>
    public class HttpContext
    {
        public HttpRequest Request   { get; set; }
        public HttpResponse Response { get; set; }
    }
}
