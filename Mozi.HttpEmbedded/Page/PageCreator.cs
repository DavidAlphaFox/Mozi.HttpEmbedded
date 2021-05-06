using System;
using System.IO;

namespace Mozi.HttpEmbedded.Page
{
    //TODO 需要实现一个模板引擎或考虑通过Razor引擎提高通用性
    /// <summary>
    /// 页面生成器
    /// </summary>
    public class PageCreator
    {
        public PageCreator()
        {

        }

        public PageCreator Load(string filePath)
        {
            return this;
        }

        public PageCreator LoadFromStream(Stream stream)
        {
            return this;
        }
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
