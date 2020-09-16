using System;
using System.IO;

namespace Mozi.HttpEmbedded.Common
{
    //TODO 对日志进行统一管理 用户动作的记录 日志的存储
    /// <summary>
    /// 记录类
    /// </summary>
    public static class Log
    {
        private static readonly string LogDir = AppDomain.CurrentDomain.BaseDirectory + @"\Log\";
        
        /// <summary>
        /// 追加式写入日志
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        public static void Save(string name, string info,LogLevel level)
        {
            DirectoryInfo dir = new DirectoryInfo(LogDir);
            if (!dir.Exists)
            {
                dir.Create();
            }
            StreamWriter sw = new StreamWriter(LogDir + name + "_" + DateTime.Now.ToString("yyyyMMdd") + ".log", true);
            sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " "+Enum.GetName(typeof(LogLevel),level)+" | " + info);
            sw.Flush();
            sw.Close();
        }
        public static void Save(string info)
        {
            Save("error", info,LogLevel.Error);
        }
        public static void Debug(string info)
        {
            Save("error", info, LogLevel.Debug);
        }

        public static void Warn(string info)
        {
            Save("error", info, LogLevel.Warn);
        }
        /// <summary>
        /// 日志级别
        /// </summary>
        public enum LogLevel
        {
            Error = 1,
            Debug = 2,
            Info = 3,
            Warn=4
        }
    }
}
