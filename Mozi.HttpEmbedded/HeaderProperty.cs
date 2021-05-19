using System;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// HTTP����ͷ����
    /// </summary>
    public class HeaderProperty : AbsClassEnum
    {
        public static HeaderProperty Accept = new HeaderProperty("Accept");                                             // 	�û�����������MIME �����б� 	HTTP Content Negotiation 	HTTP/1.1
        public static HeaderProperty AcceptCH = new HeaderProperty("Accept-CH");                                          // 

        //    �г��������ݣ��������ɾݴ���ѡ���ʵ�����Ӧ�� 	HTTP Client Hints 	
        public static HeaderProperty AcceptCharset = new HeaderProperty("Accept-Charset");                                     // 	�г��û�����֧�ֵ��ַ����� 	HTTP Content Negotiation 	HTTP/1.1
        public static HeaderProperty AcceptFeatures = new HeaderProperty("Accept-Features");                                    // 	HTTP Content Negotiation 	RFC 2295, ��8.2
        public static HeaderProperty AcceptEncoding = new HeaderProperty("Accept-Encoding");                                    // 	�г��û�����֧�ֵ�ѹ�������� 	HTTP Content Negotiation 	HTTP/1.1
        public static HeaderProperty AcceptLanguage = new HeaderProperty("Accept-Language");                                    // 	�г��û�����������ҳ�����ԡ� 	HTTP Content Negotiation 	HTTP/1.1
        //Accept-Ranges 			
        public static HeaderProperty AccessControlAllowCredentials = new HeaderProperty("Access-Control-Allow-Credentials");      // 	HTTP Access Control and Server Side Access Control 	W3C Cross-Origin Resource Sharing
        public static HeaderProperty AccessControlAllowOrigin = new HeaderProperty("Access-Control-Allow-Origin");                // 	HTTP Access Control and Server Side Access Control 	W3C Cross-Origin Resource Sharing
        public static HeaderProperty AccessControlAllowMethods = new HeaderProperty("Access-Control-Allow-Methods");              // 	HTTP Access Control and Server Side Access Control 	W3C Cross-Origin Resource Sharing
        public static HeaderProperty AccessControlAllowHeaders = new HeaderProperty("Access-Control-Allow-Headers");              // 	HTTP Access Control and Server Side Access Control 	W3C Cross-Origin Resource Sharing
        public static HeaderProperty AccessControlMaxAge = new HeaderProperty("Access-Control-Max-Age");                          // 	HTTP Access Control and Server Side Access Control 	W3C Cross-Origin Resource Sharing
        public static HeaderProperty AccessControlExposeHeaders = new HeaderProperty("Access-Control-Expose-Headers");            // 	HTTP Access Control and Server Side Access Control 	W3C Cross-Origin Resource Sharing
        public static HeaderProperty AccessControlRequestMethod = new HeaderProperty("Access-Control-Request-Method");            // 	HTTP Access Control and Server Side Access Control 	W3C Cross-Origin Resource Sharing
        public static HeaderProperty AccessControlRequestHeaders = new HeaderProperty("Access-Control-Request-Headers");          // 	HTTP Access Control and Server Side Access Control 	W3C Cross-Origin Resource Sharing
        public static HeaderProperty Age = new HeaderProperty("Age");// 			                                                    
        public static HeaderProperty Allow = new HeaderProperty("Allow");// 			                                                
        public static HeaderProperty Alternates = new HeaderProperty("Alternates");                                               // 	HTTP Content Negotiation 	RFC 2295, ��8.3
        public static HeaderProperty Authorization = new HeaderProperty("Authorization");                                         // 	�����÷�������֤�û������ƾ֤ 		
        public static HeaderProperty CacheControl = new HeaderProperty("Cache-Control");                                          //  HTTP Caching FAQ 	
        public static HeaderProperty Connection = new HeaderProperty("Connection");                                               // 		
        public static HeaderProperty ContentEncoding = new HeaderProperty("Content-Encoding");                                    // 		
        public static HeaderProperty ContentLanguage = new HeaderProperty("Content-Language");                                    // 		
        public static HeaderProperty ContentLength = new HeaderProperty("Content-Length");                                        // 			
        public static HeaderProperty ContentLocation = new HeaderProperty("Content-Location");                                    // 			
        public static HeaderProperty ContentMD5 = new HeaderProperty("Content-MD5");                                              // 	δʵ�� (�鿴 bug 232030) 	
        public static HeaderProperty ContentRange = new HeaderProperty("Content-Range");                                          // 			
        public static HeaderProperty ContentSecurityPolicy = new HeaderProperty("Content-Security-Policy");                       // 	�����û�������һ��ҳ���Ͽ��Լ���ʹ�õ���Դ�� 	CSP (Content Security Policy) 	W3C Content Security Policy
        public static HeaderProperty ContentType = new HeaderProperty("Content-Type");                                            // 	ָʾ�������ĵ���MIME ���͡������û������������ȥ������յ������ݡ� 		
        public static HeaderProperty Cookie = new HeaderProperty("Cookie");                                                       // 			RFC 2109
        public static HeaderProperty DNT = new HeaderProperty("DNT");                                                             // 	���ø�ֵΪ1�� �����û���ȷ�˳��κ���ʽ�����ϸ��١� 	Supported by Firefox 4, Firefox 5 for mobile, IE9, and a few major companies. 	Tracking Preference Expression (DNT)
        public static HeaderProperty Date = new HeaderProperty("Date");                                                           // 			
        public static HeaderProperty ETag = new HeaderProperty("ETag");                                                           // 		HTTP Caching FAQ 	
        public static HeaderProperty Expect = new HeaderProperty("Expect");                                                       // 			
        public static HeaderProperty Expires = new HeaderProperty("Expires");                                                     // 		HTTP Caching FAQ 	
        public static HeaderProperty From = new HeaderProperty("From");                                                           // 			
        public static HeaderProperty Host = new HeaderProperty("Host");                                                           // 			
        public static HeaderProperty IfMatch = new HeaderProperty("If-Match");                                                    // 			
        public static HeaderProperty IfModifiedSince = new HeaderProperty("If-Modified-Since");                                   // 		HTTP Caching FAQ 	
        public static HeaderProperty IfNoneMatch = new HeaderProperty("If-None-Match");                                           // 		HTTP Caching FAQ 	
        public static HeaderProperty IfRange = new HeaderProperty("If-Range");                                                    // 			
        public static HeaderProperty IfUnmodifiedSince = new HeaderProperty("If-Unmodified-Since");                               // 			
        public static HeaderProperty LastEventID = new HeaderProperty("Last-Event-ID");                                           // 	��������������ǰHTTP�����Ͻ��յ�����¼���ID������ͬ���ı�/�¼����� 	Server-Sent Events 	Server-Sent Events spec
        public static HeaderProperty LastModified = new HeaderProperty("Last-Modified");                                          // 		HTTP Caching FAQ 	
        public static HeaderProperty Link = new HeaderProperty("Link");                                                           // 	

        //��ͬ��HTML��ǩ�е�"link"����������HTTP���ϣ�����һ�����ȡ����Դ��ص�URL�Լ���ϵ�����ࡣ


        //For the rel=prefetch case, see Link Prefetching FAQ


        //Introduced in HTTP 1.1's RFC 2068, section 19.6.2.4, it was removed in the final HTTP 1.1 spec, then reintroduced, with some extensions, in RFC 5988
        public static HeaderProperty Location = new HeaderProperty("Location");                                                   // 			
        public static HeaderProperty MaxForwards = new HeaderProperty("Max-Forwards");                                            // 			
        public static HeaderProperty Negotiate = new HeaderProperty("Negotiate");                                                 // 		HTTP Content Negotiation 	RFC 2295, ��8.4
        public static HeaderProperty Origin = new HeaderProperty("Origin");                                                       // 		HTTP Access Control and Server Side Access Control 	More recently defined in the Fetch spec (see Fetch API.) Originally defined in W3C Cross-Origin Resource Sharing
        public static HeaderProperty Pragma = new HeaderProperty("Pragma");                                                       // 		for the pragma: nocache value see HTTP Caching FAQ 	
        public static HeaderProperty ProxyAuthenticate = new HeaderProperty("Proxy-Authenticate");                                // 			
        public static HeaderProperty ProxyAuthorization = new HeaderProperty("Proxy-Authorization");                              // 			
        public static HeaderProperty Range = new HeaderProperty("Range");                                                         // 			
        public static HeaderProperty Referer = new HeaderProperty("Referer");                                                     // 	

        //����ע�⣬��HTTP / 0.9�淶��������������������Э��ĺ����汾�б�����

        public static HeaderProperty RetryAfter = new HeaderProperty("Retry-After");// 			
        public static HeaderProperty SecWebsocketExtensions = new HeaderProperty("Sec-Websocket-Extensions");// 			 Websockets
        public static HeaderProperty SecWebsocketKey = new HeaderProperty("Sec-Websocket-Key");// 			 Websockets
        public static HeaderProperty SecWebsocketOrigin = new HeaderProperty("Sec-Websocket-Origin");// 			 Websockets
        public static HeaderProperty SecWebsocketProtocol = new HeaderProperty("Sec-Websocket-Protocol");// 			 Websockets
        public static HeaderProperty SecWebsocketVersion = new HeaderProperty("Sec-Websocket-Version");// 			 Websockets
        public static HeaderProperty Server = new HeaderProperty("Server");// 			
        public static HeaderProperty SetCookie = new HeaderProperty("Set-Cookie");// 			RFC 2109
        public static HeaderProperty SetCookie2 = new HeaderProperty("Set-Cookie2");// 			RFC 2965
        public static HeaderProperty StrictTransportSecurity = new HeaderProperty("Strict-Transport-Security");// 		HTTP Strict Transport Security 	IETF reference
        public static HeaderProperty TCN = new HeaderProperty("TCN");// 		HTTP Content Negotiation 	RFC 2295, ��8.5
        public static HeaderProperty TE = new HeaderProperty("TE");// 			
        public static HeaderProperty Trailer = new HeaderProperty("Trailer");// 	

        //�г�������Ϣ����֮����β�����д����ͷ�����������������һЩֵ����Content-MD5���ڴ�������ʱ����ע�⣬Trailer����ͷ�����г�Content-Length :, Trailer����Transfer-Encoding��headers��
        //        RFC 2616, ��14.40
        public static HeaderProperty TransferEncoding = new HeaderProperty("Transfer-Encoding");// 			
        public static HeaderProperty Upgrade = new HeaderProperty("Upgrade");// 			
        public static HeaderProperty UserAgent = new HeaderProperty("User-Agent");// 		for Gecko's user agents see the User Agents Reference 	
        public static HeaderProperty VariantVary = new HeaderProperty("Variant-Vary");// 		HTTP Content Negotiation 	RFC 2295, ��8.6
        public static HeaderProperty Vary = new HeaderProperty("Vary");// 	

        //�г�������Web������ѡ���ض����ݵ������ı�ͷ���˷��������ڸ�Ч����ȷ���淢�͵���Դ����Ҫ��
        //    HTTP Content Negotiation & HTTP Caching FAQ 	
        public static HeaderProperty Via = new HeaderProperty("Via");// 			
        public static HeaderProperty Warning = new HeaderProperty("Warning");// 			
        public static HeaderProperty WWWAuthenticate = new HeaderProperty("WWW-Authenticate");// 			
        public static HeaderProperty XContentDuration = new HeaderProperty("X-Content-Duration");// 		Configuring servers for Ogg media 	
        public static HeaderProperty XContentSecurityPolicy = new HeaderProperty("X-Content-Security-Policy");// 		Using Content Security Policy 	
        public static HeaderProperty XDNSPrefetchControl = new HeaderProperty("X-DNSPrefetch-Control");// 		Controlling DNS prefetching 	
        public static HeaderProperty XFrameOptions = new HeaderProperty("X-Frame-Options");// 		The XFrame-Option Response Header 	
        public static HeaderProperty XRequestedWith = new HeaderProperty("X-Requested-With");// 	

        //ͨ����ֵΪ��XMLHttpRequest��ʱʹ��
        //        Not standard

        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }

        protected override string Tag { get { return PropertyName; } }

        private HeaderProperty()
        {

        }
        private HeaderProperty(string tag)
        {
            PropertyName = tag;
        }
        /// <summary>
        /// תΪ�ַ���
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} {1}", PropertyName, PropertyValue);
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static HeaderProperty Parse(string data)
        {
            var tag = data.Substring(0, data.IndexOf((char)ASCIICode.COLON));
            var value = data.Substring(data.IndexOf((char)ASCIICode.SPACE) + 1);
            return new HeaderProperty() { PropertyName = tag, PropertyValue = value };
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static HeaderProperty Parse(byte[] data)
        {
            int itag = -1;
            itag = Array.IndexOf(data, ASCIICode.COLON);
            byte[] btag = new byte[itag], bvalue = new byte[data.Length - itag - 2];
            Array.Copy(data, btag, btag.Length);
            Array.Copy(data, itag + 2, bvalue, 0, bvalue.Length);
            return new HeaderProperty() { PropertyName = StringEncoder.Decode(btag), PropertyValue = StringEncoder.Decode(bvalue) };
        }
    }
}