using System;
using System.Collections.Generic;
using System.Xml;
using Mozi.HttpEmbedded.WebDav.Exceptions;
using Mozi.HttpEmbedded.WebDav.MethodHandlers;
using Mozi.HttpEmbedded.WebDav.Stores;
using Mozi.HttpEmbedded.Common;
using Mozi.HttpEmbedded.Encode;

namespace Mozi.HttpEmbedded.WebDav.Method
{
    /// <summary>
    ///  <c>PROPFIND</c> WebDAV扩展方法
    /// </summary>
    internal class Propfind : WebDavMethodHandlerBase, IMethodHandler
    {
        private string _requestUri;
        private List<WebDavProperty> _requestedProperties;
        private List<IWebDavStoreItem> _webDavStoreItems;

        /// <summary>
        /// 响应请求
        /// </summary>
        /// <param name="server"><see cref="DavServer" /> </param>
        /// <param name="context">
        /// <see cref="HttpContext" /> 
        ///  </param>
        /// <param name="store"><see cref="IWebDavStore" /> <see cref="DavServer" /></param>
        /// <exception cref="WebDavUnauthorizedException"></exception>
        public StatusCode ProcessRequest(DavServer server, HttpContext context, IWebDavStore store)
        {
            bool isPropname = false;
            int depth = GetDepthHeader(context.Request);
            _requestUri = context.Request.Path.ToString();

            try
            {
                _webDavStoreItems = GetWebDavStoreItems(WebDavExtensions.GetStoreItem(_requestUri, store), depth);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode.Unauthorized;
            }

            //获取请求文档
            XmlDocument requestDoc = GetXmlDocument(context.Request);

            //提取请求信息
            _requestedProperties = new List<WebDavProperty>();
            if (requestDoc.DocumentElement != null)
            {
                if (requestDoc.DocumentElement.LocalName != "propfind")
                    Log.Save("PROPFIND method without propfind in xml document");
                else
                {
                    XmlNode n = requestDoc.DocumentElement.FirstChild;
                    if (n == null)
                        Log.Save("propfind element without children");
                    else
                    {
                        switch (n.LocalName)
                        {
                            case "allprop":
                                _requestedProperties = GetAllProperties();
                                break;
                            case "propname":
                                isPropname = true;
                                _requestedProperties = GetAllProperties();
                                break;
                            case "prop":
                                foreach (XmlNode child in n.ChildNodes)
                                    _requestedProperties.Add(new WebDavProperty(child.LocalName, "", child.NamespaceURI));
                                break;
                            default:
                                _requestedProperties.Add(new WebDavProperty(n.LocalName, "", n.NamespaceURI));
                                break;
                        }
                    }
                }
            }
            else
                _requestedProperties = GetAllProperties();
            XmlDocument responseDoc = ResponseDocument(context, isPropname, StatusCode.Success);


            //转换成字节码
            byte[] responseBytes =StringEncoder.Encode(responseDoc.InnerXml);
            context.Response.Write(responseBytes);
            context.Response.Headers.Add(HeaderProperty.ContentType.PropertyTag, "text/xml");

            return StatusCode.MultiStatus;
        }

        #region RetrieveInformation
        /// <summary>
        /// <see cref="IWebDavStoreItem" /> 
        /// <see cref="List{T}" /> 
        /// <see cref="IWebDavStoreItem" />
        /// </summary>
        /// <param name="iWebDavStoreItem"><see cref="IWebDavStoreItem" /> that needs to be converted</param>
        /// <param name="depth">The "Depth" header</param>
        /// <returns>
        /// A <see cref="List{T}" /> of <see cref="IWebDavStoreItem" />
        /// </returns>
        /// <exception cref="WebDavConflictException"></exception>
        private static List<IWebDavStoreItem> GetWebDavStoreItems(IWebDavStoreItem iWebDavStoreItem, int depth)
        {
            List<IWebDavStoreItem> list = new List<IWebDavStoreItem>();

            IWebDavStoreCollection collection = iWebDavStoreItem as IWebDavStoreCollection;
            if (collection != null)
            {
                list.Add(collection);
                if (depth == 0)
                    return list;
                foreach (IWebDavStoreItem item in collection.Items)
                {
                    try
                    {
                        list.Add(item);
                    }
                    catch (Exception)
                    {
                    }
                }
                return list;
            }
            if (!(iWebDavStoreItem is IWebDavStoreDocument))
                throw new WebDavConflictException();

            list.Add(iWebDavStoreItem);

            return list;
        }

