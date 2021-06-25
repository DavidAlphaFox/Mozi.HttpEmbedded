using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mozi.HttpEmbedded.Attributes
{
    /// <summary>
    /// 响应内容 文档类型
    /// </summary>
    [AttributeUsage(AttributeTargets.ReturnValue)]
    internal class ResponseContentTypeAttribute : Attribute
    {
        public string ContentType { get; set; }
        public string Encoding { get; set; }
        public ResponseContentTypeAttribute(string contentType,string encoding)
        {
            ContentType = contentType;
            Encoding = encoding;
        }
        public ResponseContentTypeAttribute(string contentType):this(contentType,"")
        {
            
        }
    }
}
