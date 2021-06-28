using System;

namespace Mozi.HttpEmbedded.Attributes
{
    /// <summary>
    /// WebService属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal class WebServiceAttribute:Attribute
    {
        /// <summary>
        /// 服务名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 服务命名空间
        /// </summary>
        public string Namespace { get; set; }
        /// <summary>
        /// 服务描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 允许HttpGet/HttpPost直接访问
        /// </summary>
        public bool AllowHttpAccess { get; set; }
        public WebServiceAttribute()
        {
            Name = "";
            Namespace = "http://tempurl.org";
            Description = "";
        }
    }
}
