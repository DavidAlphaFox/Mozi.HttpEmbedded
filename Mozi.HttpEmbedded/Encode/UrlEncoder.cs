using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Mozi.HttpEmbedded.Encode
{
    /// <summary>
    /// URLÌØÊâ×Ö·û×ªÂë
    /// </summary>
    public static class UrlEncoder
    {
        private static readonly char[] From = { ' ', '"','#','%','&','(', ')', '+',',','/',':', ';','<','=', '>','?', '@','\\','|' };

        private static readonly string[] To = { "%20", "%22", "%23", "%25", "%26", "%28", "%29", "%2B", "%2C", "%2F", "%3A", "%3B", "%3C", "%3D", "%3E","%3F", "%40", "%5C", "%7C"};
        
        /// <summary>
        /// URLÌØÊâ×Ö·û½âÂë
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Decode(string data)
        {
            for (int i = 0; i < To.Length; i++)
            {
                string s = To[i];
                while (data.Contains(s))
                {
                    data = data.Replace(s, To[i]);
                }
            }
            return data;
        }
        /// <summary>
        /// URLÌØÊâ×Ö·û±àÂë
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Encode(string data)
        {
            for (int i = 0; i < From.Length; i++)
            {
                char s = From[i];
                while (data.Contains(s.ToString()))
                {
                    data = data.Replace(s.ToString(), To[i]);
                }
            }
            return data;
        }
        /// <summary>
        /// ½âÎö²éÑ¯×Ö·û´®
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseQuery(string data)
        {
            data = Decode(data);
            Dictionary<string, string> res=new Dictionary<string, string>();
            string[] querys = data.Split(new[] { (char)ASCIICode.AND }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in querys)
            {
                string[] kp = item.Split(new[] { (char)ASCIICode.EQUAL }, StringSplitOptions.RemoveEmptyEntries);
                if (kp.Length > 0)
                {
                    var key = kp[0];
                    var value = kp.Length > 1 ? kp[1] : "";
                    res.Add(key, value);
                }
            }
            return res;
        }
    }
}