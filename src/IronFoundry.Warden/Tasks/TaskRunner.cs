namespace IronFoundry.Warden.Tasks
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Containers;
    using Jobs;
    using Protocol;
    using Newtonsoft.Json;
    using NLog;
    using Utilities.Impersonation;

    public class TaskRunner : IJobRunnable
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private readonly Container container;
        private readonly ITaskRequest request;
        private readonly TaskCommandDTO[] commands;

        private ConcurrentQueue<TaskCommandStatus> jobStatusQueue;

        private bool runningAsync = false;

        public TaskRunner(Container container, ITaskRequest request)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            this.container = container;

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            this.request = request;

            if (this.request.Script.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("request.Script can't be empty.");
            }

            commands = JsonConvert.DeserializeObject<TaskCommandDTO[]>(request.Script);
            if (commands.IsNullOrEmpty())
            {
                throw new ArgumentException("Expected to run at least one command.");
            }
        }

        public bool HasStatus
        {
            get { return !jobStatusQueue.IsEmpty; }
        }

        public IEnumerable<IJobStatus> RetrieveStatus()
        {
            var statusList = new List<IJobStatus>();

            TaskCommandStatus status;
            while (jobStatusQueue.TryDequeue(out status))
            {
                statusList.Add(status);
            }

            return statusList;
        }

        public event EventHandler<JobStatusEventArgs> JobStatusAvailable;

        public Task<IJobResult> RunAsync()
        {
            runningAsync = true;
            jobStatusQueue = new ConcurrentQueue<TaskCommandStatus>();
            return DoRunAsync();
        }

        public void Cancel()
        {
            cts.Cancel();
        }

        public IJobResult Run()
        {
            var commandFactory = new TaskCommandFactory(container);
            var credential = container.GetCredential();
            var results = new List<TaskCommandResult>();
            foreach (TaskCommandDTO cmd in commands)
            {
                if (cts.IsCancellationRequested)
                {
                    break;
                }

                TaskCommand taskCommand = commandFactory.Create(cmd.Command, cmd.Args);
                try
                {
                    var task = new Func<TaskCommandResult>(taskCommand.Execute);
                    TaskCommandResult result = request.Privileged 
                        ? Impersonator.InvokeImpersonated(credential, task) 
                        : task();
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    results.Add(new TaskCommandResult(1, null, ex.Message));
                    break;
                }
            }

            return FlattenResults(results);
        }

        private async Task<IJobResult> DoRunAsync()
        {
            var commandFactory = new TaskCommandFactory(container);
            var credential = container.GetCredential();
            var results = new List<TaskCommandResult>();
            foreach (TaskCommandDTO cmd in commands)
            {
                if (cts.IsCancellationRequested)
                {
                    break;
                }

                TaskCommand taskCommand = commandFactory.Create(cmd.Command, cmd.Args);

                try
                {
                    if (runningAsync && taskCommand.CanExecuteAsync)
                    {
                        var asyncTaskCommand = (AsyncTaskCommand)taskCommand;
                        asyncTaskCommand.StatusAvailable += asyncTaskCommand_StatusAvailable;

                        var task = new Func<Task<TaskCommandResult>>(asyncTaskCommand.ExecuteAsync);
                        TaskCommandResult result = await (request.Privileged
                            ? Impersonator.InvokeImpersonatedAsync(credential, task)
                            : task());

                        asyncTaskCommand.StatusAvailable -= asyncTaskCommand_StatusAvailable;
                        results.Add(result);
                    }
                    else
                    {
                        var task = new Func<TaskCommandResult>(taskCommand.Execute);
                        TaskCommandResult result = request.Privileged
                            ? Impersonator.InvokeImpersonated(credential, task)
                            : task();
                        results.Add(result);
                    }
                }
                catch (Exception ex)
                {
                    results.Add(new TaskCommandResult(1, null, ex.Message));
                    break;
                }
            }

            return FlattenResults(results);
        }

        private void asyncTaskCommand_StatusAvailable(object sender, TaskCommandStatusEventArgs e)
        {
            TaskCommandStatus status = e.Status;
            if (status == null)
            {
                throw new InvalidOperationException("status");
            }

            if (JobStatusAvailable == null)
            {
                jobStatusQueue.Enqueue(status); // TODO: what if too much status?
            }
            else
            {
                jobStatusQueue.Enqueue(status);
                TaskCommandStatus queued;
                while ((!jobStatusQueue.IsEmpty) && jobStatusQueue.TryDequeue(out queued))
                {
                    JobStatusAvailable(this, new JobStatusEventArgs(status));
                }
            }
        }

        private static IJobResult FlattenResults(IEnumerable<TaskCommandResult> results)
        {
            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            int lastExitCode = 0;
            foreach (var result in results)
            {
                stdout.SmartAppendLine(result.Stdout);
                stderr.SmartAppendLine(result.Stderr);
                if (result.ExitCode != 0)
                {
                    lastExitCode = result.ExitCode;
                    break;
                }
            }

            return new TaskCommandResult(lastExitCode, stdout.ToString(), stderr.ToString());
        }
    }
}
