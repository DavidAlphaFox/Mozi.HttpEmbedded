using System.Collections.Generic;

namespace Mozi.HttpEmbedded.Generic
{
    /// <summary>
    /// 忽略大小写比较器
    /// </summary>
    public class StringCompareIgnoreCase : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (x != null && y != null)
            {
                return x.ToLowerInvariant() == y.ToLowerInvariant();
            }
            return false;
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }
}