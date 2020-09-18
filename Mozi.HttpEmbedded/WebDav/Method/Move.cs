using Mozi.HttpEmbedded.Common;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.WebDav.Storage;

namespace Mozi.HttpEmbedded.WebDav.MethodHandlers
{
    /// <summary>
    ///  <c>MOVE</c> WebDAV扩展方法
    /// </summary>
    internal class Move : MethodHandlerBase, IMethodHandler
    {
        /// <summary>
        /// 响应请求
        /// </summary>
        /// <param name="server"><see cref="DavServer" /> </param>
        /// <param name="context"> 
        ///     <see cref="HttpContext" /> 
        /// </param>
        /// <param name="store"><see cref="IWebDavStore" /> <see cref="DavServer" /></param>
        public StatusCode ProcessRequest(DavServer server, HttpContext context, IWebDavStore store)
        {
            var source = WebDavExtensions.GetStoreItem(context.Request.Path, store);

            string destinationUri = context.Request.Headers["Destination"];
            IWebDavStoreCollection destinationParentCollection = GetParentCollection(store, destinationUri);

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
                destinationParentCollection.Delete(destination);
                isNew = false;
            }

            destinationParentCollection.MoveItemHere(source, destinationName);

            return StatusCode.Success;
        }
    }
}