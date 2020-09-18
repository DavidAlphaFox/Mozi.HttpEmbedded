using Mozi.HttpEmbedded.WebDav.MethodHandlers;
using Mozi.HttpEmbedded.WebDav.Storage;

namespace Mozi.HttpEmbedded.WebDav.Method
{
    /// <summary>
    ///  <c>DELETE</c> WebDAV��չ����
    /// </summary>
    internal class Delete : MethodHandlerBase, IMethodHandler
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
            //��Ŀ¼��Դ�嵥
            IWebDavStoreCollection collection = GetParentCollection(store, context.Request.Path);

            //��Ŀ¼��Դ�嵥
            IWebDavStoreItem item = GetItemFromCollection(collection, context.Request.Path);

            //�Ƴ���Ŀ
            collection.Delete(item);
            return StatusCode.Success;
        }
    }
}