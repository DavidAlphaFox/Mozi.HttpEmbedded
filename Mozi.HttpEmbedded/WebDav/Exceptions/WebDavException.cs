using System;

namespace Mozi.HttpEmbedded.WebDav.Exceptions
{
    /// <summary>
    /// </summary>
    [Serializable]
    public class WebDavException : Exception
    {
        private StatusCode _status;
        private string message;

        public override string Message { get; }

        public WebDavException(StatusCode status,string message,Exception innerException) 
        {
            _status = status;
            Message = message;
        }
        public StatusCode Status
        {
            get { return _status; }
        }
    }
}