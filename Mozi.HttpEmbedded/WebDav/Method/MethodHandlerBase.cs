using System;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.Common;
using Mozi.HttpEmbedded.WebDav.Storage;
using Mozi.HttpEmbedded.WebDav.Exception;

namespace Mozi.HttpEmbedded.WebDav.MethodHandlers
{
    /// <summary>
    /// </summary>
    internal abstract class MethodHandlerBase
    {
        private const int DepthInfinity = -1;

        public static IWebDavStoreCollection GetParentCollection(IWebDavStore store, string path)
        {
            IWebDavStoreCollection collection;
            try
            {
                collection = WebDavExtensions.GetStoreItem(path, store) as IWebDavStoreCollection;
            }
            catch (UnauthorizedAccessException)
            {
                throw  new WebDavUnauthorizedException();
            }
            catch(WebDavNotFoundException)
            {
                throw new WebDavConflictException();
            }
            if (collection == null)
            {
                throw new WebDavConflictException();
            }

            return collection;
        }

        public static IWebDavStoreItem GetItemFromCollection(IWebDavStoreCollection collection, string childUri)
        {
            IWebDavStoreItem item;
            try
            {
                UrlTree ut = new UrlTree(childUri);
                item = collection.GetItemByName(UrlEncoder.Decode(ut.Last().TrimEnd('/', '\\')));
            }
            catch (UnauthorizedAccessException)
            {
                throw new WebDavUnauthorizedException();
            }
            catch (WebDavNotFoundException)
            {
                throw new WebDavNotFoundException();
            }
            if (item == null)
                throw new WebDavNotFoundException();

            return item;
        }

        public static int GetDepthHeader(HttpRequest request)
        {
            string depth = request.Headers["Depth"];

            if (string.IsNullOrEmpty(depth) || depth.Equals("infinity"))
            {
                return DepthInfinity;
            }

            int value;
            if (!int.TryParse(depth, out value))
            {
                return DepthInfinity;
            }

            if (value == 0 || value == 1)
            {
                return value;
            }

            return DepthInfinity;
        }

        public static bool GetOverwriteHeader(HttpRequest request)
        {
            string overwrite = request.Headers["Overwrite"];

            return overwrite != null && overwrite.Equals("T");
        }

        public static string GetTimeoutHeader(HttpRequest request)
        {

            string timeout = request.Headers["Timeout"];

            if (string.IsNullOrEmpty(timeout) || timeout.Equals("infinity") || timeout.Equals("Infinite, Second-4100000000"))
            {
                return "Second-345600";
            }
            return timeout;
        }
        public static Uri GetDestinationHeader(HttpRequest request)
        {
            string destinationUri = request.Headers["Destination"];

           
            if (!string.IsNullOrEmpty(destinationUri))
                return new Uri(destinationUri);
            
            throw new WebDavConflictException();
        }
    }
}
