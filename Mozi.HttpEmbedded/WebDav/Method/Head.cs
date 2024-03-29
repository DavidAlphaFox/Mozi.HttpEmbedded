using Mozi.HttpEmbedded.Source;
using Mozi.HttpEmbedded.WebDav.Exceptions;
using Mozi.HttpEmbedded.WebDav.Storage;

namespace Mozi.HttpEmbedded.WebDav.Method
{
    /// <summary>
    ///  <c>PROPFIND</c> WebDAV扩展方法
    /// </summary>
    internal class WebDavHeadMethodHandler : MethodHandlerBase, IMethodHandler
    {
        /// <summary>
        /// 响应请求
        /// </summary>
        /// <param name="server"><see cref="DavServer" /> </param>
        /// <param name="context"> 
        /// <see cref="HttpContext" /> 
        ///  </param>
        /// <param name="store"><see cref="IWebDavStore" /> <see cref="DavServer" /></param>
        /// <exception cref="WebDavNotFoundException"><para>
        ///   <paramref name="context" /> </para>
        /// <para>- or -</para>
        /// <para>
        ///   <paramref name="context" /> </para></exception>
        /// <exception cref="WebDavConflictException"><paramref name="context" /> </exception>
        public StatusCode ProcessRequest(DavServer server, HttpContext context, IWebDavStore store)
        {
            //父级目录资源清单
            IWebDavStoreCollection collection = GetParentCollection(store, context.Request.Path);

            //取资源清单
            IWebDavStoreItem item = GetItemFromCollection(collection, context.Request.Path);

            context.Response.AddHeader(HeaderProperty.ContentType.PropertyName, Mime.Default);
            context.Response.AddHeader(HeaderProperty.LastModified.PropertyName, item.ModificationDate.ToUniversalTime().ToString("R"));

            return StatusCode.Success;
        }
    }
}