using System.Collections.Generic;

namespace Mozi.HttpEmbedded.DataSerialize
{
    /// <summary>
    /// 数据序列化接口
    /// </summary>
    public interface ISerializer
    {
        string Encode(object data);

        T Decode<T>(string data);

        IEnumerable<T> DecodeList<T>(string data);
    }
}
