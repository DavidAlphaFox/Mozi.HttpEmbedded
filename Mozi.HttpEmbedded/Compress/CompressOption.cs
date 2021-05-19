namespace Mozi.HttpEmbedded.Compress
{
    /// <summary>
    /// 压缩选项
    /// </summary>
    public class CompressOption
    {
        public ContentEncoding CompressType { get; set; }
        public int MinContentLength { get; set; }
        public int CompressLevel { get; set; }
        /// <summary>
        /// 启用压缩的MIME类型
        /// </summary>
        public string[] CompressTypes { get; set; }
        /// <summary>
        /// 是否对代理进行压缩
        /// </summary>
        public bool CompressProxied { get; set; }

        public CompressOption()
        {
            CompressTypes = new string[] { "text/html" };
        }
    }
}
