using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mozi.HttpEmbedded.Encode;

namespace Mozi.HttpEmbedded.WebDav.Utilities
{
    /// <summary>
    /// For generating an MD5 hash
    /// </summary>
    /// <remarks>
    /// Source: <see href="https://gist.github.com/kristopherjohnson/3021045" />
    /// </remarks>
    internal static class Md5Util
    {
        public static string Md5HashStringForUtf8String(string s)
        {
            byte[] bytes =StringEncoder.Encode(s);

            MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(bytes);
 
            return HexStringFromBytes(hashBytes);
        }

        /// <summary>
        /// Convert an array of bytes to a string of hex digits
        /// </summary>
        /// <param name="bytes">Array of bytes</param>
        /// <returns>
        /// String of hex digits
        /// </returns>
        public static string HexStringFromBytes(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string hex in bytes.Select(b => b.ToString("x2")))
                sb.Append(hex);
            return sb.ToString();
        }
    }
}