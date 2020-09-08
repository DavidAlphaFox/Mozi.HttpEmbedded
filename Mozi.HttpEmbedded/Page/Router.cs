using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Mozi.HttpEmbedded.Page
{
    /// <summary>
    /// 全局路由
    /// </summary>
    public sealed class Router
    {
        private static Router _r;

        public static Router Default
        {
            get { return _r ?? (_r = new Router()); }
        }
        private List<Assembly> _assemblies=new List<Assembly>();

        private List<Type> apis = new List<Type>();

        private readonly List<RouteMapper> _mappers = new List<RouteMapper>() { new RouteMapper() { Pattern = "/{controller}/{id}" }, new RouteMapper() { Pattern = "/{controller}.{id}" } };
 
        private Router()
        {
            
        }

        /// <summary>
        /// 调起
        /// </summary>
        /// <param name="ctx"></param>
        internal object Invoke(HttpContext ctx)
        {
            Assembly ass = Assembly.GetExecutingAssembly();
            Type[] types = ass.GetExportedTypes();
            foreach (var type in types)
            {
                if (type.IsSubclassOf(typeof(BaseApi)))
                {
                   apis.Add(type); 
                }
            }
            
            ass.GetModules();
            string path = ctx.Request.Path;
            //确定路径映射关系
            AccessPoint ap=Match(path);
            Type cls = apis.Find(x => x.Name.Equals(ap.Domain, StringComparison.OrdinalIgnoreCase));

            MethodInfo method = cls.GetMethod(ap.Method,BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
            ParameterInfo[] pms = method.GetParameters();
            object[] args = new object[pms.Length];

            for (int i = 0; i < pms.Length; i++)
            {
               var argname = pms[i].Name;
               if (ctx.Request.Query.ContainsKey(argname))
               {
                  args[i] = ctx.Request.Query[argname];
               }
               if (ctx.Request.Post.ContainsKey(argname))
               {
                   args[i] = ctx.Request.Post[argname];
               }
            }
            object instance = Activator.CreateInstance(cls);
            //注入变量
            ((BaseApi) instance).Context = ctx;
            //调用方法
            return method.Invoke(instance, BindingFlags.IgnoreCase, null, args, CultureInfo.CurrentCulture);
            //调起相关方法 
        }

        /// <summary>
        /// 载入模块
        /// </summary>
        /// <returns></returns>
        public Router Register(string filePath)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 路由注入
        /// </summary>
        /// <param name="pattern">
        /// 匹配范式        
        /// <para>
        ///     范式:<code>api/{controller}/{id}</code> {controller}和{id}为固定参数       
        ///     <list type="table">
        ///         <item><term><c>{controller}</c></term><description>表示继承自<see cref="C:BaseApi"/>的类</description></item>
        ///         <item><term><c>{id}</c></term><description>类中的非静态方法名</description></item>
        ///     </list>        
        ///     <example>
        ///         范式1
        ///         api/{controller}/{id}
        ///         范式2
        ///         api/on{controller}/get{id}
        ///         范式3
        ///         api.{controller}.{id}
        ///         范式4
        ///         api.on{controller}.get{id}
        ///     </example>
        /// 
        /// </para>
        /// </param>
        /// <returns></returns>
        public Router Map(string pattern)
        {
            _mappers.Add(new RouteMapper(){Pattern = pattern});
            return this;
        }

        public AccessPoint Match(string path)
        {
            foreach (var mapper in _mappers)
            {
                if (mapper.Match(path))
                {
                    return mapper.Parse(path);
                }
            }
            return null;
        }
        /// <summary>
        /// 路由映射
        /// </summary>
        private class RouteMapper
        {

            private string _pattern = "";

            public string Pattern        
            {
                get
                {
                    return _pattern; 
                    
                } 
                set
                {
                    _pattern = value;
                    ApplyPattern(value);
                } 
            }

            private string Prefix { get; set; }
            private string Suffix { get; set; }
            private String Link   { get; set; }
            private string IdName { get; set; }
            private Regex Matcher { get; set; }

            public RouteMapper()
            {

            }

            private void ApplyPattern(string pattern)
            {
                //修正
                if (pattern.IndexOf("/", StringComparison.CurrentCulture) != 0)
                {
                    pattern = "/" + pattern;
                }
                //替换特殊字符
                int indCrl = pattern.IndexOf("{controller}", StringComparison.CurrentCulture);
                int indID = pattern.IndexOf("{id}", StringComparison.CurrentCulture);
         
                Prefix = pattern.Substring(0, indCrl);
                Link = pattern.Substring(indCrl + 12, indID - indCrl - 12);
                Suffix = "";
                Matcher = new Regex(String.Format("{0}[a-zA-Z]\\w+{1}[a-zA-Z]\\w+{2}", Regex.Escape(Prefix), Regex.Escape(Link), Regex.Escape(Suffix)), RegexOptions.IgnoreCase);
            }
            /// <summary>
            /// 判断是否匹配路由
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public bool Match(string path)
            {
              
                if (Matcher.IsMatch(path))
                {
                    return true;
                }
                return false;
            }
            /// <summary>
            /// 解析入口点
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public AccessPoint Parse(string path)
            {
                AccessPoint ap = null;
                if (Match(path))
                {
                    ap=new AccessPoint();
                    int indCrl=Prefix.Length;
                    int indLink=path.IndexOf(Link,indCrl+1,StringComparison.CurrentCulture);

                    ap.Domain = path.Substring(indCrl, indLink - indCrl);
                    ap.Method = path.Substring(indLink + Link.Length);
                }
                return ap;
            }
        }

        public class AccessPoint
        {
             public String Domain { get; set; }
             public String Method { get; set; }
        }
    }
}
