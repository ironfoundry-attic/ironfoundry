namespace CloudFoundry.Net.Vmc
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class VcapFilesResult : VcapClientResult
    {
        private IList<FilesResultData> _files = new List<FilesResultData>();
        private IList<FilesResultData> _dirs = new List<FilesResultData>();

        public VcapFilesResult() : base() { }

        public VcapFilesResult(bool success) : base(success) { }

        public VcapFilesResult(byte[] file)
        {
            if (null == file)
            {
                throw new ArgumentNullException("file");
            }
            else
            {
                File = file;
            }
        }

        public bool IsFile
        {
            get { return null != File; }
        }

        [JsonIgnore]
        public byte[] File
        {
            get;
            private set;
        }

        public IEnumerable<FilesResultData> Files
        {
            get { return _files; }
        }

        public IEnumerable<FilesResultData> Directories
        {
            get { return _dirs; }
        }

        public void AddFile(string fileName, string fileSize)
        {
            _files.Add(new FilesResultData(fileName, fileSize));
        }

        public void AddDirectory(string dirName)
        {
            _dirs.Add(new FilesResultData(dirName));
        }

        public class FilesResultData
        {
            public string Name { get; private set; }
            public string Size { get; private set; }

            public FilesResultData(string name)
            {
                Name = name;
            }

            public FilesResultData(string name, string size)
            {
                Name = name;
                Size = size;
            }
        }
    }
}