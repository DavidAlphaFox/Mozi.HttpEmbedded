using Mozi.HttpEmbedded.Source;
using System;

namespace Mozi.HttpEmbedded.WebDav.Storage.Base
{
    /// <summary>
    /// This is the base class for <see cref="IWebDavStoreItem" /> implementations.
    /// </summary>
    public class WebDavStoreDocumentBase : WebDavStoreItemBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="WebDavStoreItemBase" /> class.
        /// </summary>
        /// <param name="parentCollection">The parent <see cref="IWebDavStoreCollection" /> that contains this <see cref="IWebDavStoreItem" /> implementation.</param>
        /// <param name="name">The name of this <see cref="IWebDavStoreItem" /></param>
        /// <exception cref="ArgumentNullException"><paramref name="name" /> is <c>null</c>.</exception>
        protected WebDavStoreDocumentBase(IWebDavStoreCollection parentCollection, string name) : base(parentCollection, name)
        {
        }

        #region IWebDAVStoreItem Members

        /// <summary>
        /// Gets or sets the mime type of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        /// <value>
        /// The type of the MIME.
        /// </value>
        public string MimeType
        {
            get
            {
                //TODO 此处需要跟踪
                return Mime.GetContentType(Name);
            }
        }

        #endregion
    }
}