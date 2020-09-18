namespace Mozi.HttpEmbedded.WebDav.Storage
{
    /// <summary>
    /// This interface must be implemented by classes that serve as stores of collections and
    /// documents for the 
    /// <see cref="DavServer" />.
    /// </summary>
    public interface IWebDavStore
    {
        /// <summary>
        /// Gets the root collection of this <see cref="IWebDavStore" />.
        /// </summary>
        /// <value>
        /// The root.
        /// </value>
        IWebDavStoreCollection Root
        {
            get;
        }
    }
}