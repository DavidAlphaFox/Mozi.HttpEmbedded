using System;

namespace Mozi.HttpEmbedded.Template
{
    /// <summary>
    /// 模板抽象类
    /// </summary>
    internal abstract class AbsTemplate
    {
        public static AbsTemplate Load(string page)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 全局变量 静态变量 常量
        /// </summary>
        /// <returns></returns>
        private AbsTemplate ApplyGlobal()
        {
            throw new NotImplementedException();
        }

        public AbsTemplate Apply()
        {
            throw new NotImplementedException();
        }

        public AbsTemplate ApplyData()
        {
            throw new NotImplementedException();
        }

        public string GetBuffer()
        {
            throw new NotImplementedException();
        }
    }
}
