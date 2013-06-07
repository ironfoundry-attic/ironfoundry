namespace IronFoundry.Warden.Tasks
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Jobs;
    using IronFoundry.Warden.Protocol;
    using Newtonsoft.Json;
    using NLog;

    public class TaskRunner : IJobRunnable
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private readonly Container container;
        private readonly ITaskRequest request;
        private readonly TaskCommandDTO[] commands;
        private readonly ConcurrentQueue<TaskCommandResult> results = new ConcurrentQueue<TaskCommandResult>();

        private bool runningAsync = false;
        private bool commandsCompleted = false;

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

        public event EventHandler<JobStatusEventArgs> JobStatusAvailable;

        public IJobStatus Status
        {
            get
            {
                TaskCommandResult toProcess = null;
                if (results.TryDequeue(out toProcess))
                {
                    int? exitCode = null;
                    if (commandsCompleted)
                    {
                        exitCode = toProcess.ExitCode;
                    }
                    return new TaskCommandStatus(exitCode, toProcess.Stdout, toProcess.Stderr);
                }
                else
                {
                    return null;
                }
            }
        }

        public Task RunAsync()
        {
            runningAsync = true;
            return DoRunAsync();
        }

        public void Cancel()
        {
            cts.Cancel();
        }

        public IJobResult Run()
        {
            IJobResult jobResult = null;

            // TODO
            if (request.Privileged == false)
            {
                // TODO IMPERSONATION!
            }

            var commandFactory = new TaskCommandFactory(container);

            foreach (TaskCommandDTO cmd in commands)
            {
                if (cts.IsCancellationRequested)
                {
                    break;
                }

                TaskCommand taskCommand = commandFactory.Create(cmd.Command, cmd.Args);
                try
                {
                    TaskCommandResult result = taskCommand.Execute();
                    results.Enqueue(result);
                }
                catch (Exception ex)
                {
                    results.Enqueue(new TaskCommandResult(1, null, ex.Message));
                    break;
                }
            }

            commandsCompleted = true;

            if (!runningAsync)
            {
                jobResult = FlattenResults();
            }

            return jobResult;
        }

        private Task DoRunAsync()
        {
            // TODO
            if (request.Privileged == false)
            {
                // TODO IMPERSONATION!
            }

            var commandFactory = new TaskCommandFactory(container);

            var taskCommands = new List<TaskCommand>();
            var tasks = new List<Task>();
            foreach (TaskCommandDTO cmd in commands)
            {
                if (cts.IsCancellationRequested)
                {
                    break;
                }

                TaskCommand taskCommand = commandFactory.Create(cmd.Command, cmd.Args);
                taskCommand.ResultAvailable += taskCommand_ResultAvailable;

                taskCommands.Add(taskCommand);
                var asyncTask = taskCommand.ExecuteAsync();
                tasks.Add(taskCommand.ExecuteAsync());
            }

            return Task.WhenAll(tasks).ContinueWith((task) =>
                {
                    foreach (var tc in taskCommands)
                    {
                        tc.ResultAvailable -= taskCommand_ResultAvailable;
                    }
                });
        }

        private void taskCommand_ResultAvailable(object sender, TaskCommandResultEventArgs e)
        {
            if (!runningAsync)
            {
                throw new InvalidOperationException("Trying to raise event in non-async mode!");
            }

            TaskCommandResult result = e.Result;

            if (JobStatusAvailable == null)
            {
                log.Trace("taskCommand_ResultAvailable enqueuing '{0}'", result.GetType());
                results.Enqueue(result); // TODO: what if too many results??
            }
            else
            {
                IJobStatus jobStatus = ToStatusWithEnqueued(result);
                log.Trace("taskCommand_ResultAvailable raising event '{0}'", jobStatus.Data);
                JobStatusAvailable(this, new JobStatusEventArgs(jobStatus));
            }
        }

        private IJobStatus ToStatusWithEnqueued(TaskCommandResult taskCommandResult)
        {
            if (taskCommandResult == null)
            {
                throw new ArgumentNullException("taskCommandResult");
            }

            TaskCommandStatus status = null;

            if (results.IsEmpty)
            {
                status = new TaskCommandStatus(null, taskCommandResult.Stdout, taskCommandResult.Stderr);
            }
            else
            {
                results.Enqueue(taskCommandResult);
                IJobResult flattened = FlattenResults();
                status = new TaskCommandStatus(null, flattened.Stdout, flattened.Stderr); // TODO: LAST RESULT EXIT CODE
            }

            return status;
        }

        private IJobResult FlattenResults()
        {
            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            int lastExitCode = 0;

            TaskCommandResult rslt;
            while (results.TryDequeue(out rslt))
            {
                stdout.SmartAppendLine(rslt.Stdout);
                stderr.SmartAppendLine(rslt.Stderr);
                if (rslt.ExitCode != 0)
                {
                    lastExitCode = rslt.ExitCode;
                    break;
                }
            }

            return new TaskCommandResult(lastExitCode, stdout.ToString(), stderr.ToString());
        }
    }
}
