using System;

namespace Mozi.HttpEmbedded.Encode
{
    /// <summary>
    /// �ַ�������
    /// </summary>
    public static class StringEncoder
    {
        public static byte[] Encode(String data)
        {
            return System.Text.Encoding.UTF8.GetBytes(data);
        }

        public static string Decode(byte[] data)
        {
            return System.Text.Encoding.UTF8.GetString(data);
        }


    }
}