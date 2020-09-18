using System;
using System.Diagnostics;
using System.IO;

namespace Mozi.HttpEmbedded.WebDav.Storage.DiskStore
{
    /// <summary>
    /// This class implements a disk-based <see cref="IWebDavStore" />.
    /// </summary>
    [DebuggerDisplay("Disk Store ({RootPath})")]
    public sealed class WebDavDiskStore : IWebDavStore
    {
        private readonly string _rootPath;

        /// <summary>
        /// Initializes a new instance of <see cref="WebDavDiskStore" /> class.
        /// </summary>
        /// <param name="rootPath">The root path of a folder on disk to host in this <see cref="WebDavDiskStore" />.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"><paramref name="rootPath" /> is <c>null</c>.</exception>
        /// <exception cref="DirectoryNotFoundException"><paramref name="rootPath" /> specifies a folder that does not exist.</exception>
        public WebDavDiskStore(string rootPath)
        {
            if (rootPath == null)
            {
                throw new ArgumentNullException(rootPath);
            }
            else
            {
                if (!Path.IsPathRooted(rootPath))
                {
                    rootPath = AppDomain.CurrentDomain.BaseDirectory + rootPath + "\\";
                }
                if (!Directory.Exists(rootPath))
                {
                    throw new DirectoryNotFoundException(rootPath);
                }
            }
            _rootPath = rootPath;
        }

        /// <summary>
        /// Gets the root path for the folder that is hosted in this <see cref="WebDavDiskStore" />.
        /// </summary>
        /// <value>
        /// The root path.
        /// </value>
        public string RootPath
        {
            get
            {
                return _rootPath;
            }
        }

        #region IWebDAVStore Members

        /// <summary>
        /// Gets the root collection of this <see cref="IWebDavStore" />.
        /// </summary>
        public IWebDavStoreCollection Root
        {
            get
            {
                return new DiskStoreCollection(null, _rootPath);
            }
        }

        #endregion
    }
}