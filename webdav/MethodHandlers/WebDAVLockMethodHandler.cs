﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using Common.Logging;
using WebDAVSharp.Server.Adapters;
using WebDAVSharp.Server.Exceptions;
using WebDAVSharp.Server.Stores;

namespace WebDAVSharp.Server.MethodHandlers
{
    /// <summary>
    /// This class implements the <c>LOCK</c> HTTP method for WebDAV#.
    /// </summary>
    internal class WebDavLockMethodHandler : WebDavMethodHandlerBase, IWebDavMethodHandler
    {
        /// <summary>
        /// Gets the collection of the names of the HTTP methods handled by this instance.
        /// </summary>
        /// <value>
        /// The names.
        /// </value>
        public IEnumerable<string> Names
        {
            get
            {
                return new[]
                {
                    "LOCK"
                };
            }
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="server">The <see cref="WebDavServer" /> through which the request came in from the client.</param>
        /// <param name="context">The
        /// <see cref="IHttpListenerContext" /> object containing both the request and response
        /// objects to use.</param>
        /// <param name="store">The <see cref="IWebDavStore" /> that the <see cref="WebDavServer" /> is hosting.</param>
        /// <exception cref="WebDAVSharp.Server.Exceptions.WebDavPreconditionFailedException"></exception>
        public void ProcessRequest(WebDavServer server, IHttpListenerContext context, IWebDavStore store)
        {
            //ILog log = LogManager.GetCurrentClassLogger();
            var log = LogManager.GetLogger(GetType().Name);

            /***************************************************************************************************
             * Retreive al the information from the request
             ***************************************************************************************************/

            // read the headers
            var depth = GetDepthHeader(context.Request);
            var timeout = GetTimeoutHeader(context.Request);

            // Initiate the XmlNamespaceManager and the XmlNodes
            XmlNamespaceManager manager = null;
            XmlNode lockscopeNode = null, locktypeNode = null, ownerNode = null;

            // try to read the body
            try
            {
                var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
                var requestBody = reader.ReadToEnd();

                if (!requestBody.Equals("") && requestBody.Length != 0)
                {
                    var requestDocument = new XmlDocument();
                    requestDocument.LoadXml(requestBody);

                    if (requestDocument.DocumentElement != null && requestDocument.DocumentElement.LocalName != "prop" &&
                        requestDocument.DocumentElement.LocalName != "lockinfo")
                    {
                        log.Debug("LOCK method without prop or lockinfo element in xml document");
                    }

                    manager = new XmlNamespaceManager(requestDocument.NameTable);
                    manager.AddNamespace("D", "DAV:");
                    manager.AddNamespace("Office", "schemas-microsoft-com:office:office");
                    manager.AddNamespace("Repl", "http://schemas.microsoft.com/repl/");
                    manager.AddNamespace("Z", "urn:schemas-microsoft-com:");

                    // Get the lockscope, locktype and owner as XmlNodes from the XML document
                    lockscopeNode = requestDocument.DocumentElement.SelectSingleNode("D:lockscope", manager);
                    locktypeNode = requestDocument.DocumentElement.SelectSingleNode("D:locktype", manager);
                    ownerNode = requestDocument.DocumentElement.SelectSingleNode("D:owner", manager);
                }
                else
                {
                    throw new WebDavPreconditionFailedException();
                }
            }
            catch (Exception ex)
            {
                log.Warn(ex.Message);
                throw;
            }

            /***************************************************************************************************
             * Lock the file or folder
             ***************************************************************************************************/

            var isNew = false;

            // Get the parent collection of the item
            var collection = GetParentCollection(server, store, context.Request.Url);

            try
            {
                // Get the item from the collection
                var item = GetItemFromCollection(collection, context.Request.Url);
            }
            catch (Exception)
            {
                collection.CreateDocument(context.Request.Url.Segments.Last().TrimEnd('/', '\\'));
                isNew = true;
            }
           
            

            /***************************************************************************************************
             * Create the body for the response
             ***************************************************************************************************/

            // Create the basic response XmlDocument
            var responseDoc = new XmlDocument();
            var responseXml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><D:prop " +
                "xmlns:D=\"DAV:\"><D:lockdiscovery><D:activelock/></D:lockdiscovery></D:prop>";
            responseDoc.LoadXml(responseXml);

            // Select the activelock XmlNode
            var activelock = responseDoc.DocumentElement.SelectSingleNode("D:lockdiscovery/D:activelock", manager);

            // Import the given nodes
            activelock.AppendChild(responseDoc.ImportNode(lockscopeNode, true));
            activelock.AppendChild(responseDoc.ImportNode(locktypeNode, true));
            activelock.AppendChild(responseDoc.ImportNode(ownerNode, true));

            // Add the additional elements, e.g. the header elements

            // The timeout element
            var timeoutProperty = new WebDavProperty("timeout", timeout);
            activelock.AppendChild(timeoutProperty.ToXmlElement(responseDoc));

            // The depth element
            var depthProperty = new WebDavProperty("depth", (depth == 0 ? "0" : "Infinity"));
            activelock.AppendChild(depthProperty.ToXmlElement(responseDoc));
            
            // The locktoken element
            var locktokenProperty = new WebDavProperty("locktoken", "");
            var locktokenElement = locktokenProperty.ToXmlElement(responseDoc);
            var hrefProperty = new WebDavProperty("href", "opaquelocktoken:e71d4fae-5dec-22df-fea5-00a0c93bd5eb1");
            locktokenElement.AppendChild(hrefProperty.ToXmlElement(responseDoc));
            activelock.AppendChild(locktokenElement);

            /***************************************************************************************************
             * Send the response
             ***************************************************************************************************/
            
            // convert the StringBuilder
            var resp = responseDoc.InnerXml;
            var responseBytes = Encoding.UTF8.GetBytes(resp);

            if (isNew)
            {
                // HttpStatusCode doesn't contain WebDav status codes, but HttpWorkerRequest can handle these WebDav status codes
                context.Response.StatusCode = (int)HttpStatusCode.Created;
                //context.Response.StatusDescription = HttpWorkerRequest.GetStatusDescription((int)HttpStatusCode.Created);
                context.Response.StatusDescription = HttpStatusCode.Created.ToString();
            }
            else
            {
                // HttpStatusCode doesn't contain WebDav status codes, but HttpWorkerRequest can handle these WebDav status codes
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                //context.Response.StatusDescription = HttpWorkerRequest.GetStatusDescription((int)HttpStatusCode.OK);
                context.Response.StatusDescription = HttpStatusCode.OK.ToString();
            }


            // set the headers of the response
            context.Response.ContentLength64 = responseBytes.Length;
            context.Response.AdaptedInstance.ContentType = "text/xml";

            // the body
            context.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);

            context.Response.Close();
        }
    }
}