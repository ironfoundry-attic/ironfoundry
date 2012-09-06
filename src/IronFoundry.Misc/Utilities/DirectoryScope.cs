namespace IronFoundry.Misc.Utilities
{
    using System;
    using System.IO;

    public class DirectoryScope : IDisposable
    {
        private readonly string oldCwd;

        public static DirectoryScope Create(string workingPath)
        {
            return new DirectoryScope(workingPath);
        }

        private DirectoryScope(string workingPath)
        {
            oldCwd = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(workingPath);
        }

        public void Dispose()
        {
            Directory.SetCurrentDirectory(oldCwd);
        }
    }
}