namespace IronFoundry.Warden.Tasks
{
    using System;
    using System.IO;
    using Containers;
    using ICSharpCode.SharpZipLib.GZip;
    using ICSharpCode.SharpZipLib.Tar;

    public class TarCommand : TaskCommand
    {
        private static readonly char[] trimChars = new[] { '/' };
        private readonly string sourceDirectory;
        private readonly string targetFile;

        public TarCommand(Container container, string[] arguments)
            : base(container, arguments)
        {
            if (arguments.Length != 2)
            {
                throw new ArgumentException("tar command must have two arguments: source directory and target file name.");
            }

            this.sourceDirectory = container.ConvertToPathWithin(arguments[0]);
            if (!Directory.Exists(this.sourceDirectory))
            {
                throw new ArgumentException(String.Format("tar command: first argument must be directory that exists ('{0}')", this.sourceDirectory));
            }

            if (arguments[1].IsNullOrWhiteSpace())
            {
                throw new ArgumentException("tar command: second argument must be a file name.");
            }
            else
            {
                this.targetFile = container.ConvertToPathWithin(arguments[1]);
            }
        }

        public override TaskCommandResult Execute()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(sourceDirectory);
                using (var fs = File.OpenWrite(targetFile))
                {
                    using (var gzipStream = new GZipOutputStream(fs))
                    {
                        using (var tarArchive = TarArchive.CreateOutputTarArchive(gzipStream))
                        {
                            tarArchive.RootPath = ".";
                            var tarEntry = TarEntry.CreateEntryFromFile(".");
                            tarArchive.WriteEntry(tarEntry, true);
                        }
                    }
                }
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDirectory);
            }
            return new TaskCommandResult(0, String.Format("tar: '{0}' -> '{1}'", sourceDirectory, targetFile), null); 
        }
    }
}
