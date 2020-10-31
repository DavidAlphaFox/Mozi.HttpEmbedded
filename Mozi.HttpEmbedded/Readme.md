# Mozi.HttpEmbedded 嵌入式Web服务器

## 项目介绍

Mozi.HttpEmbedded是一个基于.Net构建的嵌入式Web服务器，为.Net App提供web服务功能。

> 嵌入式的目标不是单片机，而是.Net应用程序。

> 此项目并不会实现非常丰富的大型Web服务器功能

> 项目基于.Net Framework 4.0开发,也可转换为.Net Core/.Net Standard项目


## 特点

Mozi.HttpEmbedded在Socket之上使用异步单线程模型,构建了一个HTTP服务器。

1. 嵌入式  
	本项目可作为.Net应用的内嵌Web服务器，亦可作为单独Web服务器。
2. 轻量化  
	项目编译结果小，部署后占用系统资源少。
3. 可用性  
	开箱即用，配置少，可控性高。同时遵从.Net平台Web项目开发的典型规范。
4. 低耦合  
	不改变现有业务逻辑，无需对现有代码进行改造，即可实现现有功能面向HTTP提供服务器。
5. 可控性  
	宿主程序对Web服务高度可控

## 典型应用场景

业务体系或功能已开发完成，期望在项目中加入一个Web服务器功能，同时不能对现有代码进行大规模改动。

1. APP内嵌WEB服务  
	通过HTTP方式对应用终端数据或文件，进行主动推送、下载
2. 简易WEB服务器  
	仅用很少的系统资源即可快速搭建WEB服务器
3. 程序运行时数据透视  
	监控终端程序运行时数据

> 在经典的Asp.Net开发中，Web服务的部署高度依赖于IIS,.Net Core项目则可基于Kestrel/IIS部署。

> 基于KESTREL或IIS部署的WEB项目，都基于Asp.Net体系。

## 原理及功能

1. HTTP协议  
	实现HTTP/1.1 HTTP/1.2

2. 通讯认证  
	实现基本认证(Basic)

3. Cookie管理  
	支持标准Cookie

4. HTTP请求方法  
	GET POST

5. 路由  
	实现了URL管理,参见Router模块

6. 引用与依赖关系  
	依赖于.Net Framework

7. Payload压缩  
	使用GZip压缩

8. 字符编码  
	字符编码部分使用UTF-8

## 功能与版本迭代
   不定期对Mozi.HttpEmbedded的功能进行完善,解决各种BUG。HTTP标准功能繁多，需要一步步实现。

## 版权说明
本项目采用MIT开源协议,欢迎复制，引用和修改。欢迎对功能提供建议。

## 使用说明

~~~csharp
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

~~~

## By [Jason][1] on Feb. 5,2020

[1]:mailto:brotherqian@163.com