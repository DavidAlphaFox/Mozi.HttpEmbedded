using Mozi.HttpEmbedded.Common;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.WebDav.Storage;

namespace Mozi.HttpEmbedded.WebDav.Method
{
    /// <summary>
    ///  <c>COPY</c> WebDAV��չ����
    /// </summary>
    internal class Copy : MethodHandlerBase, IMethodHandler
    {
        /// <summary>
        /// ��Ӧ����
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
                string destPath = context.Request.Headers["destItem"];
                IWebDavStoreCollection destParentCollection = GetParentCollection(store, destPath);

                bool copyContent = GetDepthHeader(context.Request) != 0;
                bool isNew = true;
                UrlTree ut = new UrlTree(destPath);
                string destName = UrlEncoder.Decode(ut.Last().TrimEnd('/', '\\'));
                IWebDavStoreItem destItem = destParentCollection.GetItemByName(destName);

                if (destItem != null)
                {
                    if (source.ItemPath == destItem.ItemPath)
                        return StatusCode.Forbidden;
                    if (!GetOverwriteHeader(context.Request))
                        return StatusCode.PreconditionFailed;
                    if (destItem is IWebDavStoreCollection)
                        destParentCollection.Delete(destItem);
                    isNew = false;
                }

                destParentCollection.CopyItemHere(source, destName, copyContent);
                return isNew ? StatusCode.Created : StatusCode.NoContent;
            }
            else
                return StatusCode.Forbidden;
        }
    }
}