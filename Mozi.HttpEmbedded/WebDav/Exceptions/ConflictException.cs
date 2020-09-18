using System;

namespace Mozi.HttpEmbedded.WebDav.Exceptions
{
    /// <summary>
    /// 409
    /// </summary>
    [Serializable]
    public class WebDavConflictException : WebDavException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException">
        /// <see cref="Exception" /> 
        /// </param>
        public WebDavConflictException(string message = null, Exception innerException = null) : base(StatusCode.Conflict, message, innerException)
        {

        }
    }
}