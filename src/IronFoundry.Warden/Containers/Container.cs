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
    using Utilities.JobObjects;

    public abstract class Container
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim();
        private readonly ContainerHandle handle;
        private readonly ContainerUser user;
        private readonly ContainerDirectory directory;

        // http://msdn.microsoft.com/en-us/library/windows/desktop/ms684161(v=vs.85).aspx
        private readonly JobObject jobObject = new JobObject();

        public Container(string handle)
        {
            this.handle = new ContainerHandle(handle);
            this.user = new ContainerUser(handle);
            this.directory = new ContainerDirectory(this.handle, this.user);
        }

        public Container()
        {
            this.handle = new ContainerHandle();
            this.user = new ContainerUser(handle, true);
            this.directory = new ContainerDirectory(this.handle, this.user, true);
        }

        public NetworkCredential GetCredential()
        {
            return user.GetCredential();
        }

        public ContainerHandle Handle
        {
            get { return handle; }
        }

        public string ContainerPath
        {
            get { return directory.ToString(); }
        }

        public IEnumerable<string> ConvertToPathsWithin(string[] arguments)
        {
            foreach (string arg in arguments)
            {
                string rv = null;

                if (arg.Contains("@ROOT@"))
                {
                    rv = arg.Replace("@ROOT@", this.ContainerPath).ToWinPathString();
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
                return pathTmp.Replace("@ROOT@", this.ContainerPath).ToWinPathString();
            }
            else
            {
                return pathTmp;
            }
        }

        public TempFile TempFileInContainer(string extension)
        {
            return new TempFile(this.ContainerPath, extension);
        }

        public void Destroy()
        {
            rwlock.EnterWriteLock();
            try
            {
                user.Delete();
                directory.Delete();
                jobObject.Dispose();
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }

        public void AddProcess(Process process, ResourceLimits rlimits)
        {
            if (!jobObject.HasProcess(process))
            {
                try
                {
                    jobObject.AddProcess(process);
                    if (rlimits != null)
                    {
                        jobObject.JobMemoryLimit = rlimits.JobMemoryLimit;
                        jobObject.JobUserTimeLimit = TimeSpan.FromSeconds(rlimits.Cpu);
                    }
                    // TODO
                    // rlimits.Nice;
                    // DISK QUOTA!
                }
                catch (Win32Exception e)
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                    log.WarnException(String.Format("Process with ID {0} could not be added to the job object in container: {1}", process.Id, handle), e);
                    throw;
                }
            }
        }
    }
}
