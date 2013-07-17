namespace IronFoundry.Warden.Tasks
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Containers;
    using NLog;
    using Protocol;

    public abstract class ProcessCommand : TaskCommand
    {
        private readonly ManualResetEventSlim processExitLatch = new ManualResetEventSlim();
        private readonly StringBuilder stdout = new StringBuilder();
        private readonly StringBuilder stderr = new StringBuilder();

        private readonly ResourceLimits rlimits;

        private readonly Logger log = LogManager.GetCurrentClassLogger();

        protected ProcessCommand(Container container, string[] arguments, ResourceLimits rlimits) : base(container, arguments)
        {
            this.rlimits = rlimits;
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
            return Task.Run((Func<TaskCommandResult>)DoExecute);
        }

        protected abstract TaskCommandResult DoExecute();

        protected TaskCommandResult RunProcess(string workingDirectory, string executable, string processArguments)
        {
            log.Trace("Starting process: {0} {1}", executable, processArguments);

            int exitCode = -1;
            int pid = container.StartProcess(executable, workingDirectory, processArguments, rlimits,
                ProcessOutputReceived, 
                ProcessErrorReceived,
                ec => {
                    exitCode = ec;
                    processExitLatch.Set();
                });

            if (pid > 0)
            {
                log.Trace("Started Process ID: '{0}'", pid);
                processExitLatch.Wait();

                return new TaskCommandResult(exitCode, stdout.ToString(), stderr.ToString());
            }
            else
            {
                throw new Exception("Unable to start the specified process");
            }
        }

        private void ProcessOutputReceived(string output)
        {
            if (output != null)
            {
                string outputLine = output + '\n';
                stdout.Append(outputLine);
                OnStatusAvailable(new TaskCommandStatus(null, outputLine, null));
            }
        }

        private void ProcessErrorReceived(string error)
        {
            if (error != null)
            {
                string outputLine = error + '\n';
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
