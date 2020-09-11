using System;

namespace Mozi.HttpEmbedded.Page
{
    /// <summary>
    /// API接口使用示范 
    /// </summary>
    public class Test:BaseApi
    {
        /// <summary>
        /// 无参方法
        /// </summary>
        /// <returns></returns>
        public string Hello()
        {
            return "Welcome to Mozi.HttpEnbedded";
        }
        /// <summary>
        /// 取时间
        /// </summary>
        /// <returns></returns>
        public string GetTime()
        {

        }
    }
}
