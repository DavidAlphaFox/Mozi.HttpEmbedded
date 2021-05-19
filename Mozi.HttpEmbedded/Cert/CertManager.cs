using System;
using System.Security.Cryptography.X509Certificates;

namespace Mozi.HttpEmbedded.Cert
{
    /// <summary>
    /// SSL证书管理
    /// </summary>
    internal class CertManager
    {
        public CertManager()
        {

        }
        /// <summary>
        /// 加载CERT文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public void LoadCert(string filePath)
        {
            X509Store store = new X509Store();
            X509Certificate2 cert = new X509Certificate2();
            cert.Import(new byte[] { }, "", X509KeyStorageFlags.PersistKeySet);
        }
        /// <summary>
        /// 验证
        /// </summary>
        /// <returns></returns>
        public bool Valid()
        {
            throw new NotImplementedException();
        }
    }
}
