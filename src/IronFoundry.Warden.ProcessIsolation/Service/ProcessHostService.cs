namespace IronFoundry.Warden.ProcessIsolation.Service
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.ServiceModel;
    using System.Threading.Tasks;
    using NLog;
    using Native;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ProcessHostService : IProcessHostService, IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<int, IProcessHostClientCallback> clients = new Dictionary<int, IProcessHostClientCallback>();
        private readonly JobObject parentJobObject = new JobObject(Environment.UserName);
        private readonly List<JobProcess> childProcesses = new List<JobProcess>();

        public ProcessHostService()
        {
            parentJobObject.KillProcessesOnJobClose = true;
        }

        #region Client Management
        public void RegisterClient(int processID)
        {
            log.Info("Registering client process {0}...", processID);

            var callback = OperationContext.Current.GetCallbackChannel<IProcessHostClientCallback>();

            lock (clients)
            {
                if (false == clients.ContainsKey(processID))
                {
                    log.Info("Client process '{0}' registered.", processID);
                    clients.Add(processID, callback);
                }
                else
                {
                    clients[processID] = callback;
                    log.Info("Client '{0}' is renewing its registration.", processID);
                }

                WireUpCallbackChannel(callback as ICommunicationObject, processID);
            }
        }

        public void UnregisterClient(int processID)
        {
            lock (clients)
            {
                log.Info("Client process '{0}' is unregistering...", processID);
                RemoveAndClose(processID);
                log.Info("Client process '{0}' is unregistered.", processID);
            }
        }

        private void WireUpCallbackChannel(ICommunicationObject callback, int processID)
        {
            if (callback == null) return;

            callback.Closing += delegate
            {
                lock (clients)
                {
                    RemoveAndClose(processID);
                }
                log.Warn("Client {0} has disconnected.", processID);
            };
            callback.Faulted += delegate
            {
                lock (clients)
                {
                    RemoveAndClose(processID);
                }
            };
        }

        private static bool RemoveAndClose(int clientID)
        {
            try
            {
                if (clients.ContainsKey(clientID))
                {
                    var client = clients[clientID];
                    clients.Remove(clientID);

                    CloseCallbackChannel(client);
                }
            }
            catch(Exception ex)
            {
                log.DebugException("Error closing channel", ex);
            }

            return true;
        }

        private static void CloseCallbackChannel(IProcessHostClientCallback callback)
        {
            var channel = callback as ICommunicationObject;
            if (channel != null)
            {
                try
                {
                    if (channel.State == CommunicationState.Faulted)
                    {
                        channel.Abort();
                    }
                    else
                    {
                        channel.Close();
                    }
                }
                catch
                {
                    channel.Abort();
                }
            }
        }

        #endregion

        // ref: http://msdn.microsoft.com/en-us/library/windows/desktop/ms684141(v=vs.85).aspx
        public void SetProcessLimits(int processID, ResourceLimits limits)
        {
            try
            {
                var processJob = childProcesses.FirstOrDefault(pbj => pbj.Process.Id == processID);
                if (processJob != null)
                {
                    processJob.JobObject.JobMemoryLimit = limits.MemoryMB * 1024 * 1024;
                    processJob.JobObject.JobCpuLimit = limits.CpuPercent;
                }
            }
            catch (Exception ex)
            {
                log.ErrorException("Error setting job limits", ex);
                MulticastClientNotify(NotifyServiceMessage, String.Format("Unable to set job limits: {0}", ex.Message));
            }
        }

        public void SetJobLimits(ResourceLimits limits)
        {
            try
            {
                parentJobObject.JobMemoryLimit = limits.MemoryMB * 1024 * 1024;
                parentJobObject.JobCpuLimit = limits.CpuPercent;
            }
            catch (Exception ex)
            {
                log.ErrorException("Error setting job limits", ex);
                MulticastClientNotify(NotifyServiceMessage, String.Format("Unable to set job limits: {0}", ex.Message));
            }
        }

        public int StartProcess(string fileName, string workingDirectory, string args)
        {
            int pid = -1;

            try
            {
                var process = new BackgroundProcess(workingDirectory, fileName, args);
                process.StartInfo.LoadUserProfile = true;
                process.ErrorDataReceived += ProcessOnErrorDataReceived;
                process.OutputDataReceived += ProcessOnOutputDataReceived;
                process.Exited += ProcessOnExited;
                process.Start();

                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                pid = process.Id;
                var jobProcess = new JobProcess
                {
                    JobObject = new JobObject(String.Concat(Environment.UserName, "_", pid.ToString())),
                    Process = process
                };
                parentJobObject.AddProcess(process); // add to parent job object first
                jobProcess.JobObject.AddProcess(process); // add to child job to create hierarchy
            }
            catch (Exception ex)
            {
                log.ErrorException("Unable to start process", ex);
                MulticastClientNotify(NotifyServiceMessage, String.Format("Unable to start process {0}: {1}", fileName, ex.Message));
                MulticastClientNotify(NotifyProcessExited, pid, -1);
            }

            return pid;
        }

        public void StopProcess(int processID)
        {
            var processJob = childProcesses.FirstOrDefault(p => p.Process.Id == processID);
            if (processJob == null)
            {
                MulticastClientNotify(NotifyServiceMessage, String.Format("Process ID {0} is not controlled by this job.", processID));
            }
            else
            {
                try
                {
                    if (!processJob.Process.HasExited)
                    {
                        processJob.Process.Kill();
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorException(String.Format("Error killing process {0} on client's request.", processID), ex);
                    MulticastClientNotify(NotifyServiceMessage, String.Format("Error killing process {0}: {1}.", processID, ex.Message));
                }
            }
        }

        public List<ProcessInfo> ListProcesses()
        {
            try
            {
                return childProcesses.Select(pbj => pbj.Process).Select(p => new ProcessInfo
                {
                    ID = p.Id,
                    Name = p.ProcessName
                }).ToList();
            }
            catch (Exception ex)
            {
                log.ErrorException("Error getting process list.", ex);
                MulticastClientNotify(NotifyServiceMessage, String.Format("Error getting process list: {0}.", ex.Message));
                return new List<ProcessInfo>(0);
            }
        }

        private void ProcessOnExited(object sender, EventArgs eventArgs)
        {
            var process = (Process) sender;
            if (process != null)
            {
                process.ErrorDataReceived -= ProcessOnErrorDataReceived;
                process.OutputDataReceived -= ProcessOnOutputDataReceived;
                process.Exited -= ProcessOnExited;
                MulticastClientNotify(NotifyProcessExited, process.Id, process.ExitCode);
                childProcesses.RemoveAll(pbj => pbj.Process.Id == process.Id);
            }
        }

        private int GetPID(Process process)
        {
            return process == null ? -1 : process.Id;
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            var pid = GetPID(sender as Process);
            MulticastClientNotify(NotifyOutputReceived, pid, dataReceivedEventArgs.Data);
        }

        private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            var pid = GetPID(sender as Process);
            MulticastClientNotify(NotifyErrorReceived, pid, dataReceivedEventArgs.Data);
        }

        private static void NotifyServiceMessage(IProcessHostClientCallback callback, string message)
        {
            callback.ServiceMessageReceived(message);
        }

        private static void NotifyOutputReceived(IProcessHostClientCallback callback, int pid, string output)
        {
            callback.ProcessOutputReceived(pid, output);
        }

        private static void NotifyErrorReceived(IProcessHostClientCallback callback, int pid, string error)
        {
            callback.ProcessErrorReceived(pid, error);
        }

        private static void NotifyProcessExited(IProcessHostClientCallback callback, int pid, int exitCode)
        {
            callback.ProcessExit(pid, exitCode);
        }

        private void MulticastClientNotify<T>(Action<IProcessHostClientCallback, T> action, T arg)
        {
            var invalidClients = new List<int>();

            lock (clients)
            {
                Parallel.ForEach(clients.Keys, key =>
                {
                    try
                    {
                        action(clients[key], arg);
                    }
                    catch (Exception)
                    {
                        invalidClients.Add(key); // consume it and mark client as invalid
                    }
                });

                invalidClients.All(RemoveAndClose);
            }
        }

        private void MulticastClientNotify<T>(Action<IProcessHostClientCallback, int, T> action, int pid, T arg)
        {
            var invalidClients = new List<int>();

            lock (clients)
            {
                Parallel.ForEach(clients.Keys, key =>
                {
                    try
                    {
                        action(clients[key], pid, arg);
                    }
                    catch (Exception)
                    {
                        invalidClients.Add(key); // consume it and mark client as invalid
                    }
                });

                invalidClients.All(RemoveAndClose);
            }
        }

        public void Dispose()
        {
            foreach (var process in childProcesses.Select(pbj => pbj.Process))
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        process.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    log.WarnException(String.Format("Unable to kill process {0}.", process.Handle), ex);
                }
            }

            if (parentJobObject != null)
            {
                parentJobObject.Dispose();
            }
        }

        private class JobProcess
        {
            public Process Process { get; set; }
            public JobObject JobObject { get; set; }
        }
    }
}
