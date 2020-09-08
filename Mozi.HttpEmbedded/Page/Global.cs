using System.Collections.Generic;

namespace Mozi.HttpEmbedded.Page
{
    /// <summary>
    /// 全局数据
    /// 此处定义的数据会被全局使用 仅只读 功能类似于宏
    /// </summary>
    public class Global
    {
        public Dictionary<string,object> _data=new Dictionary<string, object>();
 
        public  Global Set()
        {
            return this;
        }
    }
}
