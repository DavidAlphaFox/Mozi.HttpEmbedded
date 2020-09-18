using System;
using System.IO;
using System.Security.Principal;
using System.Threading;
using Mozi.HttpEmbedded.WebDav.Storage.Base;

namespace Mozi.HttpEmbedded.WebDav.Storage.DiskStore
{
    /// <summary>
    /// This class implements a disk-based 
    /// <see cref="IWebDavStoreItem" /> which can be either
    /// a folder on disk (
    /// <see cref="DiskStoreCollection" />) or a file on disk
    /// (
    /// <see cref="DiskStoreDocument" />).
    /// </summary>
    public class DiskStoreItem : WebDavStoreItemBase
    {
        /// <summary>
        /// Gets the Identity of the person logged on via HTTP Request.
        /// </summary>
        protected readonly WindowsIdentity Identity;
        private readonly DiskStoreCollection _parentCollection;
        private readonly string _path;

        /// <summary>
        /// Initializes a new instance of <see cref="DiskStoreItem" /> class.
        /// </summary>
        /// <param name="parentCollection">The parent 
        /// <see cref="DiskStoreCollection" /> that contains this 
        /// <see cref="DiskStoreItem" />;
        /// or 
        /// if this is the root 
        /// <see cref="DiskStoreCollection" />.</param>
        /// <param name="path">The path that this <see cref="DiskStoreItem" /> maps to.</param>
        /// <exception cref="ArgumentNullException">path</exception>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is or empty.</exception>
        protected DiskStoreItem(DiskStoreCollection parentCollection, string path) : base(parentCollection, path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            _parentCollection = parentCollection;
            _path = path;
            Identity = (WindowsIdentity)Thread.GetData(Thread.GetNamedDataSlot(DavServer.HttpUser));
        }

        /// <summary>
        /// Gets the path to this <see cref="DiskStoreItem" />.
        /// </summary>
        public override string ItemPath
        {
            get
            {
                return _path;
            }
        }

        #region IWebDAVStoreItem Members

        /// <summary>
        /// Gets or sets the name of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to rename item</exception>
        public new string Name
        {
            get
            {
                return Path.GetFileName(_path);
            }

            set
            {
                throw new InvalidOperationException("Unable to rename item");
            }
        }
        public string Ext
        {
            get
            {
                return Path.GetExtension(_path);
            }
        }
        /// <summary>
        /// Gets if this <see cref="IWebDavStoreItem" /> is a collection.
        /// </summary>
        public new bool IsCollection
        {
            get
            {
                //取资源的属性清单
                FileAttributes attr = System.IO.File.GetAttributes(_path);

                //区分文档和目录
                return (attr & FileAttributes.Directory) == FileAttributes.Directory;
            }
        }

        /// <summary>
        /// Gets the creation date of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        public override DateTime CreationDate
        {
            get
            {

                return System.IO.File.GetCreationTime(_path);
            }
        }

        /// <summary>
        /// Gets the modification date of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        public override DateTime ModificationDate
        {
            get
            {

                return System.IO.File.GetLastWriteTime(_path);
            }
        }

        /// <summary>
        /// Gets the hidden state of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        /// <remarks>
        /// Source: <see href="http://stackoverflow.com/questions/3612035/c-sharp-check-if-a-directory-is-hidden" />
        /// </remarks>
        public new int Hidden
        {
            get
            {
                DirectoryInfo dir = new DirectoryInfo(_path);
                return (dir.Attributes & FileAttributes.Hidden) != 0 ? 1 : 0;
            }
        }

        #endregion
    }
}