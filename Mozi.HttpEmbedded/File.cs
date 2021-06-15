using System.Collections.Generic;

namespace Mozi.HttpEmbedded
{
    /// <summary>
    /// �ϴ����ļ�
    /// </summary>
    public class File
    {
        public string FileName { get; set; }
        public string FieldName { get; set; }
        public int FileIndex { get; set; }
        public byte[] FileData { get; set; }
        internal string FileTempSavePath { get; set; }

        ~File()
        {
            FileData = null;
        }
    }
    /// <summary>
    /// �ļ�����
    /// </summary>
    public class FileCollection
    {
        private readonly List<File> _files = new List<File>();

        public File this[string name] { get { return GetFile(name); } }
        public File this[int ind] { get { return _files[ind]; } }

        /// <summary>
        /// �ļ�����
        /// </summary>
        public List<File> Files { get { return _files; } }

        public int Length { get { return _files.Count; } }

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