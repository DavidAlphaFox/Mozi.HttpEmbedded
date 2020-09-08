﻿using Mozi.HttpEmbedded.Encode;

namespace Mozi.HttpEmbedded.Compress
{
    /// <summary>
    /// 压缩选项
    /// </summary>
    public class CompressOption
    {
        public TransformEncoding CompressType { get; set; }
        public int MinContentLenght { get; set; }
        public int CompressLevel    { get; set; }
    }
}