        /// <summary>
        /// Reads the XML body of the 
        /// <see cref="HttpRequest" />
        /// and converts it to an 
        /// <see cref="XmlDocument" />
        /// </summary>
        /// <param name="request"><see cref="HttpRequest" /></param>
        /// <returns>
        /// <see cref="XmlDocument" /> that contains the request body
        /// </returns>
        private XmlDocument GetXmlDocument(HttpRequest request)
        {
            try
            {

                string requestBody = StringEncoder.Decode(request.Body);

                if (!string.IsNullOrEmpty(requestBody))
                {
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(requestBody);
                    return xmlDocument;
                }
            }
            catch (Exception)
            {
                Log.Warn("XmlDocument has not been read correctly");
            }

            return new XmlDocument();
        }

        /// <summary>
        /// Adds the standard properties for an Propfind allprop request to a <see cref="List{T}" /> of <see cref="WebDavProperty" />
        /// </summary>
        /// <returns>
        /// The list with all <see cref="WebDavProperty" />
        /// </returns>
        private List<WebDavProperty> GetAllProperties()
        {
            List<WebDavProperty> list = new List<WebDavProperty>
            {
                new WebDavProperty("creationdate"),
                new WebDavProperty("displayname"),
                new WebDavProperty("getcontentlength"),
                new WebDavProperty("getcontenttype"),
                new WebDavProperty("getetag"),
                new WebDavProperty("getlastmodified"),
                new WebDavProperty("resourcetype"),
                new WebDavProperty("supportedlock"),
                new WebDavProperty("ishidden")
            };
            //list.Add(new WebDAVProperty("getcontentlanguage"));
            //list.Add(new WebDAVProperty("lockdiscovery"));
            return list;
        }

        #endregion

        #region BuildResponseBody

        /// <summary>
        /// Builds <see cref="XmlDocument" /> containing the response body
        /// </summary>
        /// <param name="context"><see cref="HttpContext" /></param>
        /// <param name="propname">The boolean defining the Propfind propname request</param>
        /// <returns>
        /// <see cref="XmlDocument" /> containing the response body
        /// </returns>
        private XmlDocument ResponseDocument(HttpContext context, bool propname,StatusCode status)
        {
            // 实例XML文档
            XmlDocument responseDoc = new XmlDocument();
            const string responseXml = "<?xml version=\"1.0\"?><D:multistatus xmlns:D=\"DAV:\"></D:multistatus>";
            responseDoc.LoadXml(responseXml);

            XmlNamespaceManager manager = new XmlNamespaceManager(responseDoc.NameTable);
            manager.AddNamespace("D", "DAV:");
            manager.AddNamespace("Office", "schemas-microsoft-com:office:office");
            manager.AddNamespace("Repl", "http://schemas.microsoft.com/repl/");
            manager.AddNamespace("Z", "urn:schemas-microsoft-com:");

            int count = 0;

            foreach (IWebDavStoreItem webDavStoreItem in _webDavStoreItems)
            {
                WebDavProperty responseProperty = new WebDavProperty("response", "");
                XmlElement responseElement = responseProperty.ToXmlElement(responseDoc);

                string result;
                //TODO 取路径 此处需要测试
                if (count == 0)
                {
                    result = _requestUri;
                }
                else
                {
                    result = _requestUri + "/" + webDavStoreItem.Name + "/";
                }
                WebDavProperty hrefProperty = new WebDavProperty("href", result);
                responseElement.AppendChild(hrefProperty.ToXmlElement(responseDoc));
                count++;

                WebDavProperty propstatProperty = new WebDavProperty("propstat", "");
                XmlElement propstatElement = propstatProperty.ToXmlElement(responseDoc);

                WebDavProperty propProperty = new WebDavProperty("prop", "");
                XmlElement propElement = propProperty.ToXmlElement(responseDoc);

                foreach (WebDavProperty davProperty in _requestedProperties)
                {
                    propElement.AppendChild(PropChildElement(davProperty, responseDoc, webDavStoreItem, propname));
                }

                propstatElement.AppendChild(propElement);

               
                //TODO 此处响应状态码 检查是否正确
                WebDavProperty statusProperty = new WebDavProperty("status", "HTTP/1.1 " + status.Code.ToString()+" "+status.Text.ToString());
                propstatElement.AppendChild(statusProperty.ToXmlElement(responseDoc));

               
                responseElement.AppendChild(propstatElement);

               
                responseDoc.DocumentElement.AppendChild(responseElement);
            }

            return responseDoc;
        }

