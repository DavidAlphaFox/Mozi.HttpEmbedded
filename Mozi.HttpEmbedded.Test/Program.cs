using Mozi.HttpEmbedded.Auth;
using Mozi.HttpEmbedded.Page;
using Mozi.SSDP;
using System;

namespace Mozi.HttpEmbedded.Test
{
    static class Program
    {
        static void Main(string[] args)
        {
            HttpServer hs = new HttpServer();
            //配置端口并启动服务器
            hs.SetPort(9000).Start();

            //开启认证
            hs.UseAuth(AuthorizationType.Basic).SetUser("admin", "admin");

            //开启静态文件支持
            hs.UseStaticFiles("");

            //程序集注入
            //1,此方法会扫描程序集内继承自BaseApi或属性标记为[BasicApi]的类
            //2,Http通讯数据标准默认为xml,使用Router.Default.SetDataSerializer(ISerializer ser)更改序列化类型
            //Router.Default.Register("./test.dll");

            //路由映射
            Router router = Router.Default;
            router.Map("services/{controller}/{id}");

            //开启WebDAV
            hs.UseWebDav("dav");
            Service ser = new Service();
            ser.Active();
            Console.ReadLine();
        }
    }
}
