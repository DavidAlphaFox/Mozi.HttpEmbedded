using System;
using System.Collections.Generic;

namespace Mozi.HttpEmbedded.Encode
{
    //DONE ���URL����ת�������
    /// <summary>
    /// URL�����ַ�ת��
    /// </summary>
    public static class UrlEncoder
    {
        private static readonly char[] From = { ' ', '"', '#', '%', '&', '(', ')', '+', ',', '/', ':', ';', '<', '=', '>', '?', '@', '\\', '|' };

        private static readonly string[] To = { "%20", "%22", "%23", "%25", "%26", "%28", "%29", "%2B", "%2C", "%2F", "%3A", "%3B", "%3C", "%3D", "%3E", "%3F", "%40", "%5C", "%7C" };

        /// <summary>
        /// URL�����ַ�����
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Decode(string data)
        {
            //�����ַ�
            for (int i = 0; i < To.Length; i++)
            {
                string s = To[i];
                if (data.Contains(s))
                {
                    data = data.Replace(s, To[i]);
                }
            }

            //�������ַ�
            var startIndex = -1;
            var endIndex = -1;
            for(int i = 0; i < data.Length; i++)
            {
                var item = data[i];
                if (startIndex == -1 && item.Equals((char)ASCIICode.PERCENT))
                {
                    startIndex = i;
                }
                if (item.Equals((char)ASCIICode.PERCENT) &&!data[i+2].Equals((char)ASCIICode.PERCENT))
                {
                    endIndex = i+2;
                }
            }
            if (startIndex != -1)
            {

                var groupMath = data.Substring(startIndex, endIndex - startIndex + 1);
                data = data.Replace(groupMath, StringEncoder.Decode(Hex.From(groupMath.Replace((char)ASCIICode.PERCENT, (char)ASCIICode.SPACE))));

            }
            return data;
        }
        /// <summary>
        /// URL�����ַ�����
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Encode(string data)
        {
            for (int i = 0; i < From.Length; i++)
            {
                char s = From[i];
                if (data.Contains(s.ToString()))
                {
                    data = data.Replace(s.ToString(), To[i]);
                }
            }
            return data;
        }
        /// <summary>
        /// ������ѯ�ַ���
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseQuery(string data)
        {
            data = Decode(data);
            Dictionary<string, string> res = new Dictionary<string, string>();
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