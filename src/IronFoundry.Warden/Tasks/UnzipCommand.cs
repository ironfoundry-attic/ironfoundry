namespace IronFoundry.Warden.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Properties;

    public class UnzipCommand : TaskCommand
    {
        private readonly FileInfo zipFile;
        private readonly DirectoryInfo destDir;

        public UnzipCommand(Container container, string[] arguments)
            : base(container, arguments)
        {
            if (arguments.IsNullOrEmpty() || arguments.Length != 2)
            {
                throw new ArgumentException("unzip: must have exactly two arguments.");
            }

            if (arguments[0].IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(Resources.UnzipCommand_MissingZipFileErrorMessage);
            }

            this.zipFile = new FileInfo(arguments[0]);
            if (!this.zipFile.Exists)
            {
                throw new ArgumentException(Resources.UnzipCommand_MissingZipFileErrorMessage);
            }

            if (arguments[1].IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(Resources.UnzipCommand_MissingDestDirErrorMessage);
            }
            this.destDir = new DirectoryInfo(container.ConvertToPathWithin(arguments[1]));
        }

        public override TaskCommandResult Execute()
        {
            if (destDir.Exists)
            {
                destDir.Delete(true);
            }
            else
            {
                destDir.Create();
            }

            ZipFile.ExtractToDirectory(zipFile.FullName, destDir.FullName);

            return new TaskCommandResult(0, String.Format("Extracted '{0}' to '{1}'", zipFile.FullName, destDir.FullName), null);
        }
    }
}
