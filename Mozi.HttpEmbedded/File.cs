using System;
using System.Collections.Generic;

namespace Mozi.HttpEmbedded
{
    public class File
    {
        public String FileName  { get; set; }
        public int FileIndex    { get; set; }
        public byte[] FileData  { get; set; }
    }

    public class FileCollection
    {
        private List<File> _files=new List<File>();

        public File this[string name] { get { return GetFile(name); } }

        public File this[int ind]
        {
            get { return _files[0]; }
        }

        public int Length { get { return _files.Count; }}

        public File GetFile(string name)
        {
            return _files.Find(x => x.FileName.Equals(name));
        }

        internal void Append(File f)
        {
            _files.Add(f);
        }
    }

}