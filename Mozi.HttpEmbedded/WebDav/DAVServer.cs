﻿using System.Collections.Generic;
using Mozi.HttpEmbedded.WebDav.Exceptions;
using Mozi.HttpEmbedded.WebDav.Method;
using Mozi.HttpEmbedded.WebDav.MethodHandlers;
using Mozi.HttpEmbedded.WebDav.Stores;
using Mozi.HttpEmbedded.WebDav.Stores.DiskStore;

namespace Mozi.HttpEmbedded.WebDav
{
    /// <summary>
    /// WebDAV服务器
    /// </summary>
    public class DavServer 
    {
        private  IWebDavStore _store;
        /// <summary>
        /// WEBDAV 方法列表
        /// </summary>
        private readonly Dictionary<RequestMethod, IMethodHandler> _methodHandlers = new Dictionary<RequestMethod, IMethodHandler>
        {
            { RequestMethod.PROPFIND,   new Propfind()  },
            { RequestMethod.PROPPATCH,  new Proppatch() },
            { RequestMethod.MKCOL,      new MkCol() },
            { RequestMethod.COPY ,      new Copy() },
            { RequestMethod.MOVE,       new Move() },
            { RequestMethod.LOCK ,      new Lock() },
            { RequestMethod.UNLOCK,     new Unlock() }
        };

        public const string HttpUser = "WebDAV.User";

        public DavServer()
        {

        }

        public DavServer SetStore(string path)
        {
            _store = new WebDavDiskStore(path);
            return this;
        }
        /// <summary>
        /// 响应请求
        /// </summary>
        /// <param name="state">The state.</param>
        /// <exception cref="WebDavUnauthorizedException">If the user is unauthorized or has no access</exception>
        /// <exception cref="WebDavNotFoundException">If the item was not found</exception>
        public StatusCode ProcessRequest(ref HttpContext context)
        {
            RequestMethod method = context.Request.Method;
            IMethodHandler methodHandler;
            if (!_methodHandlers.TryGetValue(method, out methodHandler))
            {
                return StatusCode.MethodNotAllowed;
            }
            context.Response.AddHeader("DAV", "1,2,1#extend");

            return methodHandler.ProcessRequest(this, context, _store);
        }
    }
}