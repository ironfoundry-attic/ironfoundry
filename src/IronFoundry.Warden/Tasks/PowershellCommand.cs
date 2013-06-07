namespace IronFoundry.Warden.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using IronFoundry.Warden.Containers;

    public class PowershellCommand : TaskCommand
    {
        private const string powershellArgFmt = "-NoProfile -NonInteractive -ExecutionPolicy RemoteSigned -WindowStyle Hidden -File \"{0}\"";

        private readonly StringBuilder stdout = new StringBuilder();
        private readonly StringBuilder stderr = new StringBuilder();

        public PowershellCommand(Container container, string[] arguments)
            : base(container, arguments)
        {
            if (base.arguments.IsNullOrEmpty())
            {
                throw new ArgumentException("powershell: command must have at least one argument.");
            }
        }

        public override TaskCommandResult Execute()
        {
            using (var ps1File = container.TempFileInContainer(".ps1"))
            {
                File.WriteAllLines(ps1File.FullName, container.ConvertToPathsWithin(arguments), Encoding.ASCII);

                string psArgs = String.Format(powershellArgFmt, ps1File.FullName);
                int exitCode = 0;
                using (var process = new BackgroundProcess(ps1File.DirectoryName, "powershell.exe", psArgs))
                {
                    process.ErrorDataReceived += process_ErrorDataReceived;
                    process.OutputDataReceived += process_OutputDataReceived;

                    process.StartBackground();
                    exitCode = process.ExitCode;

                    process.ErrorDataReceived -= process_ErrorDataReceived;
                    process.OutputDataReceived -= process_OutputDataReceived;
                }

                string sout = stdout.ToString();
                string serr = stderr.ToString();
                return new TaskCommandResult(exitCode, sout, serr);
            }
        }

        private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                stdout.AppendLine(e.Data);
            }
        }

        private void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                stderr.AppendLine(e.Data);
            }
        }
    }
}
