using System;
using System.Security.Cryptography;

namespace Mozi.HttpEmbedded.Common
{
    /// <summary>
    /// 加密解密类
    /// </summary>
    internal class Encrypt
    {
        /// <summary>
        /// 生成真随机数
        /// </summary>
        /// <param name="len"></param>
        /// <param name="isPureNumber"></param>
        /// <returns></returns>
        public static string GenerateRandom(int len, int isPureNumber = 0)
        {
            var seads = isPureNumber == 1 ? "0123456789" : "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string gr = "";
            byte[] bytes = new byte[4];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            var random = new Random(BitConverter.ToInt32(bytes, 0));
            for (int i = 0; i < len; i++)
            {
                gr += seads[random.Next(0, seads.Length)];
            }
            return gr;
        }
    }
}
