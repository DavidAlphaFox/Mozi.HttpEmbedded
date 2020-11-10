using System;

namespace Mozi.HttpEmbedded.Page
{
    //TODO 需要实现一个模板引擎或考虑通过Razor引擎提高通用性
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
