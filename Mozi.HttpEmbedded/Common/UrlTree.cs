using System;
using System.Collections.Generic;
using System.Linq;

namespace Mozi.HttpEmbedded.Common
{
    /// <summary>
    /// 树形目录
    /// </summary>
    class UrlTree
    {
        private LinkedList<string> tree;
        /// <summary>
        /// 分割URL路径 path:/root/dir/subdir/
        /// <para>
        ///     <see cref="HttpRequest.Path">path</see>是从请求头中取出的路径数据
        /// </para>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        List<string> GetSegement(string path)
        {
            return path.Split(new char[] { (char)ASCIICode.DIVIDE }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        /// <summary>
        /// 实例化
        /// <para>
        ///     <see cref="HttpRequest.Path">path</see>是从请求头中取出的路径数据
        /// </para>
        /// </summary>
        /// <param name="path"></param>
        public UrlTree(string path)
        {
            tree = new LinkedList<string>();
            List<string> segements = GetSegement(path);
            foreach (var seg in segements)
            {
                tree.AddLast(seg);
            }
        }

        public string GetRoot()
        {
            return tree.First();
        }

        public string Last()
        {
            return tree.Last.Value;
        }
    }
}
