using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using Mozi.HttpEmbedded.Source;
using Mozi.HttpEmbedded.WebDav.Exceptions;
using Mozi.HttpEmbedded.WebDav.Utilities;

namespace Mozi.HttpEmbedded.WebDav.Stores.DiskStore
{
    /// <summary>
    /// This class implements a disk-based <see cref="WebDavDiskStoreDocument" /> mapped to a file.
    /// </summary>
    [DebuggerDisplay("File ({Name})")]
    public sealed class WebDavDiskStoreDocument : WebDavDiskStoreItem, IWebDavStoreDocument
    {
        /// <summary>
        /// Initializes a new instance of <see cref="WebDavDiskStoreDocument" /> class.
        /// </summary>
        /// <param name="parentCollection">The parent 
        /// <see cref="WebDavDiskStoreCollection" /> that contains this 
        /// <see cref="WebDavDiskStoreItem" />;
        /// or 
        /// if this is the root 
        /// <see cref="WebDavDiskStoreCollection" />.</param>
        /// <param name="path">The path that this <see cref="WebDavDiskStoreItem" /> maps to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is or empty.</exception>
        public WebDavDiskStoreDocument(WebDavDiskStoreCollection parentCollection, string path)
            : base(parentCollection, path)
        {
            
        }

        #region IWebDAVStoreDocument Members

        /// <summary>
        /// Gets the size of the document in bytes.
        /// </summary>
        public long Size
        {
            get { return new FileInfo(ItemPath).Length; }
        }

        /// <summary>
        /// Gets the mime type of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        public string MimeType
        {
            get
            {
                return Mime.GetContentType(Ext);
            }
        }

        /// <summary>
        /// Gets the etag of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        public string Etag
        {
            get
            {
                return Md5Util.Md5HashStringForUtf8String(ItemPath + ModificationDate + Hidden + Size);
            }
        }


        #endregion

        #region IWebDAVStoreDocument Members

        /// <summary>
        /// Opens a <see cref="Stream" /> object for the document, in read-only mode.
        /// </summary>
        /// <returns>
        /// <see cref="Stream" /> object that can be read from.
        /// </returns>
        /// <exception cref="WebDavUnauthorizedException">If the user is unauthorized or has no access</exception>
        public Stream OpenReadStream()
        {
            Stream stream = null;
            try
            {
                WindowsImpersonationContext wic = Identity.Impersonate();
                stream = new FileStream(ItemPath, FileMode.Open, FileAccess.Read, FileShare.None);
                wic.Undo();
            }
            catch
            {
                throw new WebDavUnauthorizedException();
            }
            return stream;
        }

        /// <summary>
        /// Opens a <see cref="Stream" /> object for the document, in write-only mode.
        /// </summary>
        /// <param name="append">A value indicating whether to append to the existing document;
        /// if 
        /// <c>false</c>, the existing content will be dropped.</param>
        /// <returns>
        /// <see cref="Stream" /> object that can be written to.
        /// </returns>
        /// <exception cref="WebDavUnauthorizedException">If the user is unauthorized or has no access</exception>
        public Stream OpenWriteStream(bool append)
        {
            if (append)
            {
                FileStream result = new FileStream(ItemPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                result.Seek(0, SeekOrigin.End);
                return result;
            }

            Stream stream = null;
            try
            {
               
                WindowsImpersonationContext wic = Identity.Impersonate();
                stream = new FileStream(ItemPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                wic.Undo();
            }
            catch
            {
                throw new WebDavUnauthorizedException();
            }
            return stream;
        }

        #endregion
    }
}