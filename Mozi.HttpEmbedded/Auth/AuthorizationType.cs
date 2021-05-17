using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded.Auth
{
    /// <summary>
    /// HTTP��֤����
    /// </summary>
    public class AuthorizationType:AbsClassEnum
    {
        public const string REALM = "HttpEmbedded";

        /// <summary>
        /// ������֤ ���Ĵ��� ����ȫ
        /// </summary>
        public static readonly AuthorizationType Basic    = new AuthorizationType("Basic");

        //TODO δʵ�ָ߼���֤
        internal static AuthorizationType Digest   = new AuthorizationType("Digest");
        internal static AuthorizationType WSSE     = new AuthorizationType("WSSE");

        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        protected override string Tag
        {
            get { return Name; }
        }

        private AuthorizationType(string name)
        {
            _name = name;
        }
    }
}