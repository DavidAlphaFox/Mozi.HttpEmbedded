using System;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 请求上下文对象
    /// </summary>
    public class HttpContext:IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// 请求对象
        /// </summary>
        public HttpRequest Request { get; set; }
        /// <summary>
        /// 响应对象
        /// </summary>
        public HttpResponse Response { get; set; }
        /// <summary>
        /// 服务器对象
        /// </summary>
        public HttpServer Server { get; set; }

        ~HttpContext()
        {
            Dispose(disposing: false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {           

                }
                Request = null;
                Response = null;
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
