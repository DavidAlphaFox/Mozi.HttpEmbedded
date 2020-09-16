using Mozi.HttpEmbedded.WebDav.Stores;

namespace Mozi.HttpEmbedded.WebDav.MethodHandlers
{
    /// <summary>
    /// </summary>
    public interface IMethodHandler
    {
        /// <summary>
        /// 响应请求
        /// </summary>
        /// <param name="server"><see cref="DavServer" /> </param>
        /// <param name="context"> 
        /// <see cref="HttpContext" /> 
        ///  </param>
        /// <param name="store"><see cref="IWebDavStore" /> <see cref="DavServer" /></param>
        StatusCode ProcessRequest(DavServer server, HttpContext context, IWebDavStore store);
    }
}