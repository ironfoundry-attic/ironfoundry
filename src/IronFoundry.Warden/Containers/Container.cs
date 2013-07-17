namespace IronFoundry.Warden.Containers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;
    using NLog;
    using ProcessIsolation.Client;
    using ProcessIsolation.Service;
    using Protocol;
    using Utilities;
    using ResourceLimits = ProcessIsolation.Service.ResourceLimits;

    public class Container
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim();
        private readonly ContainerHandle handle;
        private readonly ContainerUser user;
        private readonly ContainerDirectory directory;
        private readonly ContainerProcessIORouter processIORouter;
        private readonly ProcessHostManager processHostManager;
        private readonly IProcessHostClient processHostClient;

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
            state = containerState;

            user = new ContainerUser(handle);
            directory = new ContainerDirectory(this.handle, user);

            processHostManager = CreateProcessHostManager();
            processHostClient = new ProcessHostClient(this.handle.ToString());
            processIORouter = new ContainerProcessIORouter(processHostClient);

            if (state == ContainerState.Active)
            {
                RestoreProcesses();
            }
        }

        public Container()
        {
            handle = new ContainerHandle();
            user = new ContainerUser(handle, shouldCreate: true);
            directory = new ContainerDirectory(handle, user, true);
            state = ContainerState.Born;

            processHostManager = CreateProcessHostManager();
            processHostClient = new ProcessHostClient(this.handle.ToString());
            processIORouter = new ContainerProcessIORouter(processHostClient);
        }

        private ProcessHostManager CreateProcessHostManager()
        {
            // note: for debug build, the IF.Warden.ProcessIsolationService output is post build event copied (and renamed)
            //       to the IF.Warden.Service output directory /PisoSvc/PisoSvc.exe
            // note: the warden service installer does this as well
            var hostDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PisoSvc");
            return new ProcessHostManager(hostDirectory, directory.Path, "pisosvc.exe", handle, GetCredential());
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

        public bool HasProcesses
        {
            get
            {
                rwlock.EnterReadLock();
                try
                {
                    processHostClient.Register();
                    return processHostClient.ListProcesses().Count > 0;
                }
                finally
                {
                    rwlock.ExitReadLock();
                }
            }
        }

        public void AfterCreate()
        {
            ChangeState(ContainerState.Active);
        }

        public void Stop()
        {
            processHostClient.Register();
            foreach (var process in processHostClient.ListProcesses())
            {
                processHostClient.StopProcess(process.ID);
            }
        }

        public void AfterStop()
        {
            ChangeState(ContainerState.Stopped);
        }

        public ContainerPort ReservePort(ushort suggestedPort)
        {
            rwlock.EnterUpgradeableReadLock();
            try
            {
                if (port == null)
                {
                    rwlock.EnterWriteLock();
                    try
                    {
                        port = new ContainerPort(suggestedPort, this.user);
                    }
                    finally
                    {
                        rwlock.ExitWriteLock();
                    }
                }
                else
                {
                    log.Trace("Container '{0}' already assigned port '{1}'", handle, port);
                }
            }
            finally
            {
                rwlock.ExitUpgradeableReadLock();
            }

            return port;
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
            ProcessHostManager.Cleanup(handle, new ContainerDirectory(new ContainerHandle(handle), null).ToString());
            ContainerUser.CleanUp(handle);
            ContainerDirectory.CleanUp(handle);
            ContainerPort.CleanUp(handle, 0); // TODO
        }

        public void Destroy()
        {
            // TODO: fix this so that one cleanup doesn't screw the rest of them by throwing - BGH
            rwlock.EnterWriteLock();
            try
            {
                if (processIORouter != null)
                {
                    processIORouter.Dispose();
                }
                if (processHostClient != null)
                {
                    processHostClient.Unregister();
                }
                if (processHostManager != null)
                {
                    processHostManager.Dispose();
                }
                if (port != null)
                {
                    port.Delete(user);
                }

                directory.Delete();
                user.Delete();

                this.state = ContainerState.Destroyed;
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }

        public int StartProcess(string fileName, string workingDirectory, string args, Protocol.ResourceLimits resourceLimits,
            Action<string> onOutput, Action<string> onError, Action<int> onExit)
        {
            processHostClient.Register();
            var pid = processHostClient.StartProcess(fileName, workingDirectory, args);
            processIORouter.AddProcessIO(pid, onOutput, onError, onExit);
            processHostClient.SetProcessLimits(pid, new ResourceLimits { MemoryMB = (uint)resourceLimits.JobMemoryLimit });
            return pid;
        }

        private void RestoreProcesses()
        {
            processHostManager.RunService();
            processHostClient.Register();

            // TODO: the way the system is architected (ProcessCommand handles the IO vs passing through)
            // there is no way to restore the async IO, so we basically wire up the processes again for tracking
            // purposes but no async IO will happen.  Need to investigate a better solution for this.
            //foreach (var process in processHostClient.ListProcesses())
            //{
            //    processIORouter.AddProcessIO(process.ID, onOutput, onError, onExit);
            //}
        }

        private void ChangeState(ContainerState containerState)
        {
            rwlock.EnterWriteLock();
            try
            {
                this.state = containerState;
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }
    }
}
