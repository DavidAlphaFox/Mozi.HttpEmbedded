namespace Mozi.HttpEmbedded.Secure
{
    /// <summary>
    /// 密码套件类型
    /// </summary>
    enum  CipherSuite
    {
        TLS_NULL_WITH_NULL_NULL = 0x00, 

        TLS_RSA_WITH_NULL_MD5 =  0x01, 
        TLS_RSA_WITH_NULL_SHA =  0x02 ,
        TLS_RSA_EXPORT_WITH_RC4_40_MD5 =  0x03 ,
        TLS_RSA_WITH_RC4_128_MD5 =  0x04 ,
        TLS_RSA_WITH_RC4_128_SHA =  0x05 ,
        TLS_RSA_EXPORT_WITH_RC2_CBC_40_MD5 =  0x06 ,
        TLS_RSA_WITH_IDEA_CBC_SHA =  0x07 ,
        TLS_RSA_EXPORT_WITH_DES40_CBC_SHA =  0x08 ,
        TLS_RSA_WITH_DES_CBC_SHA =  0x09 ,
        TLS_RSA_WITH_3DES_EDE_CBC_SHA =  0x0A ,

        TLS_DH_DSS_EXPORT_WITH_DES40_CBC_SHA =  0x0B ,
        TLS_DH_DSS_WITH_DES_CBC_SHA =  0x0C ,
        TLS_DH_DSS_WITH_3DES_EDE_CBC_SHA =  0x0D ,
        TLS_DH_RSA_EXPORT_WITH_DES40_CBC_SHA =  0x0E ,
        TLS_DH_RSA_WITH_DES_CBC_SHA =  0x0F ,
        TLS_DH_RSA_WITH_3DES_EDE_CBC_SHA =  0x10 ,
        TLS_DHE_DSS_EXPORT_WITH_DES40_CBC_SHA =  0x11 ,
        TLS_DHE_DSS_WITH_DES_CBC_SHA =  0x12 ,
        TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA =  0x13 ,
        TLS_DHE_RSA_EXPORT_WITH_DES40_CBC_SHA =  0x14 ,
        TLS_DHE_RSA_WITH_DES_CBC_SHA =  0x15 ,
        TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA =  0x16 ,

        TLS_DH_anon_EXPORT_WITH_RC4_40_MD5 =  0x17 ,
        TLS_DH_anon_WITH_RC4_128_MD5 =  0x18 ,
        TLS_DH_anon_EXPORT_WITH_DES40_CBC_SHA =  0x19, 
        TLS_DH_anon_WITH_DES_CBC_SHA =  0x1A ,
        TLS_DH_anon_WITH_3DES_EDE_CBC_SHA =  0x1B ,
    }
}
