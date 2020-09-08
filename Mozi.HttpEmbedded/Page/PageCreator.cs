using System;

namespace Mozi.HttpEmbedded.Page
{
    /// <summary>
    /// 页面生成器
    /// </summary>
    public class PageCreator
    {
        /// <summary>
        /// 注入全局数据
        /// </summary>
        /// <returns></returns>
        public PageCreator InjectGlobal()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 注入临时数据
        /// </summary>
        /// <returns></returns>
        public PageCreator InjectValues()
        {
            return null;
        }
    }
}
