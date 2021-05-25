using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace Mozi.HttpEmbedded.Page
{
    //TODO 如何实现DLL热覆盖？
    /// <summary>
    /// 内置接口 API接口使用示范 
    /// </summary>
    public class Runtime : BaseApi
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
        [Description("获取服务器时间 UTC")]
        public string GetTime()
        {
            return DateTime.Now.ToUniversalTime().ToString("r");
        }

        /// <summary>
        /// 验证用户
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [Description("验证用户")]
        public ResponseMessage AuthUser(string username, string password)
        {
            ResponseMessage rm = new ResponseMessage();
            return rm;
        }
        /// <summary>
        /// 列出所有用户
        /// </summary>
        /// <returns></returns>
        public ResponseMessage GetUsers()
        {
            throw new NotImplementedException();
        }
        //TODO 此处重传同名文件有问题
        /// <summary>
        /// 上传文件 支持多文件上传
        /// </summary>
        /// <returns></returns>
        public ResponseMessage UploadFile()
        {
            ResponseMessage rm = new ResponseMessage();
            bool success = false;
            if (Context.Request.Files.Length > 0)
            {
                try
                {
                    for (int i = 0; i < Context.Request.Files.Length; i++)
                    {
                        File f = Context.Request.Files[i];
                        using (FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + f.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        {
                            fs.Write(f.FileData, 0, f.FileData.Length);
                            fs.Flush();
                            fs.Close();
                        }
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    rm.message = ex.Message;
                }
            }
            rm.success = success;
            return rm;
        }
        /// <summary>
        /// 将文件存放到指定的路径
        /// </summary>
        /// <param name="dir">路径名以“.”分割,例如dir1.dir2.dir，不要在路径名中包含不符合路径命名的特殊字符</param>
        /// <returns></returns>
        public ResponseMessage PutFile(string dir)
        {
            ResponseMessage rm = new ResponseMessage();
            bool success = false;
            if (!string.IsNullOrEmpty(dir))
            {
                dir = AppDomain.CurrentDomain.BaseDirectory + dir.Replace('.', '/');
            }
            else
            {
                dir = AppDomain.CurrentDomain.BaseDirectory;
            }
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (Context.Request.Files.Length > 0)
            {
                try
                {
                    for (int i = 0; i < Context.Request.Files.Length; i++)
                    {
                        File f = Context.Request.Files[i];
                        using (FileStream fs = new FileStream(dir + "/" + f.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        {
                            fs.Write(f.FileData, 0, f.FileData.Length);
                            fs.Flush();
                            fs.Close();
                        }
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    rm.message = ex.Message;
                }
            }
            rm.success = success;
            return rm;
        }
        /// <summary>
        /// 列出所有API
        /// </summary>
        /// <returns></returns>
        public ResponseMessage ListApi()
        {
            ResponseMessage rm = new ResponseMessage();
            List<Type> types = Router.Default.GetTypes();
            List<ApiInfo> apis = new List<ApiInfo>();
            foreach (var type in types)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var m in methods)
                {
                    ApiInfo api = new ApiInfo
                    {
                        domain = type.Namespace,
                        controller = type.Name,
                        methodname = m.Name
                    };
                    var attrs = m.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (attrs.Length > 0)
                    {
                        api.description = ((DescriptionAttribute)attrs[0]).Description;
                    }

                    ParameterInfo[] pms = m.GetParameters();
                    if (pms.Length > 0)
                    {
                        api.args = new List<ApiParam>();
                        foreach (var p in pms)
                        {
                            api.args.Add(new ApiParam()
                            {
                                paramname = p.Name,
                                paramtype = p.ParameterType.Name,
                            });
                        }
                    }
                    apis.Add(api);
                }
            }
            rm.data = apis;
            rm.success = true;
            return rm;
        }
    }
    /// <summary>
    /// 标准消息封装
    /// </summary>
    [Serializable]
    public class ResponseMessage
    {
        public bool success { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }
    /// <summary>
    /// API信息
    /// </summary>
    [Serializable]
    public class ApiInfo
    {
        public string domain { get; set; }
        public string controller { get; set; }
        public string methodname { get; set; }
        public string description { get; set; }
        public List<ApiParam> args { get; set; }
        public string returntype { get; set; }
    }
    /// <summary>
    /// API参数信息
    /// </summary>
    [Serializable]
    public class ApiParam
    {
        public string paramname { get; set; }
        public string paramtype { get; set; }
        public string constraint { get; set; }

    }
}
