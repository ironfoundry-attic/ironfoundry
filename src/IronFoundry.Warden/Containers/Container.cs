namespace IronFoundry.Warden.Containers
{
    using System;
    using System.Threading;

    public abstract class Container
    {
        private readonly ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim();
        private readonly ContainerHandle handle;
        private readonly ContainerUser user;
        private readonly ContainerDirectory directory;

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

        public ContainerHandle Handle
        {
            get { return handle; }
        }

        public string ContainerPath
        {
            get { return directory.ToString(); }
        }

        public string PathWithin(string path)
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

        public void Destroy()
        {
            rwlock.EnterWriteLock();
            try
            {
                user.Delete();
                directory.Delete();
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }
    }
}
