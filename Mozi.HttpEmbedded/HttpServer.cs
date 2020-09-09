using System;
using System.Collections.Generic;
using Mozi.HttpEmbedded.Auth;
using Mozi.HttpEmbedded.Compress;
using Mozi.HttpEmbedded.Page;
using Mozi.HttpEmbedded.Source;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// Http服务器
    /// </summary>
    public class HttpServer
    {
        
        private readonly SocketServer _sc=new SocketServer();
        
        private int _port=80;

        private string _serverName = "HttpEmbedded";

        /// <summary>
        /// 支持的HTTP服务协议版本
        /// </summary>
        public HttpVersion  ProxyVersion { get; set; }
        /// <summary>
        /// 是否使用基本认证
        /// </summary>
        public bool EnableAuth { get; private set; }

        private Authenticator Auth { get;  set; }

        /// <summary>
        /// 是否开启压缩
        /// </summary>
        public bool EnableCompress { get; private set; }
        /// <summary>
        /// 压缩选项
        /// </summary>
        public CompressOption ZipOption { get; private set; }
        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port
        {
            get { return _port; }
            private set { _port = value; }
        }
        /// <summary>
        /// 时区
        /// </summary>
        public String Timezone { get; set; }
        /// <summary>
        /// 编码格式
        /// </summary>
        public String Encoding { get; set; }
        /// <summary>
        /// 服务器名称
        /// </summary>
        public String ServerName
        {
            get { return _serverName; }
            private set { _serverName = value; }
        }
        /// <summary>
        /// 服务器用户
        /// </summary>
        private List<User> _users=new List<User>(); 

        public HttpServer()
        {
            Auth=new Authenticator();
            _sc.OnServerStart += _sc_OnServerStart;
            _sc.OnClientConnect += _sc_OnClientConnect;
            _sc.OnReceiveStart += _sc_OnReceiveStart;
            _sc.AfterReceiveEnd += _sc_AfterReceiveEnd;
            _sc.AfterServerStop += _sc_AfterServerStop;
        }

        void _sc_AfterServerStop(object sender, ServerArgs args)
        {
            throw new NotImplementedException();
        }

        void _sc_OnReceiveStart(object sender, DataTransferArgs args)
        {
           
        }
        //TODO 响应码处理有问题
        /// <summary>
        /// 响应请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void _sc_AfterReceiveEnd(object sender, DataTransferArgs args)
        {
            HttpContext context=new HttpContext();
            context.Response=new HttpResponse();
            StatusCode sc = StatusCode.Success;
            try
            {
                context.Request = HttpRequest.Parse(args.Data);
                if (!EnableAuth)
                {
                    sc = HandleRequest(ref context);
                }
                else
                {
                    sc = HandleAuth(ref context);
                }
            }
            catch (Exception ex)
            {
                sc = StatusCode.InternalServerError;
            }
            finally
            {                
                context.Response.AddHeader(HeaderProperty.Server, ServerName);
                context.Response.SetStatus(sc);
                args.Socket.Send(context.Response.GetBuffer());
                //TODO HTTP/1.1 通过Coonection控制连接 服务器同时对连接进行监测 保证服务器效率
                args.Socket.Close(100);
            }
            GC.Collect();
        }
        /// <summary>
        /// 处理认证
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private StatusCode HandleAuth(ref HttpContext context)
        {
            var authorization = context.Request.Headers.GetValue(HeaderProperty.Authorization.PropertyTag);
            if (!String.IsNullOrEmpty(authorization) && Auth.Check(authorization))
            {
                return HandleRequest(ref context);
            }
            else
            {
                context.Response.AddHeader(HeaderProperty.WWWAuthenticate, String.Format("{0} realm=\"{1}\"", Auth.AuthType.Name,AuthorizationType.REALM));
                return StatusCode.Unauthorized;
            }
        }
        /// <summary>
        /// 处理响应
        /// </summary>
        /// <param name="context"></param>
        private StatusCode HandleRequest(ref HttpContext context)
        {
            RequestMethod method = context.Request.Method;
           
            if (method == RequestMethod.GET || method == RequestMethod.POST || method == RequestMethod.HEAD||method==RequestMethod.PUT ||method==RequestMethod.OPTIONS||method == RequestMethod.DELETE||method==RequestMethod.TRACE||method==RequestMethod.CONNECT)
            {
                StaticFiles st = StaticFiles.Default;
                var path = context.Request.Path;
                string fileext = GetFileExt(path);
                string contenttype = Mime.GetContentType(fileext);
                //判断资源类型
                bool isStatic = st.IsStatic(fileext);
                context.Response.Headers.Add(HeaderProperty.ContentType, contenttype);
                //静态文件处理
                if (st.Enabled && isStatic)
                {
                    //响应静态文件
                    if (st.Exists(path, ""))
                    {               
                        string ifmodifiedsince =context.Request.Headers.GetValue(HeaderProperty.IfModifiedSince.PropertyTag);
                        if (st.CheckIfModified(path, ifmodifiedsince))
                        {
                            DateTime dtModified = st.GetLastModified(path).ToUniversalTime();
                            context.Response.Headers.Add(HeaderProperty.LastModified, dtModified.ToString("r"));
                            context.Response.Write(st.Load(path, ""));
                        }
                        else
                        {
                            return StatusCode.NotModified;
                        }
                    }
                    else
                    {
                        return StatusCode.NotFound;
                    }
                }
                else
                {
                    //响应动态页面
                    return HandleRequestRoutePages(ref context);
                }
            }
            //WEBDAV部分
            else
            {
                
            }
            return StatusCode.Success;
        }
        /// <summary>
        /// 取URL资源扩展名
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GetFileExt(string path)
        {
            string[] file = path.Split(new[] { (char)ASCIICode.QUESTION }, StringSplitOptions.RemoveEmptyEntries);
            string ext = "";
            string purepath = file[0];
            if (purepath.LastIndexOf((char)ASCIICode.DOT) >= 0)
            {
                ext = purepath.Substring(purepath.LastIndexOf((char)ASCIICode.DOT) + 1);
            }
            return ext;
        }
        /// <summary>
        /// 路由页面
        /// </summary>
        /// <param name="context"></param>
        private StatusCode HandleRequestRoutePages(ref HttpContext context)
        {
            Router router=Router.Default;
            if (router.Match(context.Request.Path) != null)
            {
                router.Invoke(context);
                context.Response.Write("<html>"
                                       + "<head></head>"
                                       + "<body>"
                                       + "  <strong>Welcome to a Web Server Developed base on c#!</strong>"
                                       + "</body>"
                                       + "</html>");
                return StatusCode.Success;
            }
            return StatusCode.NotFound; 
        }

        void _sc_OnServerStart(object sender, ServerArgs args)
        {
            //throw new NotImplementedException();
        }

        void _sc_OnClientConnect(object sender, ClientConnectArgs args)
        {
            //throw new NotImplementedException();
        }
        /// <summary>
        /// 配置服务端口
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public HttpServer SetPort(int port)
        {
            _port = port;
            return this;
        }
        /// <summary>
        /// 启用认证
        /// <para>此方法可连续配置用户</para>
        /// </summary>
        /// <param name="at"><see cref="E:Auth.AuthType"/></param>
        /// <returns></returns>
        public HttpServer UseAuth(AuthorizationType at)
        {
            EnableAuth = true;
            Auth.SetAuthType(at);
            return this;
        }
        /// <summary>
        /// 设置服务器认证用户
        /// <para>如果<see cref="F:EnableAuth"/>=<see cref="Boolean.False"/>,此设置就没有意义</para>
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userPassword"></param>
        /// <returns></returns>
        public HttpServer SetUser(String userName, String userPassword)
        {
            Auth.SetUser(userName, userName);
            return this;
        }
        /// <summary>
        /// 启用Gzip
        /// </summary>
        /// <returns></returns>
        public HttpServer UseGzip(CompressOption option)
        {
            EnableCompress = true;
            ZipOption = option;
            return this;
        }

        /// <summary>
        /// 允许静态文件访问
        /// </summary>
        /// <param name="root">静态文件目录</param>
        /// <returns></returns>
        public HttpServer UseStaticFiles(string root)
        {
            StaticFiles.Default.Enabled = true;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public HttpServer UseErrorPage(String page)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置服务器名称
        /// </summary>
        /// <param name="serverName"></param>
        /// <returns></returns>
        public HttpServer SetServerName(string serverName)
        {
            _serverName = serverName;
            return this;
        }
        /// <summary>
        /// 启动服务器
        /// </summary>
        public void Start()
        {
            _sc.StartServer(_port);
        }
        /// <summary>
        /// 检查访问黑名单
        /// </summary>
        private void CheckIfBlocked()
        {
            
        }
        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void Shutdown()
        {
            _sc.StopServer();
        }
    }
}
