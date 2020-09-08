using System;
using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// HTTP协议版本
    /// </summary>
    public class HttpVersion : AbsClassEnum
    {
        [Obsolete("实现HTTP/0.9没有意义")]
        public static readonly HttpVersion Version09 = new HttpVersion("0.9");
        public static readonly HttpVersion Version11 = new HttpVersion("1.1");
        public static readonly HttpVersion Version12 = new HttpVersion("1.2");
        public static readonly HttpVersion Version20 = new HttpVersion("2.0");

        public String Version { get { return _vervalue; } }

        protected override String Tag { get { return Version; } }

        private string _vervalue = "";

        private HttpVersion(string vervalue)
        {
            _vervalue = vervalue;
        }
        /// <summary>
        /// 重写ToString方法
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return String.Format("HTTP/{0}", _vervalue);
        }
    }
}