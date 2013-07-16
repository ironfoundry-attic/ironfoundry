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
    public class JobObjectService : IJobObjectService, IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<int, IJobObjectServiceCallback> clients = new Dictionary<int, IJobObjectServiceCallback>();
        private readonly JobObject jobObject = new JobObject(Environment.UserName);
        private readonly List<Process> processes = new List<Process>();

        public JobObjectService()
        {
            jobObject.KillProcessesOnJobClose = true;
        }

        #region Client Management
        public void RegisterJobClient(int processID)
        {
            log.Info("Registering client process {0}...", processID);

            var callback = OperationContext.Current.GetCallbackChannel<IJobObjectServiceCallback>();

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

        public void UnregisterJobClient(int processID)
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

        private static void CloseCallbackChannel(IJobObjectServiceCallback callback)
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
        public void SetJobLimits(JobObjectLimits limits)
        {
            try
            {
                jobObject.JobMemoryLimit = limits.MemoryMB * 1024 * 1024;
                jobObject.JobCpuLimit = limits.CpuPercent;
            }
            catch (Exception ex)
            {
                log.ErrorException("Error setting job limits", ex);
                //MulticastClientNotify(NotifyServiceMessage, String.Format("Unable to set job limits: {0}", ex));
                MulticastClientNotify(NotifyServiceMessage, String.Format("Unable to set job limits: {0}", ex.Message));
            }
        }

        public void StartProcess(string fileName, string workingDirectory, string args)
        {
            try
            {
                var process = new BackgroundProcess(workingDirectory, fileName, args);
                processes.Add(process);
                process.StartInfo.LoadUserProfile = true;
                process.ErrorDataReceived += ProcessOnErrorDataReceived;
                process.OutputDataReceived += ProcessOnOutputDataReceived;
                process.Exited += ProcessOnExited;
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                jobObject.AddProcess(process);
            }
            catch (Exception ex)
            {
                log.ErrorException("Unable to start process", ex);
                MulticastClientNotify(NotifyServiceMessage, String.Format("Unable to start process {0}: {1}", fileName, ex.Message));
                MulticastClientNotify(NotifyProcessExited, -1);
            }
        }

        public void StopProcess(int processID)
        {
            var process = processes.FirstOrDefault(p => p.Id == processID);
            if (process == null)
            {
                MulticastClientNotify(NotifyServiceMessage, String.Format("Process ID {0} is not controlled by this job.", processID));
            }
            else
            {
                try
                {
                    process.Kill();
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
                return processes.Select(p => new ProcessInfo
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
                MulticastClientNotify(NotifyProcessExited, process.ExitCode);
                processes.Remove(process);
            }
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            MulticastClientNotify(NotifyOutputReceived, dataReceivedEventArgs.Data);
        }

        private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            MulticastClientNotify(NotifyErrorReceived, dataReceivedEventArgs.Data);
        }

        private static void NotifyServiceMessage(IJobObjectServiceCallback callback, string message)
        {
            callback.ServiceMessageReceived(message);
        }

        private static void NotifyOutputReceived(IJobObjectServiceCallback callback, string output)
        {
            callback.ProcessOutputReceived(output);
        }

        private static void NotifyErrorReceived(IJobObjectServiceCallback callback, string error)
        {
            callback.ProcessErrorReceived(error);
        }

        private static void NotifyProcessExited(IJobObjectServiceCallback callback, int exitCode)
        {
            callback.ProcessExit(exitCode);
        }

        private void MulticastClientNotify<T>(Action<IJobObjectServiceCallback, T> action, T arg)
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

        public void Dispose()
        {
            foreach (var process in processes)
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

            if (jobObject != null)
            {
                jobObject.Dispose();
            }
        }
    }
}
