﻿using Mozi.HttpEmbedded.Encode;
using System.Reflection;

namespace Mozi.HttpEmbedded.Docment
{
    public class DocLoader
    {
        public static string Load(string docName)
        {
            Assembly ass = Assembly.GetExecutingAssembly();
            var resName = ass.GetName().Name + ".Document." + docName;
            var stream=ass.GetManifestResourceStream(resName);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();
            return StringEncoder.Decode(buffer);
        }
    }
}
