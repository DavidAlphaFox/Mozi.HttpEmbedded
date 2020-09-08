using System;
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
        //public static AuthType Digest   = new AuthType("Digest");
        //public static AuthType WSSE     = new AuthType("WSSE");

        private string _name;

        

        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        protected override string Tag
        {
            get { return Name; }
        }

        private AuthorizationType(String name)
        {
            _name = name;
        }
    }
}