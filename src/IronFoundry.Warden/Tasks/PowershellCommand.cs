namespace IronFoundry.Warden.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using IronFoundry.Warden.Containers;

    public class PowershellCommand : AsyncTaskCommand
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

        public override Task<TaskCommandResult> ExecuteAsync()
        {
            return Task.Factory.StartNew<TaskCommandResult>(DoExecute);
        }

        public override TaskCommandResult Execute()
        {
            return DoExecute();
        }

        private TaskCommandResult DoExecute()
        {
            using (var ps1File = container.TempFileInContainer(".ps1"))
            {
                File.WriteAllLines(ps1File.FullName, container.ConvertToPathsWithin(arguments), Encoding.ASCII);

                string psArgs = String.Format(powershellArgFmt, ps1File.FullName);

                using (var process = new BackgroundProcess(ps1File.DirectoryName, "powershell.exe", psArgs))
                {
                    process.ErrorDataReceived += process_ErrorDataReceived;
                    process.OutputDataReceived += process_OutputDataReceived;

                    process.StartAndWait();

                    process.ErrorDataReceived -= process_ErrorDataReceived;
                    process.OutputDataReceived -= process_OutputDataReceived;

                    string sout = stdout.ToString();
                    string serr = stderr.ToString();

                    return new TaskCommandResult(process.ExitCode, sout, serr);
                }
            }
        }

        private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                string outputLine = e.Data + '\n';
                stdout.Append(outputLine);
                OnStatusAvailable(new TaskCommandStatus(null, outputLine, null));
            }
        }

        private void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                string outputLine = e.Data + '\n';
                stderr.Append(outputLine);
                OnStatusAvailable(new TaskCommandStatus(null, null, outputLine));
            }
        }
    }
}
