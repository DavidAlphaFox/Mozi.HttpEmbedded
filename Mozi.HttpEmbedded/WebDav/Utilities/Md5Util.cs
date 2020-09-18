using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mozi.HttpEmbedded.Encode;

namespace Mozi.HttpEmbedded.WebDav.Utilities
{
    internal static class Md5Util
    {
        public static string Md5Hash4Utf8String(string s)
        {
            byte[] bytes =StringEncoder.Encode(s);

            MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(bytes);
 
            return HexStringFromBytes(hashBytes);
        }
        public static string HexStringFromBytes(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string hex in bytes.Select(b => b.ToString("x2")))
                sb.Append(hex);
            return sb.ToString();
        }
    }
}