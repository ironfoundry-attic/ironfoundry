namespace IronFoundry.Warden.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Containers;
    using NLog;

    public class ProcessManager
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentDictionary<int, Process> processes = new ConcurrentDictionary<int, Process>();
        private readonly ContainerUser containerUser;

        public ProcessManager(ContainerUser containerUser)
        {
            if (containerUser == null)
            {
                throw new ArgumentNullException("containerUser");
            }
            this.containerUser = containerUser;
        }

        public void AddProcess(Process process)
        {
            if (processes.TryAdd(process.Id, process))
            {
                process.Exited += process_Exited;
            }
            else
            {
                throw new InvalidOperationException(
                    String.Format("Process '{0}' already added to process manager for user '{1}'", process.Id, containerUser));
            }
        }

        public void StopProcesses()
        {
            // TODO once job objects are working, we shouldn't need this.
            var processList = processes.Values.ToListOrNull();
            foreach (Process process in processList)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                    Process removed;
                    processes.TryRemove(process.Id, out removed);
                }
                catch (Exception ex)
                {
                    log.WarnException(ex);
                }
            }

            string processUser = null;
            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    processUser = process.GetUserName();
                    if (processUser == containerUser && !process.HasExited)
                    {
                        process.Kill();
                    }
                }
                catch (Exception ex)
                {
                    log.WarnException(ex);
                }
            }
        }

        private void process_Exited(object sender, EventArgs e)
        {
            var process = (Process)sender;
            process.Exited -= process_Exited;
            log.Trace("Process exited PID '{0}' exit code '{1}'", process.Id, process.ExitCode);
        }
    }
}
