namespace IronFoundry.Dea.Agent
{
    using System;
    using System.IO;

    internal sealed class FileData : IDisposable
    {
        private readonly string filePath;

        public FileData(Stream argFileStream, string argFilePath)
        {
            FileStream = argFileStream;
            filePath = argFilePath;
        }

        public Stream FileStream
        {
            get;
            private set;
        }

        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        ~FileData()
        {
            dispose(false);
        }

        private void dispose(bool argDisposing)
        {
            if (argDisposing)
            {
                try
                {
                    FileStream.Close();
                    FileStream.Dispose();
                }
                catch { }
                try
                {
                    File.Delete(filePath);
                }
                catch { }
            }
        }
    }
}