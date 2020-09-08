using System;

namespace Mozi.HttpEmbedded.Encode
{
    /// <summary>
    /// B64转码
    /// </summary>
    public class Base64
    {
        public static String To(String data)
        {
            byte[] infos = StringEncoder.Encode(data);
            return Convert.ToBase64String(infos, Base64FormattingOptions.None);
        }

        public static String From(String data)
        {
            return StringEncoder.Decode(Convert.FromBase64String(data));
        }
    }
}
