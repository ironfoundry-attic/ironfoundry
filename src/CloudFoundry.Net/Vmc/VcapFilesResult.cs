namespace CloudFoundry.Net.Vmc
{
    using System.Collections.Generic;

    public class VcapFilesResult : VcapClientResult
    {
        private IList<FilesResultData> _files = new List<FilesResultData>();
        private IList<FilesResultData> _dirs = new List<FilesResultData>();

        public VcapFilesResult() : base() { }

        public VcapFilesResult(bool success) : base(success) { }

        public VcapFilesResult(string file)
        {
            File = file;
        }

        public bool IsFile
        {
            get { return false == File.IsNullOrWhiteSpace(); }
        }

        public string File
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