namespace IronFoundry.Warden.Tasks
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using IronFoundry.Warden.Containers;

    public abstract class ProcessCommand : TaskCommand
    {
        private readonly StringBuilder stdout = new StringBuilder();
        private readonly StringBuilder stderr = new StringBuilder();

        private readonly bool shouldImpersonate = false;

        public ProcessCommand(Container container, string[] arguments, bool shouldImpersonate)
            : base(container, arguments)
        {
            this.shouldImpersonate = shouldImpersonate;
        }

        public override TaskCommandResult Execute()
        {
            return DoExecute();
        }

        /*
         * Asynchronous execution
         */
        public event EventHandler<TaskCommandStatusEventArgs> StatusAvailable;

        public Task<TaskCommandResult> ExecuteAsync()
        {
            return Task.Factory.StartNew<TaskCommandResult>(DoExecute);
        }

        protected abstract TaskCommandResult DoExecute();

        protected TaskCommandResult RunProcess(string workingDirectory, string executable, string arguments)
        {
            using (var process = new BackgroundProcess(workingDirectory, executable, arguments, GetImpersonatationCredential()))
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

        private NetworkCredential GetImpersonatationCredential()
        {
            NetworkCredential impersonationCredential = null;
            if (shouldImpersonate)
            {
                impersonationCredential = container.GetCredential();
            }
            return impersonationCredential;
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

        private void OnStatusAvailable(TaskCommandStatus status)
        {
            if (StatusAvailable != null)
            {
                StatusAvailable(this, new TaskCommandStatusEventArgs(status));
            }
        }
    }
}
