namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 请求上下文对象
    /// </summary>
    public class HttpContext
    {
        /// <summary>
        /// 请求对象
        /// </summary>
        public HttpRequest Request   { get; set; }
        /// <summary>
        /// 响应对象
        /// </summary>
        public HttpResponse Response { get; set; }
    }
}
