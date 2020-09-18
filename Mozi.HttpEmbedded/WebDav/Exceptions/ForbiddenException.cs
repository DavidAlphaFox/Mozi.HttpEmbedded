using System;

namespace Mozi.HttpEmbedded.WebDav.Exceptions
{
    /// <summary>
    /// 403
    /// </summary>
    public class WebDavForbiddenException : WebDavException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException">
        /// </param>
        public WebDavForbiddenException(string message = null, Exception innerException = null) : base(StatusCode.Forbidden, message, innerException)
        {

        }
    }
}
