using System;
using System.Globalization;
using System.IO;

namespace Mozi.HttpEmbedded.Source
{
    /// <summary>
    /// 静态资源管理
    /// </summary>
    internal sealed class StaticFiles
    {
        public bool Enabled { get; set; }

        private string _root;

        private static StaticFiles _staticfiles;

        public static StaticFiles Default
        {
            get { return _staticfiles ?? (_staticfiles = new StaticFiles()); }
        }
        /// <summary>
        /// 静态文件根路径 本地磁盘路径
        /// </summary>
        public string RootDirectory { get { return _root; } }

        private StaticFiles()
        {
            _root = AppDomain.CurrentDomain.BaseDirectory;
            init();    
        }
        /// <summary>
        /// 设置静态文件根目录
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public StaticFiles SetRoot(string root)
        {
            if (!string.IsNullOrEmpty(root))
            {
                if (!root.EndsWith("/"))
                {
                    root = root + "/";
                }
                //TODO 区分相对路径和绝对路径
                if (Path.IsPathRooted(root))
                {
                    _root = root;
                }
                else
                {
                    _root = AppDomain.CurrentDomain.BaseDirectory +root;
                }
            }
            return this;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void init()
        {
           //载入MIME类型
        }

        /// <summary>
        /// 判断是否静态文件
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public bool IsStatic(string ext)
        {
            if (!string.IsNullOrEmpty(ext) && Mime.Types.ContainsKey(ext))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public bool Exists(string path, string ext)
        {
            return System.IO.File.Exists(_root + path);
        }
        /// <summary>
        /// 检查最后修改日期
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ifModifiedSince"></param>
        /// <returns><see cref="bool:true">Modified</see></returns>
        public bool CheckIfModified(string path, string ifModifiedSince)
        {
            DateTime dtModified=System.IO.File.GetLastWriteTime(_root + path);
            try
            {
                if (!string.IsNullOrEmpty(ifModifiedSince))
                {
                    DateTime dtSince = DateTime.ParseExact(ifModifiedSince, "ddd, dd MMM yyyy HH:mm:ss GMT",
                                                        CultureInfo.InvariantCulture,
                                                        DateTimeStyles.AdjustToUniversal).ToLocalTime();
                    if ((dtModified - dtSince).TotalSeconds<1)
                    {
                        return false;
                    }
                }
            }
            catch
            {
                
            }
            return true;
        }
        /// <summary>
        /// 取文件最后修改日期
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public DateTime GetLastModified(string path)
        {
            return System.IO.File.GetLastWriteTime(_root + path);
        }
        /// <summary>
        /// 提取文件流
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public byte[] Load(string path,string ext)
        {
            using (FileStream fs = new FileStream(_root + path, FileMode.Open))
            {
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                return data;
            }
        }
        /// <summary>
        /// 分段读取文件 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ext"></param>
        /// <param name="offset"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public byte[] Load(string path,string ext,int offset,int end)
        {
            if (end > offset&&offset>=0)
            {
                using (FileStream fs = new FileStream(_root+ path, FileMode.Open))
                {
                    byte[] data = new byte[fs.Length];
                    fs.Read(data, offset, end - offset + 1);
                    return data;
                }
            }
            else
            {
                return new byte[] { };
            }
        }
        /// <summary>
        /// 取文件大小
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public long GetFileSize(string path)
        {
            FileInfo fi = new FileInfo(path);
            return fi.Length;
        }
    }
}
