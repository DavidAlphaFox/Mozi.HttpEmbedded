using System;
using System.IO;

namespace Mozi.HttpEmbedded.Page
{
    //TODO 如何实现DLL热覆盖？
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
            return "Welcome to Mozi.HttpEmbedded";
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
        //TODO 此处重传同名文件有问题
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <returns></returns>
        public ResponseMessage UploadFile()
        {
            if (Context.Request.Files.Length > 0)
            {
                for(int i = 0; i < Context.Request.Files.Length; i++)
                {
                    File f = Context.Request.Files[i];
                    FileInfo fi = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + f.FileName);

                    using (FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + f.FileName, FileMode.OpenOrCreate,FileAccess.ReadWrite))
                    {
                        fs.Write(f.FileData, 0, f.FileData.Length);
                        fs.Flush();
                        fs.Close();
                    }
                }
            }
            ResponseMessage rm = new ResponseMessage() { success = true };
            return rm;
        }
        /// <summary>
        /// GET
        /// </summary>
        /// <returns></returns>
        public ResponseMessage Get()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// POST
        /// </summary>
        /// <returns></returns>
        public ResponseMessage Post()
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
