﻿using System;
using Mozi.HttpEmbedded.Auth;
using Mozi.HttpEmbedded.Page;

namespace Mozi.HttpEmbedded
{
    static class Program
    {
        /// <summary>
        /// 入口点
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            HttpServer hs = new HttpServer();
            //配置端口并启动服务器
            hs.SetPort(9000).Start();
            //开启认证
            hs.UseAuth(AuthorizationType.Basic).SetUser("admin", "admin");
            //开启静态文件支持
            hs.UseStaticFiles("");
            //路由映射
            Router router = Router.Default;
            router.Map("services/{controller}/{id}");
            Console.ReadLine();
        }
    }
}