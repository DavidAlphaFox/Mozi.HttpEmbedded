using System.Collections.Generic;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 客户端访问控制 黑名单控制|白名单控制
    /// </summary>
    public class AccessManager
    {
        public static AccessManager _access;

        private List<string> _blacklist = new List<string>();

        public static AccessManager Instance
        {
            get { return _access ?? (_access = new AccessManager()); }
        }

        private AccessManager()
        {

        }
        private void AddBlackList(string ipAddress)
        {
            _blacklist.Add(ipAddress);
        }
        private void RemoveBlackList(string ipAddress)
        {
            _blacklist.Remove(ipAddress);
        }
        public bool CheckBlackList(string ipAddress)
        {
            return _blacklist.Exists(x => x.Equals(ipAddress));
        }
    }
}
