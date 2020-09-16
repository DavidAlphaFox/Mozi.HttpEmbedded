using Mozi.HttpEmbedded.Common;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.WebDav.MethodHandlers;
using Mozi.HttpEmbedded.WebDav.Stores;

namespace Mozi.HttpEmbedded.WebDav.Method
{
    /// <summary>
    ///  <c>COPY</c> WebDAV扩展方法
    /// </summary>
    internal class Copy : WebDavMethodHandlerBase, IMethodHandler
    {
        /// <summary>
        /// 响应请求
        /// </summary>
        /// <param name="server"><see cref="DavServer" /> </param>
        /// <param name="context"> 
        /// <see cref="HttpContext" /> 
        ///  </param>
        /// <param name="store"><see cref="IWebDavStore" /> <see cref="DavServer" /></param>
        public StatusCode ProcessRequest(DavServer server, HttpContext context, IWebDavStore store)
        {
            IWebDavStoreItem source = WebDavExtensions.GetStoreItem(context.Request.Path, store);
            if (source is IWebDavStoreDocument || source is IWebDavStoreCollection)
            {
                string destinationUri = context.Request.Headers["Destination"];
                IWebDavStoreCollection destinationParentCollection = GetParentCollection(store, destinationUri);

                bool copyContent = GetDepthHeader(context.Request) != 0;
                bool isNew = true;
                UrlTree ut = new UrlTree(destinationUri);
                string destinationName = UrlEncoder.Decode(ut.Last().TrimEnd('/', '\\'));
                IWebDavStoreItem destination = destinationParentCollection.GetItemByName(destinationName);

                if (destination != null)
                {
                    if (source.ItemPath == destination.ItemPath)
                        return StatusCode.Forbidden;
                    if (!GetOverwriteHeader(context.Request))
                        return StatusCode.PreconditionFailed;
                    if (destination is IWebDavStoreCollection)
                        destinationParentCollection.Delete(destination);
                    isNew = false;
                }

                destinationParentCollection.CopyItemHere(source, destinationName, copyContent);
                return isNew ? StatusCode.Created : StatusCode.NoContent;
            }
            else
                return StatusCode.Forbidden;
        }
    }
}