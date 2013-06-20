namespace IronFoundry.Warden.Containers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using NLog;
    using Protocol;
    using Utilities;

    public class Container
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim();
        private readonly ContainerHandle handle;
        private readonly ContainerUser user;
        private readonly ContainerDirectory directory;

        // http://blogs.msdn.com/b/alejacma/archive/2012/08/19/10280416.aspx
        // http://msdn.microsoft.com/en-us/library/windows/desktop/ms684161(v=vs.85).aspx
        private readonly JobObject jobObject;
        private readonly Dictionary<int, Process> processes = new Dictionary<int, Process>();

        private ContainerPort port;
        private ContainerState state;

        public Container(string handle, ContainerState containerState)
        {
            if (handle.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("handle");
            }
            this.handle = new ContainerHandle(handle);

            if (containerState == null)
            {
                throw new ArgumentNullException("containerState");
            }
            this.state = containerState;

            this.user = new ContainerUser(handle);
            this.directory = new ContainerDirectory(this.handle, this.user);

            this.jobObject = GetJobObject(handle);
        }

        public Container()
        {
            this.handle = new ContainerHandle();
            this.user = new ContainerUser(handle, shouldCreate: true);
            this.directory = new ContainerDirectory(this.handle, this.user, true);
            this.state = ContainerState.Born;

            this.jobObject = GetJobObject(handle);
        }

        public NetworkCredential GetCredential()
        {
            return user.GetCredential();
        }

        public ContainerHandle Handle
        {
            get { return handle; }
        }

        public ContainerUser User
        {
            get { return user; }
        }

        public ContainerState State
        {
            get { return state; }
        }

        public ContainerDirectory Directory
        {
            get { return directory; }
        }

        public void AfterCreate()
        {
            this.state = ContainerState.Active;
        }

        public void Stop()
        {
            KillProcesses();
        }

        public void AfterStop()
        {
            this.state = ContainerState.Stopped;
        }

        public ContainerPort ReservePort(ushort suggestedPort)
        {
            rwlock.EnterWriteLock();
            try
            {
                this.port = new ContainerPort(suggestedPort, this.user);
                return this.port;
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }

        public IEnumerable<string> ConvertToPathsWithin(string[] arguments)
        {
            foreach (string arg in arguments)
            {
                string rv = null;

                if (arg.Contains("@ROOT@"))
                {
                    rv = arg.Replace("@ROOT@", this.Directory).ToWinPathString();
                }
                else
                {
                    rv = arg;
                }

                yield return rv;
            }
        }

        public string ConvertToPathWithin(string path)
        {
            string pathTmp = path.Trim();
            if (pathTmp.StartsWith("@ROOT@"))
            {
                return pathTmp.Replace("@ROOT@", this.Directory).ToWinPathString();
            }
            else
            {
                return pathTmp;
            }
        }

        public TempFile TempFileInContainer(string extension)
        {
            return new TempFile(this.Directory, extension);
        }

        public static void CleanUp(string handle)
        {
            ContainerUser.CleanUp(handle);
            ContainerDirectory.CleanUp(handle);
            ContainerPort.CleanUp(handle);
        }

        public void Destroy()
        {
            rwlock.EnterWriteLock();
            try
            {
                user.Delete();

                directory.Delete();

                if (port != null)
                {
                    port.Delete();
                }

                KillProcesses();

                jobObject.Dispose();

                this.state = ContainerState.Destroyed;
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }

        public void AddProcess(Process process, ResourceLimits rlimits)
        {
            try
            {
                rwlock.EnterWriteLock();

                if (!process.HasExited)
                {
                    process.Exited += process_Exited; // TODO not necessary when job objects work
                    processes.Add(process.Id, process);

                    if (!jobObject.HasProcess(process))
                    {
                        try
                        {
                            jobObject.AddProcess(process);
                            /*
                             * TODO
                            if (rlimits != null)
                            {
                                jobObject.JobMemoryLimit = rlimits.JobMemoryLimit;
                                jobObject.JobUserTimeLimit = TimeSpan.FromSeconds(rlimits.Cpu);
                            }
                            // rlimits.Nice;
                            // DISK QUOTA!
                             */
                        }
                        catch (Win32Exception e)
                        {
                            log.ErrorException(
                                String.Format("Error adding PID {0} to job object in container '{1}'. Error code: '{2}' Native error code: '{3}' HasExited: '{4}'",
                                    process.Id, handle, e.ErrorCode, e.NativeErrorCode, process.HasExited), e);
                        }
                    }
                }
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }

        /// <summary>
        /// TODO not necessary when job objects work
        /// </summary>
        private void process_Exited(object sender, EventArgs e)
        {
            var process = (Process)sender;
            process.Exited -= process_Exited;
            log.Trace("Container process exited PID '{0}' exit code '{1}'", process.Id, process.ExitCode);
        }

        private void KillProcesses()
        {
            jobObject.TerminateProcesses(0);
            // TODO once job objects are working, we shouldn't need this.
            var processList = processes.Values.ToListOrNull();
            foreach (Process process in processList)
            {
                try
                {
                    processes.Remove(process.Id);
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
                catch { }
            }
        }

        private static JobObject GetJobObject(string jobObjectName)
        {
            var jobObject = new JobObject(jobObjectName);
            jobObject.ActiveProcessesLimit = 64;
            jobObject.DieOnUnhandledException = true;
            jobObject.KillProcessesOnJobClose = true;
            return jobObject;
        }
    }
}
