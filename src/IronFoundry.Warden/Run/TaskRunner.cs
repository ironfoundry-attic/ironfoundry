namespace IronFoundry.Warden.Run
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using IronFoundry.Warden.Containers;
    using IronFoundry.Warden.Jobs;
    using IronFoundry.Warden.Protocol;
    using Newtonsoft.Json;

    public class TaskRunner : IJobRunnable
    {
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

        public Task<IJobResult> RunAsync()
        {
            runningAsync = true;
            return Task.Factory.StartNew<IJobResult>(Run);
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
