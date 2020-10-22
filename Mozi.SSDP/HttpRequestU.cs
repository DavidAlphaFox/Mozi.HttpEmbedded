using Mozi.HttpEmbedded;

namespace Mozi.SSDP
{
    /// <summary>
    /// 请求包
    /// </summary>
    public class HttpRequestU:HttpRequest
    {
        public HttpRequestU SetPath(string path)
        {
            Path = path;
            return this;
        }

        public HttpRequestU SetMethod(RequestMethod method)
        {
            Method = method;
            return this;
        }

        public HttpRequestU SetProtocol(HttpEmbedded.HttpVersion version)
        {
            ProtocolVersion = version;
            return this;
        }

        public HttpRequestU SetHeaders(TransformHeader headers)
        {
            Headers = headers;
            return this;
        }
    }
}
