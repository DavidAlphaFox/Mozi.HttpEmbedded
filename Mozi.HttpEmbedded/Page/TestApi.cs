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
        /// 取服务器当前时间
        /// </summary>
        /// <returns></returns>
        public DateTime GetTime()
        {
            return DateTime.Now;
        }
        /// <summary>
        /// 认证用户
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public ResponseMessage AuthUser(string username,string password)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <returns></returns>
        public ResponseMessage UploadFile()
        {
            throw new NotImplementedException();
        } 
    }
    /// <summary>
    /// 标准消息封装
    /// </summary>
    public class ResponseMessage
    {
        public bool   success { get; set; }
        public int    code    { get; set; }
        public string message { get; set; }
        public object data    { get; set; }
    }
}
