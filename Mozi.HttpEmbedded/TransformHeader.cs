﻿using System;
using System.Collections.Generic;
using System.Text;
using Mozi.HttpEmbedded.Generic;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// 通讯头报文
    /// </summary>
    public class TransformHeader
    {

        public static byte[] Carriage = { (byte)ASCIICode.CR, (byte)ASCIICode.LF };

        /// <summary>
        /// 通讯头集合
        /// </summary>
        private Dictionary<string, string> HeaderData = new Dictionary<string, string>(new StringCompareIgnoreCase())
        {
        };
        /// <summary>
        /// 是否包含
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key)
        {
            return HeaderData.ContainsKey(key);
        }
        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            return HeaderData.ContainsKey(key)?HeaderData[key]:null;
        }
        /// <summary>
        /// 增加头部信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public  TransformHeader Add(String key, String value)
        {
            if (HeaderData.ContainsKey(key))
            {
                HeaderData[key] = value;
            }
            else
            {
                HeaderData.Add(key, value);
            }
            return this;
        }
        /// <summary>
        /// 增加头部信息
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public TransformHeader Add(HeaderProperty header,String value)
        {
            Add(header.PropertyTag,value);
            return this;
        }
        /// <summary>
        /// 获取缓存数据 带分割符号
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer()
        {
            List<byte> buffer=new List<byte>();
            foreach (var item in HeaderData)
            {
                buffer.AddRange(Encoding.UTF8.GetBytes(String.Format("{0}: {1}",item.Key,item.Value)));
                buffer.AddRange(Carriage);
            }
            return buffer.ToArray();
        }

        public  string this[string key]
        {
            get { return HeaderData[key]; }
            set { HeaderData[key] = value; }
        }
    }
}