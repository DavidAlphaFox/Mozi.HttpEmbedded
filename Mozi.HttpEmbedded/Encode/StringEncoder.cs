namespace Mozi.HttpEmbedded.Encode
{
    //�����ͳһ��ȡUTF-8����
    /// <summary>
    /// �ַ�������
    /// </summary>
    public static class StringEncoder
    {
        public static byte[] Encode(string data)
        {
            return System.Text.Encoding.UTF8.GetBytes(data);
        }

        public static string Decode(byte[] data)
        {
            return System.Text.Encoding.UTF8.GetString(data);
        }
    }
}