        /// <summary>
        /// <see cref="XmlElement" /> 
        /// <see cref="WebDavProperty" />
        /// </summary>
        /// <param name="webDavProperty"><see cref="WebDavProperty" /></param>
        /// <param name="xmlDocument"><see cref="XmlDocument" /></param>
        /// <param name="iWebDavStoreItem"><see cref="IWebDavStoreItem" /></param>
        /// <param name="isPropname"></param>
        /// <returns>
        /// <see cref="XmlElement" /> of <see cref="WebDavProperty" /> containing a value or child elements
        /// </returns>
        private XmlElement PropChildElement(WebDavProperty webDavProperty, XmlDocument xmlDocument, IWebDavStoreItem iWebDavStoreItem, bool isPropname)
        {
           
            if (isPropname)
            {
                webDavProperty.Value = string.Empty;
                return webDavProperty.ToXmlElement(xmlDocument);
            }

            webDavProperty.Value = GetWebDavPropertyValue(iWebDavStoreItem, webDavProperty);
            XmlElement xmlElement = webDavProperty.ToXmlElement(xmlDocument);

            if (webDavProperty.Name != "resourcetype" || !iWebDavStoreItem.IsCollection)
                return xmlElement;

            WebDavProperty collectionProperty = new WebDavProperty("collection", "");
            xmlElement.AppendChild(collectionProperty.ToXmlElement(xmlDocument));
            return xmlElement;
        }

        /// <summary>
        /// Gets the correct value for a <see cref="WebDavProperty" />
        /// </summary>
        /// <param name="webDavStoreItem"><see cref="IWebDavStoreItem" /> defines the values</param>
        /// <param name="davProperty"><see cref="WebDavProperty" /> that needs a value</param>
        /// <returns>
        /// A <see cref="string" /> containing the value
        /// </returns>
        private string GetWebDavPropertyValue(IWebDavStoreItem webDavStoreItem, WebDavProperty davProperty)
        {
            switch (davProperty.Name)
            {
                case "creationdate":
                    return webDavStoreItem.CreationDate.ToUniversalTime().ToString("s") + "Z";
                case "displayname":
                    return webDavStoreItem.Name;
                case "getcontentlanguage":
                    return string.Empty;
                case "getcontentlength":
                    return !webDavStoreItem.IsCollection ? "" + ((IWebDavStoreDocument)webDavStoreItem).Size : "";
                case "getcontenttype":
                    return !webDavStoreItem.IsCollection ? "" + ((IWebDavStoreDocument)webDavStoreItem).MimeType : "";
                case "getetag":
                    return !webDavStoreItem.IsCollection ? "" + ((IWebDavStoreDocument)webDavStoreItem).Etag : "";
                case "getlastmodified":
                    return webDavStoreItem.ModificationDate.ToUniversalTime().ToString("R");
                case "lockdiscovery":
                    return string.Empty;
                case "resourcetype":
                    return "";
                case "supportedlock":
                    return "";
                //webDavProperty.Value = "<D:lockentry><D:lockscope><D:shared/></D:lockscope><D:locktype><D:write/></D:locktype></D:lockentry>";
                case "ishidden":
                    return "" + webDavStoreItem.Hidden;
                default:
                    return string.Empty;
            }
        }

        #endregion
    }
}
