using System;
using Mozi.HttpEmbedded.WebDav.Storage;
using Mozi.HttpEmbedded.WebDav.Exceptions;

namespace Mozi.HttpEmbedded.WebDav
{
    /// <summary>
    /// This class holds extension methods for various types related to WebDAV#.
    /// </summary>
    internal static class WebDavExtensions
    {
        /// <summary>
        /// Gets the Uri to the parent object.
        /// </summary>
        /// <param name="uri"><see cref="Uri" /> of a resource, for which the parent Uri should be retrieved.</param>
        /// <returns>
        /// The parent <see cref="Uri" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">uri</exception>
        /// <exception cref="InvalidOperationException">Cannot get parent of root</exception>
        /// <exception cref="ArgumentNullException"><paramref name="uri" /> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="uri" /> has no parent, it refers to a root resource.</exception>
        public static Uri GetParentUri(this Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            if (uri.Segments.Length == 1)
                throw new InvalidOperationException("Cannot get parent of root");

            string url = uri.ToString();
            int index = url.Length - 1;
            if (url[index] == '/')
                index--;
            while (url[index] != '/')
                index--;
            return new Uri(url.Substring(0, index + 1));
        }
        /// <summary>
        /// Retrieves a store item through the specified
        /// <see cref="Uri" /> from the
        /// specified
        /// <see cref="DavServer" /> and
        /// <see cref="IWebDavStore" />.
        /// </summary>
        /// <param name="uri"><see cref="Uri" /> to retrieve the store item for.</param>
        /// <param name="server"><see cref="DavServer" /> that hosts the <paramref name="store" />.</param>
        /// <param name="store"><see cref="IWebDavStore" /> from which to retrieve the store item.</param>
        /// <returns>
        /// The retrieved store item.
        /// </returns>
        /// <exception cref="ArgumentNullException"><para>
        ///   <paramref name="uri" /> is <c>null</c>.</para>
        /// <para>
        ///   <paramref name="server" /> is <c>null</c>.</para>
        /// <para>
        ///   <paramref name="store" /> is <c>null</c>.</para></exception>
        /// <exception cref="WebDavNotFoundException">If the item was not found.</exception>
        /// <exception cref="WebDavConflictException"><paramref name="uri" /> refers to a document in a collection, where the collection does not exist.</exception>
        /// <exception cref="WebDavNotFoundException"><paramref name="uri" /> refers to a document that does not exist.</exception>
        public static IWebDavStoreItem GetStoreItem(string path, IWebDavStore store)
        {
            IWebDavStoreCollection collection = store.Root;
            IWebDavStoreItem item = null;
            //ÅÐ¶Ï¸ùÂ·¾¶
            item = collection.GetItemByName(store.Root.ItemPath + "\\" + path);
            return item;
        }
    }
}