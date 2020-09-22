using System;
using System.Collections.Generic;
using Mozi.HttpEmbedded.DataSerialize;

namespace Mozi.HttpEmbedded.Serialize
{
    /// <summary>
    /// XML序列化
    /// </summary>
    public class XMLSerializer : ISerializer
    {
        public T Decode<T>(string data)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> DecodeList<T>(string data)
        {
            throw new NotImplementedException();
        }

        public string Encode(object data)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// JSON序列化
    /// </summary>
    public class JSONSerializer: ISerializer
    {
        public T Decode<T>(string data)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> DecodeList<T>(string data)
        {
            throw new NotImplementedException();
        }

        public string Encode(object data)
        {
            throw new NotImplementedException();
        }
    }
}
