﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace WebDAVSharp.Server.MethodHandlers
{
    /// <summary>
    /// This class contains code to produce the built-in
    /// <see cref="IWebDavMethodHandler"/> instances known by WebDAV#.
    /// </summary>
    internal static class WebDavMethodHandlers
    {
        private static readonly List<IWebDavMethodHandler> _BuiltIn = new List<IWebDavMethodHandler>();

        /// <summary>
        /// Gets the collection of built-in <see cref="IWebDavMethodHandler"/>
        /// HTTP method handler instances.
        /// </summary>
        public static IEnumerable<IWebDavMethodHandler> BuiltIn
        {
            get
            {
                lock (_BuiltIn)
                {
                    if (_BuiltIn.Count == 0)
                        ScanAssemblies();
                }

                return _BuiltIn;
            }
        }

        /// <summary>
        /// Scans the WebDAV# assemblies for known <see cref="IWebDavMethodHandler"/>
        /// types.
        /// </summary>
        private static void ScanAssemblies()
        {
            var methodHandlerTypes = from type in typeof (WebDavServer).Assembly.GetTypes()
                where !type.IsAbstract
                where typeof(IWebDavMethodHandler).IsAssignableFrom(type)
                select type;

            var methodHandlerInstances =
                from type in methodHandlerTypes
                select (IWebDavMethodHandler)Activator.CreateInstance(type);

            _BuiltIn.AddRange(methodHandlerInstances);
        }
    }
}