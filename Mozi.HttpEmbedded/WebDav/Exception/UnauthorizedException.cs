using System;

namespace Mozi.HttpEmbedded.WebDav.Exception
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class WebDavUnauthorizedException : WebDavException
    {
        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException">
        /// </param>
        public WebDavUnauthorizedException(string message = null, Exception innerException = null)
            : base(StatusCode.Unauthorized, message, innerException)
        {

        }
    }
}