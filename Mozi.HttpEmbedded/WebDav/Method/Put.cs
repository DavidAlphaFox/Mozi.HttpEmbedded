using System.IO;
using Mozi.HttpEmbedded.Common;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.WebDav.MethodHandlers;
using Mozi.HttpEmbedded.WebDav.Storage;

namespace Mozi.HttpEmbedded.WebDav.Method
{
    /// <summary>
    ///  <c>PUT</c> WebDAV��չ����
    /// </summary>
    internal class Put : MethodHandlerBase, IMethodHandler
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
            IWebDavStoreCollection parentCollection = GetParentCollection(store, context.Request.Path);

            UrlTree ut = new UrlTree(context.Request.Path);
            string itemName = UrlEncoder.Decode(ut.Last().TrimEnd('/', '\\'));

            IWebDavStoreItem item = parentCollection.GetItemByName(itemName);
            IWebDavStoreDocument doc;
            if (item != null)
            {
                doc = item as IWebDavStoreDocument;
                if (doc == null)
                    return StatusCode.MethodNotAllowed;
            }
            else
            {
                doc = parentCollection.CreateDocument(itemName);
            }

            if (context.Request.Body.Length < 0)
                return StatusCode.LengthRequired;

            using (Stream stream = doc.OpenWriteStream(false))
            {
                stream.Write(context.Request.Body, 0, context.Request.Body.Length);
            }

            return StatusCode.Success;
        }
    }
}